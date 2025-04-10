using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class DamageFx : MonoBehaviourPun
{
    public Image damageImage;
    public float flashSpeed = 5f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.4f);

    private bool damaged;

    void Update()
    {
        if (!photonView.IsMine) return;

        if (damaged)
        {
            damageImage.color = flashColor;
        }
        else
        {
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }

        damaged = false; 
    }

    public void ShowDamage()
    {
        if (!photonView.IsMine) return;

        damaged = true;
    }
}

