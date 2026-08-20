using UnityEngine;

/// <summary>
/// Borra el efecto cuando termina de reproducirse.
///
/// Antes comparaba `tiempo == particulas.main.duration` con ==, cosa que en
/// coma flotante practicamente nunca se cumple: los VFX no se destruian nunca
/// y, como se crean con PhotonNetwork.Instantiate desde ShootinController, se
/// acumulaban en todos los clientes durante toda la partida.
/// </summary>
public class VFX_Destroyer : MonoBehaviour
{
    [Tooltip("Vida en segundos si el sistema de particulas no la define")]
    [SerializeField] private float vidaPorDefecto = 5f;

    [Tooltip("Segundos transcurridos desde que aparecio el efecto")]
    public float tiempo;

    private ParticleSystem particulas;
    private float vidaTotal;
    private bool destruccionPedida;

    private void Awake()
    {
        // Se resolvia con GetComponent dos veces por frame dentro de Update().
        particulas = GetComponent<ParticleSystem>();
        vidaTotal = CalcularVidaTotal();
    }

    private float CalcularVidaTotal()
    {
        if (particulas == null)
        {
            return vidaPorDefecto;
        }

        ParticleSystem.MainModule ajustes = particulas.main;
        float vida = ajustes.duration + ajustes.startLifetime.constantMax;

        return vida > 0f ? vida : vidaPorDefecto;
    }

    private void Update()
    {
        if (destruccionPedida)
        {
            return;
        }

        tiempo += Time.deltaTime;

        if (tiempo < vidaTotal)
        {
            return;
        }

        destruccionPedida = true;
        NetworkCleanup.Remove(gameObject);
    }
}
