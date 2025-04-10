using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float fireRate = 0.5f;

    private PhotonView photonView;


    private float lastShotTime;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();

    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetMouseButton(0) && Time.time - lastShotTime > fireRate)
        {
            lastShotTime = Time.time;
            PhotonNetwork.Instantiate(projectilePrefab.name, shootPoint.position, shootPoint.rotation);
        }
    }

}

