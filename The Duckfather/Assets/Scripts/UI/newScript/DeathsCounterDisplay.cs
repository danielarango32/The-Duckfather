using Photon.Pun;
using TMPro;
using UnityEngine;

/// <summary>
/// Numero de muertes propias en el HUD (seccion "Puntaje" de UI.prefab).
/// Va en el mismo GameObject que el TMP_Text para no depender de una
/// referencia serializada entre prefabs anidados (Pato N -> UI -> DeathsText).
///
/// Se lee una sola vez en Start(): PlayerManager.Die() deja escrita la
/// CustomProperty "deaths" del jugador local ANTES de crear el controller
/// nuevo, asi que el valor ya esta al dia desde el primer frame de cada
/// respawn y no hace falta escuchar OnPlayerPropertiesUpdate.
/// </summary>
public class DeathsCounterDisplay : MonoBehaviour
{
    private TMP_Text texto;

    private void Awake()
    {
        texto = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        int muertes = 0;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("deaths", out object valor))
        {
            muertes = (int)valor;
        }

        texto.text = muertes.ToString();
    }
}
