using Photon.Pun;
using UnityEngine;

/// <summary>
/// Borrado de objetos que pueden venir de PhotonNetwork.Instantiate o de un
/// Instantiate local.
///
/// Un objeto de red solo puede destruirlo su dueno, y hay que hacerlo con
/// PhotonNetwork.Destroy para que desaparezca en todos los clientes:
/// Object.Destroy lo borra solo en el que lo llama y lo deja huerfano en el
/// resto. Como en el proyecto conviven las dos formas de crear objetos, aqui
/// se decide cual toca en cada caso.
/// </summary>
public static class NetworkCleanup
{
    /// <summary>
    /// Borra el objeto por la via que corresponda. Si es un objeto de red que
    /// no nos pertenece no hace nada: el borrado llegara desde su dueno.
    /// </summary>
    public static void Remove(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        PhotonView view = target.GetComponent<PhotonView>();

        // InstantiationId == 0 significa que el objeto no salio de
        // PhotonNetwork.Instantiate (es de escena o se creo en local).
        if (view == null || view.InstantiationId == 0)
        {
            Object.Destroy(target);
            return;
        }

        if (view.IsMine)
        {
            PhotonNetwork.Destroy(target);
        }
    }
}
