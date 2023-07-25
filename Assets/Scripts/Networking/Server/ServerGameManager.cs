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
using Unity.Services.Matchmaker.Models;

public class ServerGameManager : IDisposable
{
	private string serverIP;

	private int serverPort;

	private int queryPort;

	private MultiplayAllocationService multiplayAllocationService;

	private MatchplayBackfiller backfiller;

	public NetworkServer NetworkServer { get; private set; }

	public ServerGameManager (string serverIP, int serverPort, int queryPort, NetworkManager manager, NetworkObject playerPrefab)
	{
		this.serverIP = serverIP;

		this.serverPort = serverPort;

		this.queryPort = queryPort;

		this.NetworkServer = new NetworkServer(manager, playerPrefab);

		multiplayAllocationService = new MultiplayAllocationService();
	}


	public async Task StartGameServerAsync()
	{
		await multiplayAllocationService.BeginServerCheck();

		try
		{
			MatchmakingResults matchmakingPayload = await GetMatchmakerPayload();

			if (matchmakingPayload != null)
			{
				await StartBackfill(matchmakingPayload);

				NetworkServer.OnUserJoined += UserJoined;

				NetworkServer.OnUserLeft += UserLeft;
			}
			else
			{
				Debug.Log("Matchmaker payload timed out.");
			}
		}
		catch (Exception e)
		{
			Debug.LogWarning(e);
		}

		if (!NetworkServer.OpenConnection(serverIP, serverPort))
		{
			Debug.LogError("Connection to the NetworkServer failed.");

			return;
		}
	}

	private async Task StartBackfill(MatchmakingResults payload)
	{
		backfiller = new MatchplayBackfiller($"{serverIP}:{serverPort}", payload.QueueName, payload.MatchProperties, 20);

		if (backfiller.NeedsPlayers())
		{
			await backfiller.BeginBackfilling();
		}
	}

	private async Task<MatchmakingResults> GetMatchmakerPayload()
	{
		Task<MatchmakingResults> matchmakerPayload = multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

		if (await Task.WhenAny(matchmakerPayload, Task.Delay(20000)) == matchmakerPayload)
		{
			return matchmakerPayload.Result;
		}

		return null;
	}

	private void  UserJoined(UserData user)
	{
		backfiller.AddPlayerToMatch(user);

		multiplayAllocationService.AddPlayer();

		if (!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
		{
			_ = backfiller.StopBackfill();
		}
	}

	private void UserLeft(UserData user)
	{
		int playerCount = backfiller.RemovePlayerFromMatch(user.userAuthId);

		multiplayAllocationService.RemovePlayer();

		if (playerCount <= 0)
		{
			CloseServer();

			return;
		}

		if (backfiller.NeedsPlayers() && !backfiller.IsBackfilling)
		{
			_ = backfiller.BeginBackfilling();
		}
	}

	private async void CloseServer()
	{
		await backfiller.StopBackfill();

		Dispose();

		Application.Quit();
	}

	public void Dispose()
	{
		NetworkServer.OnUserJoined -= UserJoined;

		NetworkServer.OnUserLeft -= UserLeft;

		backfiller?.Dispose();

		multiplayAllocationService?.Dispose();

		NetworkServer?.Dispose();
	}
}