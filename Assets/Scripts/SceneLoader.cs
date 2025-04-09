using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;


public class SceneLoader : MonoBehaviourPunCallbacks
{
    [SerializeField] private float countdownTime = 1f;

    private Coroutine countdownCoroutine;
    private bool gameStarting = false;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            JoinRoom();
        }
    }

    public override void OnConnectedToMaster()
    {
        JoinRoom();
    }

    void JoinRoom()
    {
        Debug.Log("Intentando entrar a una sala...");
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Unido a la sala. Jugadores: {PhotonNetwork.CurrentRoom.PlayerCount}");

        // Solo el MasterClient controla el inicio
        if (PhotonNetwork.IsMasterClient)
        {
            //TryStartCountdown();
            PhotonNetwork.LoadLevel("GameScene");

        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Jugador entró: {newPlayer.NickName}");

        if (PhotonNetwork.IsMasterClient)
        {

            TryStartCountdown();
        }
    }

    void TryStartCountdown()
    {
        // Si ya se está por empezar, no hacemos nada
        if (gameStarting)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            Debug.Log("Mínimo de jugadores alcanzado. Iniciando cuenta regresiva...");
            countdownCoroutine = StartCoroutine(CountdownToStart());
            gameStarting = true;
        }
        else
        {
            Debug.Log("Esperando al menos 2 jugadores...");
        }
    }

    IEnumerator CountdownToStart()
    {
        float timeLeft = countdownTime;

        while (timeLeft > 0)
        {
            Debug.Log($"La partida comienza en {Mathf.CeilToInt(timeLeft)} segundos...");
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        LoadGameScene();
    }

    void LoadGameScene()
    {
        Debug.Log("Cargando escena GameScene...");
        PhotonNetwork.LoadLevel("GameScene");
    }
}
