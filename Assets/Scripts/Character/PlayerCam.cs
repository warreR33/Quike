using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class PlayerCam : MonoBehaviour
{
    private PhotonView photonView;

    public MeshRenderer playerBodyRenderer;
    public MeshRenderer playerEye1Renderer;
    public MeshRenderer playerEye2Renderer;

    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform playerBody;

    float xRotation;
    float yRotation;

    private void Start()
    {

        photonView = GetComponentInParent<PhotonView>();

        //Evitar Clipping cuerpo propio
        if (photonView.IsMine)
        {
            if (playerBodyRenderer != null || playerEye1Renderer != null || playerEye2Renderer != null)
            {
                playerEye1Renderer.enabled = false;
                playerEye2Renderer.enabled = false;
                playerBodyRenderer.enabled = false;  
            }
        }
        else
        {
            //Si no es el jugador local ocultamos camara y audiolistener
            GetComponent<Camera>().enabled = false;

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
        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        playerBody.rotation = Quaternion.Euler(0, yRotation, 0); 
    }
}

