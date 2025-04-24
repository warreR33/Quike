using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;


public class GameEndUIController : MonoBehaviour
{
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI countdownText;

    public void SetWinner(string winner)
    {

        if (winnerText != null) winnerText.text = $"{winner} has won!";
    }

    public void SetCountdown(int seconds)
    {
        if (countdownText != null) countdownText.text = $"Going back to menu in {seconds}s...";
    }
}

