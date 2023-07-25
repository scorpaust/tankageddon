using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private InputReader inputReader;

    [SerializeField]
    private Transform bodyTransform;

    [SerializeField]
    private Rigidbody2D rb;

	[SerializeField]
	private ParticleSystem dustCloud;

	[Header("Settings")]

    [SerializeField]
    private float movementSpeed = 4f;

    [SerializeField]
    private float turningRate = 270f;

	[SerializeField]
	private float particleEmissionValue = 10;

    private const float ParticleStopThreshold = 0.005f;

	private ParticleSystem.EmissionModule emissionModule;

	private Vector2 previousMovementInput;

    private Vector3 prevPos;

	private void Awake()
	{
        emissionModule = dustCloud.emission;
	}

	public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputReader.MoveEvent += HandleMove;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        inputReader.MoveEvent -= HandleMove;
    }

    private void FixedUpdate() 
    {
        if ((transform.position - prevPos).sqrMagnitude > ParticleStopThreshold)
        {
            emissionModule.rateOverTime = particleEmissionValue;
        }
        else
        {
            emissionModule.rateOverTime = 0;
        }

        prevPos = transform.position;

        if (!IsOwner) return;

        rb.velocity = (Vector2)bodyTransform.up * previousMovementInput.y * movementSpeed;    
    }

    private void Update()
    {
        if (!IsOwner) return;

        float zRotation = previousMovementInput.x * -turningRate * Time.deltaTime;

        bodyTransform.Rotate(0f, 0f, zRotation);
    }

    private void HandleMove(Vector2 movementInput)
    {
        previousMovementInput = movementInput;
    }
}
