using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField]
    private int damage = 5;

    [SerializeField]
    private Projectile projectile;

    private ulong ownerClientId;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.attachedRigidbody == null) return;

        if (projectile.TeamIndex != -1)
        {
			if (other.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player))
			{
                if (player.TeamIndex.Value == projectile.TeamIndex) return;
			}
		}

        if (other.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(damage);
        }    
    }
}
