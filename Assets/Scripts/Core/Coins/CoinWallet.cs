using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> totalCoins;

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
}
