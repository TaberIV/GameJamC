﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ControllerInputProvider))]
[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(FirstPersonCamera))]
public class Teleporting : MonoBehaviour
{
    public float MaxDistance = 30;
    public float minDistance = 5;
    public float TeleportSpeed = 20f;

    public ParticleSystem IndicatorEffect;
    public LayerMask TeleportLayer = -1;

    private CharacterMovement characterMovement;
    private CharacterInputProvider inputProvider;
    private FirstPersonCamera playerCamera;
    private ParticleSystem indicator = null;

    private RaycastHit hitRay, teleRay;
    private bool hit, teleHit;

    private bool teleport;
    public bool Teleport
    {
        get { return teleport; }
        set
        {
            teleport = value;
            characterMovement.HandleMovement = !value;
        }
    }
    private Vector3 target;

    private Vector3 heightOffset = new Vector3(0, 2, 0);

    // Use this for initialization
    void Start()
    {
        inputProvider = GetComponent<CharacterInputProvider>();

        playerCamera = GetComponent<FirstPersonCamera>();
        characterMovement = GetComponent<CharacterMovement>();
    }

    // FixedUpdate is called 60 times per second
    private void FixedUpdate()
    {
        Vector3 cameraPosition = playerCamera.CameraPosition;
        Vector3 cameraDirection = playerCamera.CameraDirection;

        hit = Physics.Raycast(cameraPosition, cameraDirection, out hitRay, MaxDistance, -1, QueryTriggerInteraction.Ignore);
        teleHit = Physics.Raycast(cameraPosition, cameraDirection, out teleRay, MaxDistance, TeleportLayer.value, QueryTriggerInteraction.Ignore);
    }
    private void Update()
    {
        if (Teleport)
        {
            Vector3 path = target - transform.position;
            Vector3 velocity = path.normalized * TeleportSpeed;

            characterMovement.Move(velocity * Time.deltaTime);

            if (path.magnitude < 1)
            {
                Teleport = false;
            }
        }
        else
        {
            if (hit)
            {
                float hitDistance = Vector3.Distance(transform.position, hitRay.point);
                float teleDistance = Vector3.Distance(transform.position, teleRay.point);
                teleHit = teleHit && teleDistance > minDistance && teleDistance <= hitDistance;

                if ((!teleHit || !(inputProvider.Teleport || inputProvider.TeleportUp)) && indicator != null)
                {
                    Destroy(indicator.gameObject);
                    indicator = null;
                }
                else if (teleHit && inputProvider.Teleport && !Teleport)
                {
                    if (indicator == null)
                    {
                        indicator = Instantiate(IndicatorEffect, teleRay.point, Quaternion.Euler(0, 0, 0));
                    }
                    else
                    {
                        indicator.transform.position = teleRay.point;
                    }
                }
                else if (teleHit && inputProvider.TeleportUp)
                {
                    if (indicator != null)
                    {
                        Destroy(indicator.gameObject);
                        indicator = null;
                    }

                    target = teleRay.point + heightOffset;
                    // Teleport = true;
                }
            }
            else if (indicator != null)
            {
                Destroy(indicator.gameObject);
                indicator = null;
            }
        }

        if (transform.position.y < -50)
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }
}
