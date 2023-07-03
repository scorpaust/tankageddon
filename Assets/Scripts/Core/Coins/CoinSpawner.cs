using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField]
    private RespawningCoin coinPrefab;

    [SerializeField]
    private int maxCoins = 50;

    [SerializeField]
    private int coinValue = 10;

    [SerializeField]
    private Vector2 xSpawnRange;

    [SerializeField]
    private Vector2 ySpawnRange;

    [SerializeField]
    private LayerMask layerMask;

    private float coinRadius;

    private Collider2D[] coinBuffer = new Collider2D[1];

    private void SpawnCoin()
    {
        RespawningCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);

        coinInstance.SetValue(coinValue);

        coinInstance.GetComponent<NetworkObject>().Spawn();

        coinInstance.OnCollected += HandleCoinCollected;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius; 

        for (int i = 0; i < maxCoins; i++)
        {
            SpawnCoin();
        }
    }

    private void HandleCoinCollected(RespawningCoin coin)
    {
        coin.transform.position = GetSpawnPoint();

        coin.Reset();
    }

    private Vector2 GetSpawnPoint()
    {
        float x = 0;

        float y = 0;

        while(true)
        {
            x = Random.Range(xSpawnRange.x, xSpawnRange.y);

            y = Random.Range(ySpawnRange.x, ySpawnRange.y);

            Vector2 spawnPoint = new Vector2(x, y);

            int numColliders= Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);

            if (numColliders == 0)
            {
                return spawnPoint;
            }
        }
    }
}
