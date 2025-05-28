using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LocalWeaponCamera : MonoBehaviourPun
{
    void Start()
    {
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}

