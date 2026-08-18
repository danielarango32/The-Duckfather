using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreBoardItem : MonoBehaviourPunCallbacks
{
    public TMP_Text usernameText;
    public TMP_Text killsText;
    public TMP_Text deathsText;
    
    Player player;
    public void Initialize(Player player)
    {
        usernameText.text = player.NickName;
        this.player = player;
        UpdateStats();
        
    }

    void UpdateStats()
    {
        if (player.CustomProperties.TryGetValue("Kills",out object kills))
        {
            killsText.text = kills.ToString();
        }
        if (player.CustomProperties.TryGetValue("deaths",out object deaths))
        {
            deathsText.text = deaths.ToString();
        }
    }
    
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer == player)
        {
            if (changedProps.ContainsKey("kills") || changedProps.ContainsKey("deaths"))
            {
                this.UpdateStats();
            }
            
        }
    }
}
