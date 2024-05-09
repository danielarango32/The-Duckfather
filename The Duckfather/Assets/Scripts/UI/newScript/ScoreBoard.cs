using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class ScoreBoard : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform container;
    [SerializeField] ScoreBoardItem scoreBoardItemPrefab;
    [SerializeField] CanvasGroup canvasGroup;
    
    Dictionary<Player,ScoreBoardItem> scoreBoardItems = new Dictionary<Player, ScoreBoardItem>();
    
    void Start()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            addScoreBoardItem(player);
        }
    }
    
    void addScoreBoardItem(Player player)
    {
        ScoreBoardItem item = Instantiate(scoreBoardItemPrefab, container).GetComponent<ScoreBoardItem>();
        item.Initialize(player);
        scoreBoardItems.Add(player, item);
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        addScoreBoardItem(newPlayer);
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
      RemoveScoreBoardItem(otherPlayer);
    }
    
    void RemoveScoreBoardItem(Player Player)
    {
        Destroy(scoreBoardItems[Player].gameObject);
        scoreBoardItems.Remove(Player);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
