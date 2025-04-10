using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class PlayerCam : MonoBehaviour
{
    private PhotonView photonView;

    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform playerBody;

    float xRotation;
    float yRotation;

    private void Start()
    {
        photonView = GetComponentInParent<PhotonView>();

        if (!photonView.IsMine)
        {
            GetComponent<Camera>().enabled = false;

            //Asi solo la cam local tiene un audiolistener activo 
            AudioListener audioListener = GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }

            this.enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        //Rotamos el cuerpo
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        playerBody.rotation = Quaternion.Euler(0, yRotation, 0); 
    }
}

