using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Shooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject grenadePrefab;

    public GameObject pistolModel;
    public GameObject grenadeLauncherModel;

    public Transform shootPoint;
    public Camera playerCamera;
    public Image crosshairImage;

    public float pistolFireRate = 0.5f;
    public float grenadeCooldown = 20f;

    private float lastPistolShotTime = -999f;
    private float lastGrenadeShotTime = -999f;


    private PhotonView photonView;


    private float lastShotTime;

    private enum WeaponType { Pistol, GrenadeLauncher }
    private WeaponType currentWeapon = WeaponType.Pistol;

     private void Start()
    {
        photonView = GetComponent<PhotonView>();
        EquipWeapon(WeaponType.Pistol);
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        HandleWeaponSwitch();
        HandleCrosshair();
        HandleShooting();
    }

    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            EquipWeapon(WeaponType.Pistol);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            EquipWeapon(WeaponType.GrenadeLauncher);
    }

    void EquipWeapon(WeaponType weapon)
    {
        currentWeapon = weapon;
        pistolModel.SetActive(weapon == WeaponType.Pistol);
        grenadeLauncherModel.SetActive(weapon == WeaponType.GrenadeLauncher);
    }

    void HandleCrosshair()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 30f))
        {
            if (crosshairImage && hit.collider.GetComponent<IDamageable>() != null)
                crosshairImage.color = Color.red;
            else if (crosshairImage)
                crosshairImage.color = Color.white;
        }
    }

    void HandleShooting()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 30f))
            targetPoint = hit.point;
        else
            targetPoint = ray.origin + ray.direction * 30f;

        Vector3 direction = (targetPoint - shootPoint.position).normalized;

        if (currentWeapon == WeaponType.Pistol)
        {
            if (Time.time - lastPistolShotTime >= pistolFireRate)
            {
                lastPistolShotTime = Time.time;

                GameObject projectile = PhotonNetwork.Instantiate(projectilePrefab.name, shootPoint.position, Quaternion.LookRotation(direction));
                projectile.GetComponent<Projectile>().SetAttacker(photonView.ViewID);
            }
        }
        else if (currentWeapon == WeaponType.GrenadeLauncher)
        {
            if (Time.time - lastGrenadeShotTime >= grenadeCooldown)
            {
                lastGrenadeShotTime = Time.time;

                GameObject grenade = PhotonNetwork.Instantiate(grenadePrefab.name, shootPoint.position, Quaternion.identity);
                grenade.GetComponent<Rigidbody>().velocity = direction * 15f;
                grenade.GetComponent<GrenadeProjectile>().SetAttacker(photonView.ViewID);
            }
            else
            {
                float timeLeft = grenadeCooldown - (Time.time - lastGrenadeShotTime);
                Debug.Log($"Lanzagranadas en cooldown: {timeLeft:F1}s restantes");
            }
        }
    }

}

