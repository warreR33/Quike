using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerRowUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text killsText;
    public TMP_Text deathsText;
    public TMP_Text pingText;

    public void SetValues(string name, int kills, int deaths, int ping)
    {
        nameText.text = name;
        killsText.text = kills.ToString();
        deathsText.text = deaths.ToString();
        pingText.text = ping.ToString();
    }
}

