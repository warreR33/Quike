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
            //Cambiar por error en pantalla
            Debug.LogWarning("El nombre no puede estar vacio");
            return;
        }

        PhotonNetwork.NickName = playerName;

        //Guardar el nombre
        PlayerPrefs.SetString("PlayerName", playerName);


        //Carga Pantalla de lobby
        SceneManager.LoadScene("LoadingScene");
    }
}

