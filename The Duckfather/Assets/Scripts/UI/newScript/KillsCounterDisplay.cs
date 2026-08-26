using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Numero de kills propias en el HUD (seccion "Puntaje" de UI.prefab), junto
/// al contador de muertes. Va en el mismo GameObject que el TMP_Text y lo
/// resuelve con GetComponent<TMP_Text>() en Awake(), igual que
/// DeathsCounterDisplay, para no depender de una referencia serializada entre
/// prefabs anidados.
///
/// A diferencia de una muerte, una kill no destruye ni recrea el controller
/// propio: LifeManager.AcreditarKill() corre en el cliente del atacante sin
/// tocar su propio pato. Por eso este componente no puede apoyarse en un
/// Start() fresco tras cada kill como hace DeathsCounterDisplay, y en cambio
/// escucha OnPlayerPropertiesUpdate para refrescarse en vivo.
/// </summary>
public class KillsCounterDisplay : MonoBehaviourPunCallbacks
{
    private TMP_Text texto;

    private void Awake()
    {
        texto = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        Refrescar();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey("Kills"))
        {
            Refrescar();
        }
    }

    private void Refrescar()
    {
        int kills = 0;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Kills", out object valor))
        {
            kills = (int)valor;
        }

        texto.text = kills.ToString();
    }
}
