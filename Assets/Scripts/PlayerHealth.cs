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
    }

    public void TakeDamage(float damage)
    {
        if (photonView.IsMine)
        {
            ApplyDamage(damage);

            if (damageFx != null)
                damageFx.ShowDamage();
        }
        else
        {
            photonView.RPC("RPC_ApplyDamage", photonView.Owner, damage);
        }
    }

    [PunRPC]
    void RPC_ApplyDamage(float damage)
    {
        ApplyDamage(damage);

        if (photonView.IsMine && damageFx != null)
            damageFx.ShowDamage();
    }




    private void ApplyDamage(float damage)
    {
        currentHealth -= damage;

        Debug.Log($"Vida restante: {currentHealth}");

        UpdateHealthUI();


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Estoy muerto");
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
                healthText.text = $"{currentHealth}/{maxHealth}";
            }
        }
    }
}
