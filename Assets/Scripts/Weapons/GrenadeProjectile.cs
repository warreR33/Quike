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
    public float speed = 20f;

    private int attackerViewID;
    private bool hasExploded = false;

    [Header("Audio")]
    public AudioClip explosionSound;

    void Start()
    {
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

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(lifeTime);
        if (!hasExploded)
            Explode();
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(effect, 2f);
            }
        }



        photonView.RPC("MasterHandleExplosion", RpcTarget.MasterClient, transform.position, attackerViewID);
    }

    [PunRPC]
    void MasterHandleExplosion(Vector3 explosionPos, int attackerID)
    {
    // Aplicar daño
        HashSet<IDamageable> alreadyDamaged = new HashSet<IDamageable>();

        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
        foreach (Collider col in colliders)
        {
            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null && !alreadyDamaged.Contains(damageable))
            {
                alreadyDamaged.Add(damageable);
                damageable.TakeDamage(explosionDamage, attackerID);
            }
        }

        // Destrucción segura
        if (photonView.IsMine || photonView.Owner == null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            photonView.RPC("RequestSelfDestruct", photonView.Owner);
        }
    }

    [PunRPC]
    void RequestSelfDestruct()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
