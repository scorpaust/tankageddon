using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class CoinWallet : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private BountyCoin coinPrefab;

    [SerializeField]
    private Health health;

    [Header("Settings")]

    [SerializeField]
    private int bountyCoinCount = 10;

    [SerializeField]
    private int minBountyCoinValue = 5;

    [SerializeField]
    private float coinSpread = 4f;

	[SerializeField]
	private float bountyPercentage = 25f;

	[SerializeField]
    private LayerMask layerMask;

	private float coinRadius;

	private Collider2D[] coinBuffer = new Collider2D[1];

	public NetworkVariable<int> totalCoins;

	public override void OnNetworkSpawn()
	{
        if (!IsServer) return;

		coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        health.OnDie += HandleDie;
	}

	public override void OnNetworkDespawn()
	{
		if (!IsServer) return;

		health.OnDie -= HandleDie;
	}

	private void HandleDie(Health health)
	{
        int bountyValue = (int)(totalCoins.Value * (bountyPercentage / 100f));

        int bountyCoinValue = bountyValue / bountyCoinCount;

        if (bountyCoinValue < minBountyCoinValue) return;

        for (int i = 0; i < bountyCoinCount; i++)
        {
			BountyCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);

			coinInstance.SetValue(bountyCoinValue);

			coinInstance.NetworkObject.Spawn();
        }
	}

	private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!other.TryGetComponent<Coin>(out Coin coin)) { return; }

        int coinValue = coin.Collect();

        if (!IsServer) return;

        totalCoins.Value += coinValue;
    }

    public void SpendCoins(int costToFire)
    {
        totalCoins.Value -= costToFire;
    }

	private Vector2 GetSpawnPoint()
	{
		while (true)
		{
			Vector2 spawnPoint = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * coinSpread;

			int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);

			if (numColliders == 0)
			{
				return spawnPoint;
			}
		}
	}
}
