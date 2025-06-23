using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;


public class PauseMenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private Button exitToMenuButton;

    private bool isPaused = false;
    private bool isExiting = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        exitToMenuButton.onClick.AddListener(ExitToMenu);

        BloquearCursor(); // Entra con cursor bloqueado
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void TogglePauseMenu()
    {
        isPaused = !isPaused;
        pauseMenuUI.SetActive(isPaused);

        if (isPaused)
            LiberarCursor();
        else
            BloquearCursor();
    }

private void ExitToMenu()
{
    GameManager.Instance.SalirManualmenteAlMenu();
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}


    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void LiberarCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void BloquearCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ForzarCerrarMenu()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        BloquearCursor();
    }
}

