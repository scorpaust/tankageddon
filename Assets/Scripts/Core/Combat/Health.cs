using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Health : NetworkBehaviour
{
    [SerializeField]
    private int maxHealth = 100;

    public int MaxHealth { get { return maxHealth; } private set {} }

    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();

    private bool isDead;

    public Action<Health> OnDie;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        CurrentHealth.Value = maxHealth;
    }

    public void TakeDamage(int damageValue)
    {
        ModifyHealth(-damageValue);
    }
    public void RestoreHealth(int healValue)
    {
        ModifyHealth(healValue);
    }

    private void ModifyHealth(int value)
    {
        if (isDead) return;

        int newHealth = CurrentHealth.Value + value;

        CurrentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        if (CurrentHealth.Value == 0)
        {
            OnDie?.Invoke(this);

            isDead = true;
        }
    }
}
