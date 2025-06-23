using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class GrenadeProjectile : MonoBehaviourPun
{
    public float explosionRadius = 5f;
    public int explosionDamage = 50;
    public float lifeTime = 3f;
    public GameObject explosionEffect;

    public float speed = 20f;

<<<<<<< Updated upstream
    private int attackerViewID;
    private Rigidbody rb;
=======
    //private int attackerViewID;
    private int attackerActorNr;

>>>>>>> Stashed changes
    private bool hasExploded = false;

    void Start()
    {

        rb = GetComponent<Rigidbody>();
        StartCoroutine(ExplodeAfterDelay());
    }

    //public void SetAttacker(int viewID)
    //{
    //    attackerViewID = viewID;
    //}

    public void SetAttacker(int actorNr)
    {
        attackerActorNr = actorNr;
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

    void Update ()
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
        hasExploded = true;

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (PhotonNetwork.IsMasterClient)
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

<<<<<<< Updated upstream
        if (photonView.IsMine || PhotonNetwork.IsMasterClient)
=======

        photonView.RPC("MasterHandleExplosion", RpcTarget.MasterClient, transform.position, attackerActorNr);

        //photonView.RPC("MasterHandleExplosion", RpcTarget.MasterClient, transform.position, attackerViewID);
    }

    [PunRPC]
    void MasterHandleExplosion(Vector3 explosionPos, int attackerNr)
    {
    // Aplicar daño
        HashSet<IDamageable> alreadyDamaged = new HashSet<IDamageable>();

        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
        foreach (Collider col in colliders)
>>>>>>> Stashed changes
        {
            if (gameObject != null)
            {
<<<<<<< Updated upstream
                PhotonNetwork.Destroy(gameObject);

=======
                alreadyDamaged.Add(damageable);
                damageable.TakeDamage(explosionDamage, attackerNr);
>>>>>>> Stashed changes
            }
        }
    

<<<<<<< Updated upstream
=======
        // Destrucción segura
        if (photonView.IsMine || photonView.Owner == null)
        {
            if (gameObject != null)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else
        {
            photonView.RPC("RequestSelfDestruct", photonView.Owner);
        }
    }

    [PunRPC]
    void RequestSelfDestruct()
    {
        if (gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
>>>>>>> Stashed changes
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
