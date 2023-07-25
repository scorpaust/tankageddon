using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using Unity.Collections;

public class TankPlayer : NetworkBehaviour
{
	[Header("References")]
	[SerializeField]
	private CinemachineVirtualCamera virtualCamera;

	[SerializeField]
	private SpriteRenderer minimapIconRenderer;

	[SerializeField]
	private Texture2D crosshair;

	[field: SerializeField]
	public Health Health { get; private set; }

	[field: SerializeField]
	public CoinWallet Wallet { get; private set; }

	[Header("Settings")]
	[SerializeField]
	private int ownerPriority = 15;

	[SerializeField]
	private Color enemyColor;

	public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

	public static event Action<TankPlayer> OnPlayerSpawned;

	public static event Action<TankPlayer> OnPlayerDespawned;

	public override void OnNetworkSpawn()
	{
		if (IsServer)
		{
			UserData userData = null;

			if (IsHost)
			{
				userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
			}
			else
			{
				userData = ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
			}

			PlayerName.Value = userData.userName;

			OnPlayerSpawned?.Invoke(this);
		}

		if (IsOwner)
		{
			virtualCamera.Priority = ownerPriority;

			Cursor.SetCursor(crosshair, new Vector2(crosshair.width / 2, crosshair.height / 2), CursorMode.Auto);
		}

		if (!IsOwner)
		{
			minimapIconRenderer.color = enemyColor;
		}
	}

	public override void OnNetworkDespawn()
	{
		if (IsServer)
		{
			OnPlayerDespawned?.Invoke(this);
		}
	}
}