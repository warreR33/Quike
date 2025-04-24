using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingRoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI playersListText;
    [SerializeField] private TextMeshProUGUI countdownText;

    [SerializeField] private Button exitButton;


    private const float totalCountdownTime = 5f;
    private bool countdownStarted = false;
    private double startTime;
    private bool joinedRoom = false;


    private void Awake()
    {
        //todos los clientes cambian de escena cuando el Master lo hace
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        exitButton.onClick.AddListener(OnClickExit);


        if (!PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState == ClientState.Disconnected)
        {
            Debug.Log("Reconectando a Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Ya conectado, uniendo a sala...");
            JoinMainRoom();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado al Master");
        JoinMainRoom();
    }

    private void JoinMainRoom()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom("SalaPrincipal", roomOptions, TypedLobby.Default);
    }



    public override void OnJoinedRoom()
    {

        Debug.Log("Unido a la sala: " + PhotonNetwork.CurrentRoom.Name);


        joinedRoom = true;

        UpdatePlayerListUI();

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            //Arrancamos despues de un segundo para evitar problemas de sync
            StartCoroutine(StartCountdownAfterDelay(1f));

        }
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
       
            Debug.LogError("Fallo al unirse a la sala: " + message);
            Debug.LogError("Se redirige a MainMenu");
        SceneManager.LoadScene("MainMenu");


    }

    private void OnClickExit()
    {

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Saliste de la sala");
        SceneManager.LoadScene("MainMenu");
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
                Debug.Log("Se cierra la sala");

                PhotonNetwork.LoadLevel("GameScene");
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerListUI();

        //Si esta el master mas un jugador empieza el conteo
        if (PhotonNetwork.IsMasterClient && !countdownStarted && PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            StartCoroutine(StartCountdownAfterDelay(1f));
        }

        if (PhotonNetwork.IsMasterClient && countdownStarted)
        {
            //RPC al nuevo jugador para sincronizar el tiempo en cada cliente
            photonView.RPC("RPC_StartCountdown", newPlayer, startTime);
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

        startTime = PhotonNetwork.Time;
        countdownStarted = true;

        //RPC para pasarle el tiempo de inicio a todos los clientes
        photonView.RPC("RPC_StartCountdown", RpcTarget.All, startTime);
    }

    [PunRPC]
    void RPC_StartCountdown(double networkStartTime)
    {
        startTime = networkStartTime;
        countdownStarted = true;
    }

  
}
