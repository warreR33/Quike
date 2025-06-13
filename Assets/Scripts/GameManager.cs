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
            Destroy(gameObject); 
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null; 
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

            localPlayerDeadUIPrefab.SetActive(true);

            StartCoroutine(BackToMainMenuAfterDelay(5f));

            //Debug.Log("No hay mas jugadores en la sala. Redirigiendo a MainMenu");
            //SceneManager.LoadScene("MainMenu");

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


        if (playerStats[actorNumber].kills >= maxGameKills)
        {
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
        

        ////Pausamos juego
        Time.timeScale = 0f;

        ////Mostrar UI victoria
        ShowGameEndUI(winnerName);

        ////Esperar y cargar MainMenu
        StartCoroutine(BackToMainMenuAfterDelay(5f));

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

    private IEnumerator BackToMainMenuAfterDelay(float seconds)
    {

        //El fin de la partida lo hacemos async
        PhotonNetwork.AutomaticallySyncScene = false;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();

        }

        while (PhotonNetwork.InRoom) yield return null;

        float t = seconds;

        //Contador
        GameEndUIController controller = gameEndUIInstance?.GetComponent<GameEndUIController>();

        while (t > 0)
        {
            if (controller != null)
            {
                controller.SetCountdown(Mathf.CeilToInt(t));

            }

            yield return new WaitForSecondsRealtime(1f);
            t -= 1f;
        }


        SceneManager.LoadScene("MainMenu");
    }


    public void SpawnPlayer()
    {
        Transform spawnPoint = GetRandomSpawnPoint();
        PhotonNetwork.Instantiate("PlayerPrefab", spawnPoint.position, spawnPoint.rotation);

    }

    private Transform GetRandomSpawnPoint()
    {
     
        return spawnPoints[Random.Range(0, spawnPoints.Length)];

    }

    public void SpawnPlayerAfterDead()
    {
        localPlayerDeadUIPrefab.SetActive(true);
        StartCoroutine(Respawn());

    }

    private IEnumerator Respawn()
    {

        yield return new WaitForSeconds(3f);
        SpawnPlayer();
        localPlayerDeadUIPrefab.SetActive(false);
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
}


