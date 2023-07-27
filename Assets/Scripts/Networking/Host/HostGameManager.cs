using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport;

public class HostGameManager : IDisposable
{
    private Allocation allocation;

    private NetworkObject playerPrefab;

    public string JoinCode { get; private set; }

    private string lobbyId;

    private const int maxConnections = 20;

    private const string GameSceneName = "Level01";

    public NetworkServer NetworkServer { get; private set; }

    public HostGameManager(NetworkObject playerPrefab)
    {
        this.playerPrefab = playerPrefab;
    }

    public async Task StartHostAsync(bool isPrivate)
    {
        try
        {
			allocation = await Relay.Instance.CreateAllocationAsync(maxConnections);
        }
        catch (Exception e)
        {
            Debug.Log(e);

            return;
        }

        try
        {
            JoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(JoinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);

            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

        transport.SetRelayServerData(relayServerData);

        try 
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();

            lobbyOptions.IsPrivate = isPrivate;

            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: JoinCode
                    )
                }
            };

            string playerName = PlayerPrefs.GetString(NameSelector.playerNameKey, "Missing Name");

            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}'s Lobby", maxConnections, lobbyOptions);

            lobbyId = lobby.Id; 

            HostSingleton.Instance.StartCoroutine(HeartBeatLobby(15));
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);

            return;
        }

        NetworkServer = new NetworkServer(NetworkManager.Singleton, playerPrefab); 

        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.playerNameKey, "Missing Name"),

            userAuthId = AuthenticationService.Instance.PlayerId
        };

        string payload = JsonUtility.ToJson(userData);

        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartHost();

        NetworkServer.OnClientLeft += HandleClientLeft;

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);

    }

	private string GetRegionOrQosDefault()
	{
		throw new NotImplementedException();
	}

	private IEnumerator HeartBeatLobby(float waitTimeSeconds) 
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);

            yield return delay;
        }
    }

    public void Dispose() 
    {
        Shutdown();
    }

	public async void Shutdown()
	{
        if (string.IsNullOrEmpty(lobbyId)) return;
		
		HostSingleton.Instance.StopCoroutine(nameof(HeartBeatLobby));

		try
		{
			await Lobbies.Instance.DeleteLobbyAsync(lobbyId);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}

		lobbyId = string.Empty;

		NetworkServer?.Dispose();
		
        NetworkServer.OnClientLeft -= HandleClientLeft;
	}

    private async void HandleClientLeft(string authId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, authId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
