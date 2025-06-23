using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;


public class PlayerHealth : MonoBehaviourPun, IDamageable
{
    [SerializeField] private float maxHealth = 100;
    private float currentHealth;

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
<<<<<<< Updated upstream
=======
                
              if (hitFX != null)
                hitFX.ShowDamage();
>>>>>>> Stashed changes
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
        currentHealth -= damage;

        if (photonView.IsMine)
        {
            UpdateHealthUI();
        }

        if (currentHealth <= 0)
        {
            string killerName = "Player";

            //Player attackerPlayer = PhotonNetwork.CurrentRoom.GetPlayer(attackerActorNr);
            Photon.Realtime.Player attackerPlayer = PhotonNetwork.CurrentRoom.GetPlayer(attackerViewID);


            if (attackerPlayer != null)
            {
                if (attackerPlayer.ActorNumber != photonView.OwnerActorNr)
                {
                    gameManager.photonView.RPC("RPC_AddKill", RpcTarget.All, attackerPlayer.ActorNumber);
                }

                killerName = attackerPlayer.NickName;
            }

            gameManager.photonView.RPC("RPC_AddDeath", RpcTarget.All, photonView.OwnerActorNr);
            Die(killerName);
        }

        //if (currentHealth <= 0)
        //{
        //    //PlayerHealth attackerHealth = null;
        //    string killerName = "Player";

        //    PhotonView attackerView = PhotonView.Find(attackerViewID);



        //    if (attackerView != null)
        //    {

        //        int attackerActorNr = attackerView.OwnerActorNr;

        //        //Sincronizamos kills
        //        //Solo sumar kill si no se mato a si mismo
        //        if (attackerActorNr != PhotonNetwork.LocalPlayer.ActorNumber)
        //        {
        //            gameManager.photonView.RPC("RPC_AddKill", RpcTarget.All, attackerActorNr);
        //        }

        //        killerName = attackerView.Owner.NickName;

        //    }

        //    //Sincronizamos Deaths
        //    gameManager.photonView.RPC("RPC_AddDeath", RpcTarget.All, photonView.OwnerActorNr);


        //    Die(killerName);
        //}
    }


    private void Die(string killerName)
    {
        PlayerMovement playerMovement = this.transform.GetComponent<PlayerMovement>();

      
        if (photonView.IsMine)
        {

            if (playerMovement != null)
            {
                playerMovement.DesactiveScoreBoard();

               
            }

            if (!gameManager.GetIsWinner())
            {
                gameManager.SpawnPlayerAfterDead(killerName);

            }

<<<<<<< Updated upstream
=======
            if (deathEffectPrefab != null)
            {
                GameObject fx = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                Destroy(fx, 3f); 
            }

>>>>>>> Stashed changes
            if (gameObject != null)
            {
                PhotonNetwork.Destroy(gameObject);

            }
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
                healthText.text = $"{currentHealth}";
            }
        }
    }
}
