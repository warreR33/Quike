using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviourPun
{
    public float speed = 20f;
    public int damage = 100;
    public float lifetime = 5f;


    public BulletTrailScriptableObject TralConfig;
    protected TrailRenderer Trail;
    protected Transform Target;

    [SerializeField] private GameObject impactEffect;
    [SerializeField] private LayerMask structureLayers;

    [SerializeField] Renderer Renderer;

    private bool IsDisabling = false;

    private int attackerActorNumber;

    protected const string DO_DISABLE_METHOD_NAME = "DoDisable";

    void Awake()
    {
        Trail = GetComponent<TrailRenderer>();
    }

    protected virtual void OnEnable()
    {
        Renderer.enabled = true;
        IsDisabling = false;
        ConfigureTrail();

    }
    private void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(DestroyAfterTime());
        }
        ConfigureTrail();
    }

    private void ConfigureTrail()
    {
        if (Trail != null && TralConfig != null)
        {
            TralConfig.SetupTrail(Trail);
        }
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifetime);

        if (gameObject != null)
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
            if (((1 << other.gameObject.layer) & structureLayers) != 0)
            {
                photonView.RPC("RPC_SpawnImpactEffect", RpcTarget.All, transform.position, transform.rotation);
            }

            if (gameObject != null)
                PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    void RPC_SpawnImpactEffect(Vector3 position, Quaternion rotation)
    {
        if (impactEffect != null)
        {
            GameObject fx = Instantiate(impactEffect, position, rotation);
            Destroy(fx, 2f);
        }
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

}

