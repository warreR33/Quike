using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;


public class ScoreboardUI : MonoBehaviour
{
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Transform contentParent;

    private Dictionary<int, GameObject> rows = new Dictionary<int, GameObject>();

    public void UpdateScoreboard(Dictionary<int, GameManager.PlayerStatsData> stats)
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        var sorted = stats.OrderByDescending(p => p.Value.kills);

        foreach (var entry in sorted)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(entry.Key);
            string name = player != null ? player.NickName : $"Player {entry.Key}";
            //int ping = player != null ? player.GetPing() : 0;
            //int ping = 66;
            int ping = (player == PhotonNetwork.LocalPlayer) ? PhotonNetwork.GetPing() : 0;


            GameObject row = Instantiate(rowPrefab, contentParent);
            PlayerRowUI rowUI = row.GetComponent<PlayerRowUI>();
            rowUI.SetValues(name, entry.Value.kills, entry.Value.deaths, ping);
        }
    }
}

