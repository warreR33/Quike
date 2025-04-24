using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GrenadeProjectile : MonoBehaviourPun
{
    public float explosionRadius = 5f;
    public int explosionDamage = 50;
    public float lifeTime = 3f;
    public GameObject explosionEffect;

    private int attackerViewID;
    private Rigidbody rb;
    private bool hasExploded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(ExplodeAfterDelay());
    }

    public void SetAttacker(int viewID)
    {
        attackerViewID = viewID;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        IDamageable target = collision.collider.GetComponent<IDamageable>();
        if (target != null)
        {
            Explode(); 
        }
   
    }

    IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(lifeTime);

        if (!hasExploded)
        {
            Explode();

        }
    }

    void Explode()
    {
        hasExploded = true;

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (photonView.IsMine)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider col in colliders)
            {
                IDamageable damageable = col.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(explosionDamage,attackerViewID); 
                }
            }
        }

        PhotonNetwork.Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
