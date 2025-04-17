using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviourPun, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

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
        // Inicializamos el slider y texto
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

    public void TakeDamage(int damage)
    {
        //Si el objeto es mio aplico dano y efecto visual
        if (photonView.IsMine)
        {
            ApplyDamage(damage);

            if (damageFx != null)
                damageFx.ShowDamage();
        }
        //pero si no es local llamo al RPC para que aplique dano al otro jugador en su propia maquina
        else
        {
            photonView.RPC("RPC_ApplyDamage", photonView.Owner, damage);
        }
    }

    //Cuando un enemigo nos ataca el RPC aplica el dano en esta intancia y si somos duenos del objeto activa efecto visual
    [PunRPC]
    void RPC_ApplyDamage(int damage)
    {
        ApplyDamage(damage);

        if (photonView.IsMine && damageFx != null)
            damageFx.ShowDamage();
    }




    private void ApplyDamage(int damage)
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
