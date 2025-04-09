using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class MoveCamera : MonoBehaviour
{
    private PhotonView photonView;

    public Transform cameraPosition;

    private void Start()
    {
        photonView = GetComponentInParent<PhotonView>();

    }
    // Update is called once per frame
    private void Update()
    {
        if (!photonView.IsMine) return;

        transform.position = cameraPosition.position;
    }
}
