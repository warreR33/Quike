using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Shooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float fireRate = 0.5f;
    public Camera playerCamera;
    public Image crosshairImage;

    private PhotonView photonView;


    private float lastShotTime;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();

    }

    void Update()
    {
        if (!photonView.IsMine) return;

        //Cambiar color de la mira si apunta a enemigo
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (crosshairImage && hit.collider.GetComponent<IDamageable>() != null)
                crosshairImage.color = Color.red;
            else if (crosshairImage)
                crosshairImage.color = Color.white;
        }

        //Disparo
        if (Input.GetMouseButton(0) && Time.time - lastShotTime > fireRate)
        {
            lastShotTime = Time.time;

            Vector3 targetPoint;

            if (hit.collider != null)
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.origin + ray.direction * 100f;
            }

            Vector3 direction = (targetPoint - shootPoint.position).normalized;

            GameObject projectile = PhotonNetwork.Instantiate(projectilePrefab.name, shootPoint.position, Quaternion.LookRotation(direction));
            projectile.GetComponent<Projectile>().SetAttacker(photonView.ViewID);
        }
    }

}

