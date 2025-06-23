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
    [SerializeField] private HitFX hitFX;
    [SerializeField] private GameObject deathEffectPrefab;

    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    private GameManager gameManager;

    private bool isDead = false;


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
            if (healthSlider != null)
                healthSlider.gameObject.SetActive(false);
            if (healthText != null)
                healthText.gameObject.SetActive(false);
        }
    }

    public bool GetIsDead()
    {
        return isDead;
    }

    public void TakeDamage(int damage, int attackerActorNr)
    {
        if (isDead) return;

        if (photonView.IsMine)
        {
            ApplyDamage(damage, attackerActorNr);

            if (damageFx != null)
                damageFx.ShowDamage();
                
              if (hitFX != null)
                Debug.Log("Si, si seno1");
                hitFX.ShowDamage();
        }

        else
        {
            photonView.RPC("RPC_ApplyDamage", photonView.Owner, damage, attackerActorNr);
        }
    }

    [PunRPC]
    void RPC_ApplyDamage(int damage, int attackerActorNr)
    {
        if (!photonView.IsMine || isDead) return;

        ApplyDamage(damage, attackerActorNr);

        if (photonView.IsMine && !isDead)
        {
            if (damageFx != null)
                damageFx.ShowDamage();

            if (hitFX != null)
                Debug.Log("Si, si seno2");
                hitFX.ShowDamage();
        }
            
    }

    private void ApplyDamage(int damage, int attackerActorNr)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (photonView.IsMine)
        {
            UpdateHealthUI();
        }


        if (currentHealth <= 0)
        {
            isDead = true;

            //PlayerHealth attackerHealth = null;
            string killerName = "Player";

            //PhotonView attackerView = PhotonView.Find(attackerActorNr);
            Photon.Realtime.Player attackerPlayer = PhotonNetwork.CurrentRoom.GetPlayer(attackerActorNr);


            if (attackerPlayer != null)
            {
                //Sincronizamos kills
                //Solo sumar kill si no se mato a si mismo
                if (attackerPlayer.ActorNumber != photonView.OwnerActorNr)
                {
                    gameManager.photonView.RPC("RPC_AddKill", RpcTarget.All, attackerPlayer.ActorNumber);
                }

                //Sincronizamos kills
                //Solo sumar kill si no se mato a si mismo
                //if (attackerActorNr != PhotonNetwork.LocalPlayer.ActorNumber)
                //{
                //    gameManager.photonView.RPC("RPC_AddKill", RpcTarget.All, attackerActorNr);
                //}

                //killerName = attackerView.Owner.NickName;
                killerName = attackerPlayer.NickName;


            }

            //Sincronizamos Deaths
            gameManager.photonView.RPC("RPC_AddDeath", RpcTarget.All, photonView.OwnerActorNr);


            Die(killerName);
        }
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

            if (deathEffectPrefab != null)
            {
                GameObject fx = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                Destroy(fx, 3f); // Opcional: destruir luego de 3 segundos
            }

            if (gameObject != null)
            {
                photonView.RPC("RPC_ShowDeathFX", RpcTarget.All, transform.position);
                PhotonNetwork.Destroy(gameObject);
            }
        }
   

    }

    [PunRPC]
    void RPC_ShowDeathFX(Vector3 position)
    {
        if (deathEffectPrefab != null)
        {
            GameObject fx = Instantiate(deathEffectPrefab, position, Quaternion.identity);
            Destroy(fx, 3f); // O la duración de tu partícula
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
