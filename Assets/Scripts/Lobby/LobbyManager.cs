using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform roomListContainer;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private TMP_Dropdown playerCountDropdown;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button refreshButton;



    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    private void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = true;

        //Despausamos el juego tras el loop inicial
        Time.timeScale = 1f;

        ShowCursor();

        createRoomButton.onClick.AddListener(CreateRoom);
        refreshButton.onClick.AddListener(RefreshRoomList);

        Debug.Log($"Start LobbyManager - Connected: {PhotonNetwork.IsConnected}, Ready: {PhotonNetwork.IsConnectedAndReady}, InLobby: {PhotonNetwork.InLobby}");

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (!PhotonNetwork.InLobby)
        {
            if(PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.JoinLobby();

            }
        }
        else
        {
            //Ya estamos en el lobby actualizar igual
            UpdateRoomListUI();
        }

        StartCoroutine(ReconnectIfStuck());

    }

    private IEnumerator ReconnectIfStuck()
    {
        yield return new WaitForSeconds(5f);

        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("No estaba conectado despues de 5 segundos, reconectando...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }


    void RefreshRoomList()
    {
        Debug.Log("Refrescando salas...");
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinLobby(); 
        }
    }

    void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado a MasterServer");

        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");

        }
    }




    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"No se pudo crear la sala: {message}");
    }


 
    public override void OnJoinedLobby()
    {
        Debug.Log("Unido al lobby");
        cachedRoomList.Clear();
        UpdateRoomListUI();


        //Si sos el master que acaba de volver del GameManager, crea nueva sala
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CountOfRooms == 0)
        {
            string newRoomName = "Arena 4x4" + Random.Range(1000, 9999);

            RoomOptions options = new RoomOptions
            {
                MaxPlayers = 4,
                IsVisible = true,
                IsOpen = true,
                EmptyRoomTtl = 60000,
                PlayerTtl = 10000
            };

            PhotonNetwork.CreateRoom(newRoomName, options);

           
        }
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                //Si la sala fue eliminada, la quitamos del cache
                cachedRoomList.Remove(room.Name);
            }
            else
            {
                //Si es nueva o actualizada, la agregamos o actualizamos
                cachedRoomList[room.Name] = room;
            }
        }

        UpdateRoomListUI();


    }



    void UpdateRoomListUI()
    {
        //Primero eliminamos las salas anteriores del UI
        foreach (Transform child in roomListContainer)
            Destroy(child.gameObject);

        //Creamos un item visual para cada sala
        foreach (var room in cachedRoomList.Values)
        {
            GameObject item = Instantiate(roomListItemPrefab, roomListContainer);
            TMP_Text text = item.GetComponentInChildren<TMP_Text>();
            text.text = $"{room.Name} [{room.PlayerCount}/{room.MaxPlayers}]";

            Button button = item.GetComponent<Button>();

            //deshabilitar botones de salas cerradas o en el delay de salir de una sala
            button.interactable = PhotonNetwork.InRoom == false && room.IsOpen && room.PlayerCount < room.MaxPlayers;

            if (!room.IsOpen || room.PlayerCount >= room.MaxPlayers)
            {
                text.color = Color.red; 
            }
            else
            {
                text.color = Color.blue; 
            }

            button.onClick.AddListener(() => JoinRoom(room.Name));
        }
    }


    void JoinRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Photon no está listo para unirse a una sala.");
            return;
        }

        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Esperando a salir de la sala anterior...");
            StartCoroutine(DelayedJoinRoom(roomName));
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
    }

    IEnumerator DelayedJoinRoom(string roomName)
    {
        while (PhotonNetwork.InRoom)
            yield return null;

        PhotonNetwork.JoinRoom(roomName);
    }

    void CreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Reconectando al Master Server...");
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        string roomName = roomNameInput.text;
        if (string.IsNullOrWhiteSpace(roomName)) return;

        byte maxPlayers = byte.Parse(playerCountDropdown.options[playerCountDropdown.value].text);

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true,
            EmptyRoomTtl = 60000, //Tiempo en milisegundos antes de que la sala se destruya si queda vacia
            PlayerTtl = 10000     //Tiempo para permitir reconexion de jugadores desconectados
        };

        PhotonNetwork.CreateRoom(roomName, options);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //Esperamos un frame para garantizar que todos hayan entrado correctamente
            StartCoroutine(LoadLoadingSceneWithDelay());
        }
    }




    IEnumerator LoadLoadingSceneWithDelay()
    {
        yield return new WaitForEndOfFrame();

        PhotonNetwork.LoadLevel("LoadingScene");
    }



    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("Fallo al unirse a sala: " + message);
    }


  

}

