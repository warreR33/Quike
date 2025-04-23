using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwipe : MonoBehaviour
{
    private Quaternion startRotation;
    public float swayAmount = 8;


    void Start()
    {
        startRotation = transform.localRotation;
    }

    void Update()
    {
        Sway();
    }

    private void Sway()
    {

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Quaternion xAngle = Quaternion.AngleAxis(mouseX * -3f, Vector3.up);
        Quaternion yAngle = Quaternion.AngleAxis(mouseY * -3f, Vector3.left);
        Quaternion targetRotation = startRotation * xAngle * yAngle;

        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * swayAmount);
    }
}
