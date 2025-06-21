using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class LoadingRoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI playersListText;
    [SerializeField] private TextMeshProUGUI countdownText;

    [SerializeField] private Button exitButton;

    //Cambiar para pruebas
    private const float totalCountdownTime = 3f;

    private bool countdownStarted = false;
    private double startTime;
    private bool joinedRoom = false;




    private void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = true;

        exitButton.onClick.AddListener(OnClickExit);

        Debug.Log("Start en LoadingRoomManager - Estoy en escena y conectado: " + PhotonNetwork.IsConnected + " InRoom: " + PhotonNetwork.InRoom);

        StartCoroutine(WaitUntilInRoom());

    }
    


    IEnumerator WaitUntilInRoom()
    {
        while (!PhotonNetwork.InRoom)
        {
            Debug.Log("Esperando a estar en la sala...");
            yield return null;
        }

        Debug.Log("Ya estamos en la sala. Jugadores: " + PhotonNetwork.CurrentRoom.PlayerCount);
        joinedRoom = true;
        UpdatePlayerListUI();

        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                StartCoroutine(StartCountdownAfterDelay(1f));
            }
            else
            {
                countdownText.text = "Waiting for more players...";
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado al Master");
    }


    public override void OnJoinedRoom()
    {
        Debug.Log(">> OnJoinedRoom llamado - Nombre: " + PhotonNetwork.NickName);
        joinedRoom = true;

        UpdatePlayerListUI();

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Soy el Master. Jugadores en sala: " + PhotonNetwork.CurrentRoom.PlayerCount);

            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                StartCoroutine(StartCountdownAfterDelay(1f));
            }
            else
            {
                countdownText.text = "Waiting for more players...";
            }
        }
    }



    public override void OnJoinRoomFailed(short returnCode, string message)
    {
       
            Debug.LogError("Fallo al unirse a la sala: " + message);
            Debug.LogError("Se redirige a MainMenu");
            SceneManager.LoadScene("LobbyScene");


    }

    private void OnClickExit()
    {

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Saliste de la sala");
        SceneManager.LoadScene("LobbyScene");
    }

    private void Update()
    {
        
        if (!joinedRoom || !countdownStarted) return;

        //Si ya estamos en la sala y la cuenta regresiva comenzo
        double timeElapsed = PhotonNetwork.Time - startTime;
        double timeLeft = totalCountdownTime - timeElapsed;

        countdownText.text = $"The game will start in: {Mathf.CeilToInt((float)timeLeft)}s";

        if (timeLeft <= 0)
        {
            countdownStarted = false;

            if (PhotonNetwork.IsMasterClient)
            {
                //Cerramos la sala al iniciar 
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                Debug.Log("Se inicia partida");

                PhotonNetwork.LoadLevel("GameScene");
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerListUI();

        if (PhotonNetwork.IsMasterClient)
        {
            if (!countdownStarted && PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                Debug.Log("Nuevo jugador, empezando cuenta regresiva");
                StartCoroutine(StartCountdownAfterDelay(1f));
            }

            if (countdownStarted)
            {
                photonView.RPC("RPC_StartCountdown", newPlayer, startTime);
            }
        }
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerListUI();

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount <= 1)
        {
            countdownStarted = false;
            countdownText.text = "Waiting for more players...";
            Debug.Log("Se cancelo la cuenta regresiva, queda un solo jugador");
        }
    }



    private void UpdatePlayerListUI()
    {
        if (playersListText == null) return;

        playersListText.text = "Connected Players:\n";
        foreach (var player in PhotonNetwork.PlayerList)
        {
            playersListText.text += $"{player.NickName}\n";
        }
    }


    IEnumerator StartCountdownAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            Debug.LogWarning("No se puede iniciar cuenta regresiva: no conectado o no en sala");
            yield break;
        }

        startTime = PhotonNetwork.Time;
        countdownStarted = true;

        //Guardamos como propiedad de la sala
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
    {
        { "StartTime", startTime }
    };

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        //Enviamos el tiempo a todos
        photonView.RPC("RPC_StartCountdown", RpcTarget.All, startTime);
    }



    [PunRPC]
    void RPC_StartCountdown(double networkStartTime)
    {
        startTime = networkStartTime;
        countdownStarted = true;
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("StartTime") && !countdownStarted)
        {
            startTime = (double)propertiesThatChanged["StartTime"];
            countdownStarted = true;
        }
    }
}
