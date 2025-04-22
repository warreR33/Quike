using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LoadingRoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI playersListText;
    [SerializeField] private TextMeshProUGUI countdownText;

    private const float totalCountdownTime = 0f;
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
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = "1.0";
            PhotonNetwork.ConnectUsingSettings();
        }
    }



    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado al Master");

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
        //Debug.Log("Cantidad de jugadores en la sala: " + PhotonNetwork.CurrentRoom.PlayerCount);


        joinedRoom = true;

        UpdatePlayerListUI();

        if (PhotonNetwork.IsMasterClient)
        {
            //Arrancamos despues de un segundo para evitar problemas
            StartCoroutine(StartCountdownAfterDelay(1f));
        }
    }

    private void Update()
    {
        //Si ya estamos en la sala y la cuenta regresiva comenzo
        if (!joinedRoom || !countdownStarted) return;

        double timeElapsed = PhotonNetwork.Time - startTime;
        double timeLeft = totalCountdownTime - timeElapsed;

        countdownText.text = $"The game will start in: {Mathf.CeilToInt((float)timeLeft)}s";

        if (timeLeft <= 0)
        {
            countdownStarted = false;

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("GameScene");
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerListUI();

        if (PhotonNetwork.IsMasterClient && countdownStarted)
        {
            //RPC para sincronizar el tiempo en cada cliente
            photonView.RPC("RPC_StartCountdown", newPlayer, startTime);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerListUI();
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
