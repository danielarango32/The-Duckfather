using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    private const int MaxRoomNameLength = 32;
    private const int MaxReconnectAttempts = 1;
    private const int TargetFrameRate = 60;
    private const string GameSceneName = "Online 3";

    private const string NoConnectionMessage =
        "Sin conexion con el servidor. Espera unos segundos e intentalo de nuevo.";

    public static Launcher instance;

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject PlayerListItemPrefab;
    [SerializeField] GameObject startGameButton;

    private int reconnectAttempts;
    private bool hasRequestedLobby;

    // Una operacion de red disparada por boton esta en curso. Sin esta guarda,
    // pulsar dos veces "empezar partida" encadenaba varios LoadLevel: Photon
    // deja huerfana la carga anterior y IsMessageQueueRunning se queda en false
    // para siempre, con lo que el cliente deja de enviar y recibir.
    private bool isTransitioning;

    // Photon entrega el listado de salas por deltas, asi que hay que
    // mantener el acumulado aqui y redibujar la UI desde el.
    private readonly Dictionary<string, RoomInfo> cachedRoomList =
        new Dictionary<string, RoomInfo>();
    private readonly List<GameObject> spawnedRoomListItems = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = TargetFrameRate;
        ConnectToMaster();
    }

    // Al volver de una partida el cliente sigue conectado al Master Server, y
    // ConnectUsingSettings() devuelve false en ese caso sin lanzar ningun callback:
    // el menu se quedaria colgado en "loading". Hay que cubrir los tres estados.
    private void ConnectToMaster()
    {
        if (PhotonNetwork.InLobby)
        {
            MenuManager.instance.OpenMenu("title");
            return;
        }

        if (PhotonNetwork.IsConnected)
        {
            StartCoroutine(RequestLobbyWhenReady());
            return;
        }

        Debug.Log("Connecting to Master");
        if (!PhotonNetwork.ConnectUsingSettings())
        {
            ShowError("No se pudo iniciar la conexion. Revisa tu red e intentalo de nuevo.");
        }
    }

    private IEnumerator RequestLobbyWhenReady()
    {
        while (PhotonNetwork.IsConnected && !PhotonNetwork.IsConnectedAndReady)
        {
            yield return null;
        }

        if (PhotonNetwork.IsConnectedAndReady)
        {
            RequestLobby();
        }
    }

    private void RequestLobby()
    {
        if (hasRequestedLobby || PhotonNetwork.InLobby)
        {
            return;
        }
        hasRequestedLobby = true;
        PhotonNetwork.JoinLobby();
    }

    private void ShowError(string message)
    {
        errorText.text = message;
        MenuManager.instance.OpenMenu("error");
    }

    /// <summary>
    /// Deja pasar una unica operacion de red a la vez. Devuelve false si ya
    /// hay otra en curso, sin tocar su estado: rechazar un clic de mas no debe
    /// desbloquear la transicion que ya esta corriendo.
    /// </summary>
    private bool TryBeginTransition()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Operacion de red ignorada: ya hay una en curso.");
            return false;
        }

        isTransitioning = true;
        return true;
    }

    /// <summary>
    /// Cierra la transicion. La llaman tanto los callbacks de exito como los
    /// de fallo: si un fallo no la cerrara, los botones quedarian mudos para
    /// el resto de la sesion.
    /// </summary>
    private void EndTransition()
    {
        isTransitioning = false;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        reconnectAttempts = 0;
        PhotonNetwork.AutomaticallySyncScene = true;
        RequestLobby();
    }

    public override void OnJoinedLobby()
    {
        hasRequestedLobby = false;
        EndTransition();

        // Photon reenvia el listado completo justo despues de entrar: si
        // quedara algo del lobby anterior se mezclarian salas ya inexistentes.
        ClearRoomList();

        MenuManager.instance.OpenMenu("title");
        Debug.Log("Joined Lobby");
    }

    public override void OnLeftLobby()
    {
        ClearRoomList();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected from Photon: " + cause);
        hasRequestedLobby = false;
        EndTransition();
        ClearRoomList();

        if (reconnectAttempts < MaxReconnectAttempts)
        {
            reconnectAttempts++;
            ShowError("Conexion perdida (" + cause + "). Reconectando...");
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        ShowError("Conexion perdida (" + cause + "). Reinicia el juego para volver a intentarlo.");
    }

    public void CreateRoom()
    {
        string roomName = roomNameInputField.text.Trim();

        if (roomName.Length == 0)
        {
            ShowError("Escribe un nombre para la sala.");
            return;
        }

        if (roomName.Length > MaxRoomNameLength)
        {
            ShowError("El nombre de la sala no puede pasar de " + MaxRoomNameLength + " caracteres.");
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            ShowError(NoConnectionMessage);
            return;
        }

        if (!TryBeginTransition())
        {
            return;
        }

        if (!PhotonNetwork.CreateRoom(roomName))
        {
            EndTransition();
            ShowError("No se pudo iniciar la creacion de la sala.");
            return;
        }

        MenuManager.instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
    {
        EndTransition();
        MenuManager.instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        EndTransition();
        Debug.Log("Failed to create room: " + message);
        ShowError("No se pudo crear la sala: " + message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        EndTransition();
        Debug.Log("Failed to join room: " + message);
        ShowError("No se pudo entrar en la sala: " + message);
    }

    public void StartGame()
    {
        if (!TryBeginTransition())
        {
            return;
        }

        // Ocultarlo ademas de la guarda: mientras Online 3 carga en segundo
        // plano esta escena sigue viva y el boton seguiria siendo clicable.
        startGameButton.SetActive(false);

        PhotonNetwork.LoadLevel(GameSceneName);
    }

    public void LeaveRoom()
    {
        if (!TryBeginTransition())
        {
            return;
        }

        if (!PhotonNetwork.LeaveRoom())
        {
            // No estabamos en una sala: OnLeftRoom no va a llegar, asi que hay
            // que cerrar la transicion aqui o los botones quedarian mudos.
            EndTransition();
            MenuManager.instance.OpenMenu("title");
            return;
        }

        MenuManager.instance.OpenMenu("loading");
    }

    public void LeaveFindRoom()
    {
        MenuManager.instance.OpenMenu("title");
    }

    public void JoinRoom(RoomInfo info)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            ShowError(NoConnectionMessage);
            return;
        }

        if (!TryBeginTransition())
        {
            return;
        }

        if (!PhotonNetwork.JoinRoom(info.Name))
        {
            EndTransition();
            ShowError("No se pudo iniciar la entrada a la sala.");
            return;
        }

        MenuManager.instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        EndTransition();
        MenuManager.instance.OpenMenu("title");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // roomList es un delta, no el listado completo. Reconstruir la UI
        // directamente desde el hacia desaparecer las salas que seguian
        // existiendo pero no habian cambiado en esta actualizacion.
        MergeRoomListDelta(roomList);
        RedrawRoomList();
    }

    private void MergeRoomListDelta(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];

            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
                continue;
            }

            cachedRoomList[info.Name] = info;
        }
    }

    private void RedrawRoomList()
    {
        // Se lleva la lista de lo instanciado en lugar de recorrer los hijos
        // de roomListContent: Destroy es diferido hasta el final del frame, y
        // recorrer hijos podia arrastrarse los items recien creados.
        for (int i = 0; i < spawnedRoomListItems.Count; i++)
        {
            Destroy(spawnedRoomListItems[i]);
        }
        spawnedRoomListItems.Clear();

        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject item = Instantiate(roomListItemPrefab, roomListContent);
            item.GetComponent<RoomListItem>().SetUp(info);
            spawnedRoomListItems.Add(item);
        }
    }

    private void ClearRoomList()
    {
        cachedRoomList.Clear();
        RedrawRoomList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New player entered room: " + newPlayer.NickName);
        Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }
}


