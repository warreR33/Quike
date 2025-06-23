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

    protected TrailRenderer Trail;
    protected Transform Target;
    
    public BulletTrailScriptableObject TralConfig;
    [SerializeField] Renderer Renderer;

    private bool IsDisabling = false;

    private int attackerActorNr;
    private bool hasExploded = false;

    [Header("Audio")]
    public AudioClip explosionSound;

    protected const string DO_DISABLE_METHOD_NAME = "DoDisable";

    protected virtual void OnEnable()
    {
        Renderer.enabled = true;
        IsDisabling = false;
        ConfigureTrail();

    }
    
    void Awake()
    {
        Trail = GetComponent<TrailRenderer>();
    }


    void Start()
    {
        StartCoroutine(ExplodeAfterDelay());
        ConfigureTrail();
    }


    private void ConfigureTrail()
    {
        if (Trail != null && TralConfig != null)
        {
            TralConfig.SetupTrail(Trail);
        }
    }


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



        if (PhotonNetwork.IsMasterClient)
        {
            MasterHandleExplosion(transform.position, attackerActorNr);
        }
        else
        {
            photonView.RPC("MasterHandleExplosion", RpcTarget.MasterClient, transform.position, attackerActorNr);
        }

    }

    [PunRPC]
    void MasterHandleExplosion(Vector3 explosionPos, int attackerNr)
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
                damageable.TakeDamage(explosionDamage, attackerNr);
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


    protected void OnDisable()
    {
        CancelInvoke(DO_DISABLE_METHOD_NAME);
        Renderer.enabled = false;

        if (Trail != null && TralConfig != null)
        {
            IsDisabling = true;
            Invoke(DO_DISABLE_METHOD_NAME, TralConfig.Time);
        }
        else
        {
            DoDisable();
        }
    }

    void DoDisable() {
        if (Trail != null && TralConfig != null)
        {
            Trail.Clear();
        }

        gameObject.SetActive(false);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
    
}
