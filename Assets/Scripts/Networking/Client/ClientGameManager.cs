using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class ClientGameManager : IDisposable
{
    private JoinAllocation allocation;

    private const string MenuSceneName = "Menu";

    private NetworkClient networkClient;

    private MatchplayMatchmaker matchplayMatchmaker;

    private UserData userData;

    public async Task<bool> InitAsync()
    {
        // Authenticate player
        await UnityServices.InitializeAsync();

        networkClient = new NetworkClient(NetworkManager.Singleton);

        matchplayMatchmaker = new MatchplayMatchmaker();

        AuthState authState = await AuthenticationWrapper.DoAuth();

        if (authState == AuthState.Authenticated)
        {
			userData = new UserData
			{
				userName = PlayerPrefs.GetString(NameSelector.playerNameKey, "Missing Name"),
				userAuthId = AuthenticationService.Instance.PlayerId
			};

			return true;
        }

        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public void StartClient(string ip, int port)
    {
		UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetConnectionData(ip, (ushort)port);

        ConnectClient(userData);
	}

    public async Task StartClientAsync(string joinCode)
    {
		if (String.IsNullOrEmpty(joinCode))
		{
			Debug.LogError("Please input a join code.");
			return;
		}
		try 
        {
            allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        }
        catch(Exception e)
        {
            Debug.Log(e);

            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

        transport.SetRelayServerData(relayServerData);

        ConnectClient(userData);
    }

    private void ConnectClient(UserData userData) 
    {
        string payload = JsonUtility.ToJson(userData);

        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
    }

    public async void MatchmakeAsync(Action<MatchmakerPollingResult> onMatchmakeResponse)
    {
        if (matchplayMatchmaker.IsMatchmaking) return;

        MatchmakerPollingResult matchResult = await GetMatchAsync();

        onMatchmakeResponse?.Invoke(matchResult);
    }

    private async Task<MatchmakerPollingResult> GetMatchAsync()
    {
        MatchmakingResult matchmakingResult = await matchplayMatchmaker.Matchmake(userData);

        if (matchmakingResult.result == MatchmakerPollingResult.Success)
        {
            // Connect to server
            StartClient(matchmakingResult.ip, matchmakingResult.port);
        }

        return matchmakingResult.result;
    }

    public void Dispose() 
    {
        networkClient?.Dispose();
    }

	public void Disconnect()
	{
        networkClient.Disconnect();
	}

	public async Task CancelMatchmaking()
	{
        await matchplayMatchmaker.CancelMatchmaking();
	}
}
