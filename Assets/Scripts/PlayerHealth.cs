using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviourPun, IDamageable
{
    [SerializeField] private float maxHealth = 100;
    private float currentHealth;

    [SerializeField] private DamageFx damageFx;

    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;



    private void Start()
    {
        currentHealth = maxHealth;

        if (photonView.IsMine)
        {
            damageFx = GetComponentInChildren<DamageFx>();

        }
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }

        UpdateHealthUI();
    }

    public void TakeDamage(int damage, int attackerViewID)
    {
        if (photonView.IsMine)
        {
            ApplyDamage(damage, attackerViewID);

            if (damageFx != null)
                damageFx.ShowDamage();
        }
        else
        {
            photonView.RPC("RPC_ApplyDamage", photonView.Owner, damage, attackerViewID);
        }
    }

    [PunRPC]
    void RPC_ApplyDamage(int damage, int attackerViewID)
    {
        ApplyDamage(damage, attackerViewID);

        if (photonView.IsMine && damageFx != null)
            damageFx.ShowDamage();
    }




    private void ApplyDamage(int damage, int attackerViewID)
    {
        currentHealth -= damage;

        UpdateHealthUI();


        if (currentHealth <= 0)
        {
            PhotonView attackerView = PhotonView.Find(attackerViewID);

            if (attackerView != null)
            {
                int attackerActorNr = attackerView.OwnerActorNr;

                //Sincronizamos kills
                GameManager.Instance.photonView.RPC("RPC_AddKill", RpcTarget.All, attackerActorNr);


            }

            //Sincronizamos Deaths
            GameManager.Instance.photonView.RPC("RPC_AddDeath", RpcTarget.All, photonView.OwnerActorNr);

            Die();
        }
    }


    private void Die()
    {

        
        if (photonView.IsMine)
        {
            UpdateHealthUI();
            GameManager.Instance.SpawnPlayerAfterDead();
            PhotonNetwork.Destroy(gameObject);
        }
   

    }

   

    private void UpdateHealthUI()
    {
        if (photonView.IsMine)
        {
            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }
        }
    }
}
