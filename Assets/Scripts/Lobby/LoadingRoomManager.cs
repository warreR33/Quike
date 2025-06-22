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

        double timeElapsed = PhotonNetwork.Time - startTime;
        double timeLeft = totalCountdownTime - timeElapsed;

        countdownText.text = $"The game will start in: {Mathf.CeilToInt((float)timeLeft)}s";

        if (timeLeft <= 0)
        {
            countdownStarted = false;

            if (PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;

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

        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InRoom)
        {
            Debug.LogWarning("No se puede iniciar cuenta regresiva: no conectado o no en sala");
            yield break;
        }

        startTime = PhotonNetwork.Time;
        countdownStarted = true;

        // Solo usamos RPC para sincronizar
        photonView.RPC("RPC_StartCountdown", RpcTarget.All, startTime);
    }

    [PunRPC]
    void RPC_StartCountdown(double networkStartTime)
    {
        startTime = networkStartTime;
        countdownStarted = true;
    }
}
