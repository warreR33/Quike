using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerNameTag : MonoBehaviourPun
{
    [SerializeField] private TextMeshProUGUI nameText;
    private Camera localPlayerCamera;

    void Start()
    {
        // Setea el nombre una sola vez
        nameText.text = photonView.IsMine ? PhotonNetwork.NickName : photonView.Owner.NickName;

        // Buscar la cámara del jugador local
        foreach (var cam in Camera.allCameras)
        {
            var photonViewInParent = cam.GetComponentInParent<PhotonView>();
            if (photonViewInParent != null && photonViewInParent.IsMine)
            {
                localPlayerCamera = cam;
                break;
            }
        }

        // Fallback si no hay cámara con PhotonView
        if (localPlayerCamera == null)
            localPlayerCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (localPlayerCamera == null) return;

        // Billboard con orientación correcta
        transform.LookAt(transform.position + localPlayerCamera.transform.rotation * Vector3.forward,
                         localPlayerCamera.transform.rotation * Vector3.up);
    }
}

