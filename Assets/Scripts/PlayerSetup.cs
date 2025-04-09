using Photon.Pun;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;

    private void Start()
    {
        PhotonView view = GetComponent<PhotonView>();

        if (view.IsMine)
        {
            playerCamera.SetActive(true);
        }
        else
        {
            playerCamera.SetActive(false);
        }
    }
}

