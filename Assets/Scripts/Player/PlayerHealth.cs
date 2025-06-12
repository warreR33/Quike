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
    public float currentHealth;

    [SerializeField] private DamageFx damageFx;

    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    private GameManager gameManager;


    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        currentHealth = maxHealth;

        if (photonView.IsMine)
        {
            damageFx = GetComponentInChildren<DamageFx>();

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"{currentHealth}";
            }
        }
        else
        {
            // Desactivar UI en instancias que no son nuestras
            if (healthSlider != null)
                healthSlider.gameObject.SetActive(false);
            if (healthText != null)
                healthText.gameObject.SetActive(false);
        }
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
            //aplica dano al dueno del objeto
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
        if (photonView.IsMine)
        {
            UpdateHealthUI();

        }


        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            PhotonView attackerView = PhotonView.Find(attackerViewID);

            if (attackerView != null)
            {

                int attackerActorNr = attackerView.OwnerActorNr;



                //Sincronizamos kills
                //Solo sumar kill si no se mato a si mismo
                if (attackerActorNr != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    gameManager.photonView.RPC("RPC_AddKill", RpcTarget.All, attackerActorNr);
                }

            }
          
            //Sincronizamos Deaths
            gameManager.photonView.RPC("RPC_AddDeath", RpcTarget.All, photonView.OwnerActorNr);


            Die();


        }
    }


    private void Die()
    {

        
        


            gameManager.SpawnPlayerAfterDead();

            PlayerMovement playerMovement = this.transform.GetComponent<PlayerMovement>();

            playerMovement.DesactiveScoreBoard();

            PhotonNetwork.Destroy(gameObject);
        
   

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
                healthText.text = $"{currentHealth}";
            }

       

        }
    }
}
