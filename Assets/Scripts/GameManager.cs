using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviourPunCallbacks
{

    [SerializeField] private GameObject gameEndUIPrefab;
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
            RegisterPlayer(PhotonNetwork.CurrentRoom.GetPlayer(actorNumber));

        playerStats[actorNumber].deaths++;
    }

    [PunRPC]
    public void RPC_AddKill(int actorNumber)
    {
        if (!playerStats.ContainsKey(actorNumber))
            RegisterPlayer(PhotonNetwork.CurrentRoom.GetPlayer(actorNumber));

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


        PhotonNetwork.AutomaticallySyncScene = false;

        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();

        while (PhotonNetwork.InRoom)
            yield return null;

        float t = seconds;

        //Contador
        GameEndUIController controller = gameEndUIInstance?.GetComponent<GameEndUIController>();
        while (t > 0)
        {
            if (controller != null)
                controller.SetCountdown(Mathf.CeilToInt(t));
            yield return new WaitForSecondsRealtime(1f);
            t -= 1f;
        }


        SceneManager.LoadScene("MainMenu");
    }


    public PlayerStatsData GetStats(int actorNumber)
    {
        if (playerStats.TryGetValue(actorNumber, out var stats))
            return stats;
        return null;
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
        StartCoroutine(Respawn());

    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(3f);
        SpawnPlayer();
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

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 600, Screen.height));
        GUILayout.Label("<b><size=26>DEBUG - STATS</size></b>");

        EnsureAllPlayersRegistered();

        //Ordenar por kills
        var sortedStats = playerStats.OrderByDescending(p => p.Value.kills);

        foreach (var entry in sortedStats)
        {

        string name;

        Player player = PhotonNetwork.CurrentRoom?.GetPlayer(entry.Key);

        if (player != null)
            name = player.NickName;
        else
            name = $"Player {entry.Key}";

        GUILayout.Label($"<size=26>{name} - Kills: {entry.Value.kills} | Deaths: {entry.Value.deaths}</size>");
        }

        GUILayout.EndArea();
    }
}


