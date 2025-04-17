using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LadderClimbing : MonoBehaviourPun
{
    private PlayerMovement playerMovement;
    private Rigidbody rb;

    public bool isClimbing;
    private bool nearLadder;
    private Transform ladderTransform;

    [Header("Climbing Settings")]
    public float climbSpeed = 3f;
    public LayerMask ladderLayer;
    public float ladderCheckDistance = 1f;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        LadderCheck();

        if (nearLadder && Input.GetKey(KeyCode.W))
        {
            StartClimbing();
        }
        else if (!nearLadder || Input.GetKeyUp(KeyCode.W))
        {
            StopClimbing();
        }

        if (isClimbing)
        {
            Climb();
        }
    }

    private void LadderCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, ladderCheckDistance, ladderLayer))
        {
            nearLadder = true;
            ladderTransform = hit.transform;
        }
        else
        {
            nearLadder = false;
            ladderTransform = null;
        }
    }

    private void StartClimbing()
    {
        if (!isClimbing)
        {
            isClimbing = true;
            rb.useGravity = false;
        }
    }

    private void StopClimbing()
    {
        if (isClimbing)
        {
            isClimbing = false;
            rb.useGravity = true;
        }
    }

    private void Climb()
    {
        Vector3 climbDirection = Vector3.up;
        rb.velocity = climbDirection * climbSpeed;
    }
}
