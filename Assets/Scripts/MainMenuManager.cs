using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button playButton;

    private void Start()
    {
        //Despausamos el juego tras el loop inicial
        Time.timeScale = 1f;

        playButton.onClick.AddListener(OnClickPlay);

        playButton.interactable = false; 

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
           
            CheckIfInRoom();
        }
    }

    private void CheckIfInRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            playButton.interactable = false; 
        }
        else
        {
            playButton.interactable = true; 
        }
    }



    public override void OnConnectedToMaster()
    {
        CheckIfInRoom();

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            nameInput.text = PlayerPrefs.GetString("PlayerName");
            PhotonNetwork.NickName = nameInput.text;
        }
    }

    private void OnClickPlay()
    {
        string playerName = nameInput.text;

        if (string.IsNullOrWhiteSpace(playerName))
        {
            Debug.LogWarning("El nombre no puede estar vacio");
            return;
        }

        PhotonNetwork.NickName = playerName;

        PlayerPrefs.SetString("PlayerName", playerName);

        SceneManager.LoadScene("LoadingScene");
    }
}
