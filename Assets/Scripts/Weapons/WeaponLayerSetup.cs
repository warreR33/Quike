using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponLayerSetup : MonoBehaviourPun
{
    void Start()
    {
        if (photonView.IsMine)
        {
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("LocalWeapon"));
        }
        else
        {
            //si no es el jugador local le asigna otra camara
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}

