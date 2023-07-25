using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealingZone : NetworkBehaviour
{
    [Header("References")]    
    [SerializeField]
    private Image healPowerBar;

    [Header("Settings")]
    [SerializeField]
    private int maxHealPower = 30;

    [SerializeField]
    private float healCooldown = 60f;

    [SerializeField]
    private float healTickRate = 1f;

    [SerializeField]
    private int coinsPerTick = 10;

	[SerializeField]
	private int healthPerTick = 10;

	private float remainingCooldown;

	private float tickTimer;

    private List<TankPlayer> playersInZone = new List<TankPlayer>();

    private NetworkVariable<int> healPower = new NetworkVariable<int>();

	public override void OnNetworkSpawn()
	{
		if (IsClient)
		{
			healPower.OnValueChanged += HandleHealPowerChanged;

			HandleHealPowerChanged(0, healPower.Value);
		}

		if (IsServer)
		{
			healPower.Value = maxHealPower;
		}
	}

	public override void OnNetworkDespawn()
	{
		if (IsClient)
		{
			healPower.OnValueChanged -= HandleHealPowerChanged;
		}
	}

	private void Update()
	{
		if (!IsServer) return;

		if (remainingCooldown > 0f)
		{
			remainingCooldown -= Time.deltaTime;

			if (remainingCooldown <= 0f)
			{
				healPower.Value = maxHealPower;
			}
			else
			{
				return;
			}
		}

		tickTimer += Time.deltaTime;

		if (tickTimer >= 1 / healTickRate)
		{
			foreach (TankPlayer player in playersInZone)
			{
				if (healPower.Value == 0) break;

				if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) continue;

				if (player.Wallet.totalCoins.Value < coinsPerTick) continue;

				player.Wallet.SpendCoins(coinsPerTick);

				player.Health.RestoreHealth(healthPerTick);

				healPower.Value -= 1;

				if (healPower.Value == 0)
				{
					remainingCooldown = healCooldown;
				}
			}

			tickTimer = tickTimer % (1 / healTickRate);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
        if (!IsServer) return;

        if (!collision.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player)) return;

        playersInZone.Add(player);
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!IsServer) return;

		if (!collision.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player)) return;

		playersInZone.Remove(player);
	}

	private void HandleHealPowerChanged(int oldHealPower, int newHealPower)
	{
		healPowerBar.fillAmount = (float)newHealPower / maxHealPower;
	}
}
