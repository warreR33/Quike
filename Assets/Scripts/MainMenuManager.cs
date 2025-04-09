using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;


public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button playButton;

    private void Start()
    {
        playButton.onClick.AddListener(OnClickPlay);
    }

    private void OnClickPlay()
    {
        string playerName = nameInput.text;

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("El nombre no puede estar vacío.");
            return;
        }

        PhotonNetwork.NickName = playerName;

        //Guardar el nombre
        PlayerPrefs.SetString("PlayerName", playerName);


        //Carga Pantalla de Juego
        SceneManager.LoadScene("LoadingScene");
    }
}

