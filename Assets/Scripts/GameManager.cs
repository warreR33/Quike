using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxGameKills= 2;

    //Stats por jugador
    private Dictionary<int, PlayerStatsData> playerStats = new Dictionary<int, PlayerStatsData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            //RegisterPlayer(PhotonNetwork.LocalPlayer);

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

    //Quitar de la lista los player que se desconecten
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

    //public void AddKill(int actorNumber)
    //{
    //    if (playerStats.ContainsKey(actorNumber))
    //    {
    //        playerStats[actorNumber].kills++;

    //        Debug.Log($"Kills de {PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).NickName}: {playerStats[actorNumber].kills}");

    //        if (playerStats[actorNumber].kills >= 10)
    //        {
    //            WinGame(actorNumber);
    //        }
    //    }
    //}

    [PunRPC]
    public void RPC_AddKill(int actorNumber)
    {
        if (!playerStats.ContainsKey(actorNumber))
            RegisterPlayer(PhotonNetwork.CurrentRoom.GetPlayer(actorNumber));

        playerStats[actorNumber].kills++;

        Debug.Log($"[RPC] Kills de {PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).NickName}: {playerStats[actorNumber].kills}");

        if (playerStats[actorNumber].kills >= maxGameKills)
        {
            WinGame(actorNumber);
        }
    }

    [PunRPC]
    public void RPC_AddDeath(int actorNumber)
    {
        if (!playerStats.ContainsKey(actorNumber))
            RegisterPlayer(PhotonNetwork.CurrentRoom.GetPlayer(actorNumber));

        playerStats[actorNumber].deaths++;
        Debug.Log($"[RPC] Deaths de {PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).NickName}: {playerStats[actorNumber].deaths}");
    }
    //public void AddDeath(int actorNumber)
    //{
    //    if (playerStats.ContainsKey(actorNumber))
    //    {
    //        playerStats[actorNumber].deaths++;
    //        Debug.Log($"Muertes de {PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).NickName}: {playerStats[actorNumber].deaths}");
    //    }
    //}

    private void WinGame(int actorNumber)
    {
        string winnerName = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).NickName;
        Debug.Log($"{winnerName} ganó la partida!");
        //PhotonNetwork.LoadLevel("VictoryScene");


        photonView.RPC("RPC_EndGame", RpcTarget.AllBufferedViaServer);

    }

    [PunRPC]
    private void RPC_EndGame()
    {
        //PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel("MainMenu");
        //StartCoroutine(DisconnectAndLoadMenu());
    }

    private IEnumerator DisconnectAndLoadMenu()
    {
        PhotonNetwork.Disconnect();

        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
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
        Debug.Log("Respawneo Player ID: " + photonView.ViewID + " Nickname: " + photonView.Owner.NickName);

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




    [System.Serializable]
    public class PlayerStatsData
    {
        public int kills;
        public int deaths;
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

        foreach (var entry in playerStats)
        {
            string name = PhotonNetwork.CurrentRoom?.GetPlayer(entry.Key)?.NickName ?? $"Jugador {entry.Key}";
            GUILayout.Label($"<size=26>{name} - Kills: {entry.Value.kills} | Deaths: {entry.Value.deaths}</size>");
        }

        GUILayout.EndArea();
    }
}


