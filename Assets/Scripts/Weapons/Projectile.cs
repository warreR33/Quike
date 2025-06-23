using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviourPun
{
    public float speed = 20f;
    public int damage = 100;
    public float lifetime = 5f;

    private int attackerActorNumber;

    private void Start()
    {
        //Destruimos el objeto si no toco nada en un tiempo
        if (photonView.IsMine)
        {
            StartCoroutine(DestroyAfterTime());
        }

    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifetime);

        if(gameObject != null )
        {
            PhotonNetwork.Destroy(gameObject);

        }
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    public void SetAttacker(int actorNumber)
    {
        attackerActorNumber = actorNumber;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (!photonView.IsMine) return;

        if (other.TryGetComponent(out IDamageable damageable))
        {
            //Se dana y se pasa autor
            damageable.TakeDamage(damage, attackerActorNumber);

            if (gameObject != null)
            {
                PhotonNetwork.Destroy(gameObject);

            }
        }
        else if (!other.isTrigger)
        {
            if (gameObject != null)
            {
                PhotonNetwork.Destroy(gameObject);

            }
        }
    }
}

