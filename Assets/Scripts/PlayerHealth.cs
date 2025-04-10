using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPun, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;





    private void Start()
    {
        currentHealth = maxHealth;

    }

    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine) return;

        currentHealth -= damage;

        Debug.Log($"Vida restante: {currentHealth}");

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
}
