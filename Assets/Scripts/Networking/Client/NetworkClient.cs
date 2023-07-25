using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class NetworkClient : IDisposable
{
    private NetworkManager networkManager;

    private string menuSceneName = "Menu";

    public NetworkClient(NetworkManager networkManager) 
    {
        this.networkManager = networkManager;

        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    } 

    private void OnClientDisconnect(ulong clientId) 
    {
        if (clientId != 0 && clientId != networkManager.LocalClientId)
        {
            return;
        }

        Disconnect();
    }

    public void Dispose() 
    {   
        if (networkManager != null) 
        {
            networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

	public void Disconnect()
	{
		if (SceneManager.GetActiveScene().name != menuSceneName)
		{
			SceneManager.LoadScene(menuSceneName);
		}

		if (networkManager.IsConnectedClient)
		{
			networkManager.Shutdown();
		}
	}
}
