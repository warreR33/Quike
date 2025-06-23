using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HitFX : MonoBehaviourPun
{
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private Transform particleSpawnPoint;



    public void ShowDamage()
    {
        photonView.RPC("RPC_ShowHitFX", RpcTarget.All, particleSpawnPoint.position);
    }

    [PunRPC]
    void RPC_ShowHitFX(Vector3 pos)
    {
        if (damageEffectPrefab != null)
        {
            Debug.Log("Si, si senorrr");
            GameObject fx = Instantiate(damageEffectPrefab, pos, Quaternion.identity);
            Destroy(fx, 2f);
        }
    }
}
