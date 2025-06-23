using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerNameTag : MonoBehaviourPun
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Transform lookAtTarget;

    void Start()
    {
        if (photonView.IsMine)
        {
            nameText.text = PhotonNetwork.NickName;
        }
        else
        {
            nameText.text = photonView.Owner.NickName;
        }

        if (Camera.main != null)
            lookAtTarget = Camera.main.transform;
    }

    void Update()
    {

        if (lookAtTarget != null)
        {
            transform.forward = lookAtTarget.forward;

        }
    }
}

