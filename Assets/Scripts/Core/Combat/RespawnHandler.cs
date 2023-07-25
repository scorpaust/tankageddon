using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField]
    private TankPlayer playerPrefab;

	[SerializeField]
	private float keptCoinPercentage;

	public override void OnNetworkSpawn()
	{
		if (!IsServer) return;

		TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);

		foreach (TankPlayer player in players)
		{
			HandlePlayerSpawned(player);
		}
 
		TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;

		TankPlayer.OnPlayerSpawned += HandlePlayerDespawned;
	}

	private void HandlePlayerDespawned(TankPlayer player)
	{
		player.Health.OnDie -= (health) => HandlePlayerDied(player);
	}

	private void HandlePlayerSpawned(TankPlayer player)
	{
		player.Health.OnDie += (health) => HandlePlayerDied(player);
	}

	private void HandlePlayerDied(TankPlayer player)
	{
		int keptCoins = (int)(player.Wallet.totalCoins.Value * (keptCoinPercentage / 100)); 
		
		Destroy(player.gameObject);

		StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins));
	}

	public override void OnNetworkDespawn()
	{
		if (!IsServer) return;

		TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;

		TankPlayer.OnPlayerSpawned -= HandlePlayerDespawned;
	}

	private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins)
	{
		yield return null;

		TankPlayer playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

		playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);

		playerInstance.Wallet.totalCoins.Value += keptCoins;
	}
}
