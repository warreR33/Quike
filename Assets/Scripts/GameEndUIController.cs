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
       
        winnerText.text = $"{winner} has won!";
    }

    public void SetCountdown(int seconds)
    {
        countdownText.text = $"Going back to menu in {seconds}s...";
    }
}

