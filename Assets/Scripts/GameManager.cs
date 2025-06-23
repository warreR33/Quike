using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject gameEndUIPrefab;
    [SerializeField] private GameObject localPlayerDeadUIPrefab;
    [SerializeField] private GameObject localPlayerDisconnecting;


    private GameObject gameEndUIInstance;

    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxGameKills= 5;

    private bool isWinner = false;

    //Pequena clase para guardar los datos de partida de cada player
    [System.Serializable] public class PlayerStatsData
    {
        public int kills;
        public int deaths;
    }

    //Stats por jugador
    private Dictionary<int, PlayerStatsData> playerStats = new Dictionary<int, PlayerStatsData>();
    public Dictionary<int, PlayerStatsData> PlayerStats => playerStats;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (gameObject != null)
            {
                PhotonNetwork.Destroy(gameObject);

            }

            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }



    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LobbyScene")
        {
            if (gameObject != null)
            {
                PhotonNetwork.Destroy(gameObject);

            }
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;

    }

    private void Start()
    {
       

        Debug.Log("Cantidad de jugadores en la sala: " + PhotonNetwork.CurrentRoom.PlayerCount);

        PhotonNetwork.AutomaticallySyncScene = true;


        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {

            //Registramos a todos los jugadores
            foreach (var player in PhotonNetwork.PlayerList)
            {
                RegisterPlayer(player);
            }

            SpawnPlayer();
        }

        else
        {
            Debug.LogError("No estas conectado a Photon o no estas en una sala");
        }
    }

  

    //Quitamos de la lista los player que se desconecten
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playerStats.ContainsKey(otherPlayer.ActorNumber))
        {
            playerStats.Remove(otherPlayer.ActorNumber);
        }

        //Si solo queda un jugador se termina la partida
        if(PhotonNetwork.PlayerList.Length == 1)
        {
            Time.timeScale = 0f;

            localPlayerDisconnecting.SetActive(true);

            StartCoroutine(LeaveRoomAfterDelay(5f));


        }


    }

   

    public void RegisterPlayer(Player player)
    {
        if (!playerStats.ContainsKey(player.ActorNumber))
        {
            playerStats[player.ActorNumber] = new PlayerStatsData();
        }
    }




    [PunRPC]
    public void RPC_AddDeath(int actorNumber)
    {
        if (!playerStats.ContainsKey(actorNumber))
        {
            RegisterPlayer(PhotonNetwork.CurrentRoom.GetPlayer(actorNumber));

        }
        
        playerStats[actorNumber].deaths++;
    }

    [PunRPC]
    public void RPC_AddKill(int actorNumber)
    {
        if (!playerStats.ContainsKey(actorNumber))
        {
            RegisterPlayer(PhotonNetwork.CurrentRoom.GetPlayer(actorNumber));

        }

        playerStats[actorNumber].kills++;


        if (PhotonNetwork.IsMasterClient && !isWinner && playerStats[actorNumber].kills >= maxGameKills)
        {
            isWinner = true;
            WinGame(actorNumber);
        }
    }

    private void WinGame(int actorNumber)
    {
        string winnerName = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).NickName;

        photonView.RPC("RPC_EndGame", RpcTarget.All, winnerName);

    }

    [PunRPC]
    private void RPC_EndGame(string winnerName)
    {
        //Desactivamos Inputs
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            PhotonView view = player.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
            {
                PlayerMovement movement = player.GetComponent<PlayerMovement>();
                if (movement != null)
                    movement.SetInputOff();

                Shooter shooter = player.GetComponent<Shooter>();
                if (shooter != null)
                    shooter.SetInputOff();
            }
        }



        ////Pausamos juego
        Time.timeScale = 0f;

        //Ocultar Placa de Asesinato
        localPlayerDeadUIPrefab.SetActive(false);

        ////Mostrar UI victoria
        ShowGameEndUI(winnerName);

        ////Esperar y cargar MainMenu
        StartCoroutine(LeaveRoomAfterDelay(5f));


    }



    private void ShowGameEndUI(string winnerName)
    {

        gameEndUIInstance = Instantiate(gameEndUIPrefab);
        GameEndUIController controller = gameEndUIInstance.GetComponent<GameEndUIController>();

        if (controller != null)
        {
            controller.SetWinner(winnerName);
        }
    }


    [PunRPC]
    private void RPC_NotifyGameEnd()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }

   

    private IEnumerator LeaveRoomAfterDelay(float delay)
    {
        
        GameEndUIController controller = gameEndUIInstance?.GetComponent<GameEndUIController>();
        float t = delay;

        while (t > 0)
        {
            controller?.SetCountdown(Mathf.CeilToInt(t));
            yield return new WaitForSecondsRealtime(1f);
            t -= 1f;
        }

        //Importante aseguramos que el mensaje queue este activo hasta que realmente salimos
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        //Esperamos efectivamente salir de la sala
        while (PhotonNetwork.InRoom)
        {
            yield return null;
        }

        //Ahora si desactivamos cola y cargamos Lobby
        PhotonNetwork.IsMessageQueueRunning = false;
        SceneManager.LoadScene("LobbyScene");
    }



    public override void OnLeftRoom()
    {
        Debug.Log("Saliste de la sala (OnLeftRoom)");
     
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Desconectado: " + cause);
        SceneManager.LoadScene("LobbyScene");
    }


    public void SpawnPlayer()
    {
        if (isWinner) return;

        Transform spawnPoint = GetRandomSpawnPoint();
        PhotonNetwork.Instantiate("PlayerPrefab", spawnPoint.position, spawnPoint.rotation);

    }

    private Transform GetRandomSpawnPoint()
    {
     
        return spawnPoints[Random.Range(0, spawnPoints.Length)];

    }


    public void SpawnPlayerAfterDead(string killerName)
    {
        localPlayerDeadUIPrefab.SetActive(true);

        //Buscar el componente que controla el texto del UI dentro del prefab
        DeadUIController deadUI = localPlayerDeadUIPrefab.GetComponent<DeadUIController>();
        if (deadUI != null)
        {
            deadUI.Init(killerName); 
        }

        StartCoroutine(Respawn());
    }


    private IEnumerator Respawn()
    {

        yield return new WaitForSeconds(3f);
        SpawnPlayer();
        localPlayerDeadUIPrefab.SetActive(false);
    }



    public bool GetIsWinner()
    {
        return isWinner;
    }


    private void EnsureAllPlayersRegistered()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!playerStats.ContainsKey(player.ActorNumber))
            {
                RegisterPlayer(player);
            }
        }
    }

    

    void OnGUI()
    {
        int ping = PhotonNetwork.GetPing();

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 18;
        style.normal.textColor = Color.white;

        string text = $"Ping: {ping} ms";

        float width = 150;
        float height = 30;
        float x = Screen.width - width - 10;
        float y = 10;

        GUI.Label(new Rect(x, y, width, height), text, style);
    }

    public void SalirManualmenteAlMenu()
{
    Time.timeScale = 1f; // Por si estaba pausado
    PhotonNetwork.AutomaticallySyncScene = false;
    StartCoroutine(VolverAlMenuTrasSalir());
}

private IEnumerator VolverAlMenuTrasSalir()
{
    if (PhotonNetwork.InRoom)
        PhotonNetwork.LeaveRoom();

    while (PhotonNetwork.InRoom)
        yield return null;

    SceneManager.LoadScene("MainMenu");
}

}


