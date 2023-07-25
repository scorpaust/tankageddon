using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class MainMenu : MonoBehaviour
{
	[SerializeField]
	private TMP_Text queueStatusText;
    
	[SerializeField]
	private TMP_Text queueTimerText;
    
    [SerializeField]
    private TMP_Text findMatchButtonText;

    [SerializeField]
    private TMP_InputField joinCodeField;

    private bool isMatchmaking;

    private bool isCanceling;

    private bool isBusy;

    private float timeInQueue;

	private void Start()
	{
        if (ClientSingleton.Instance == null) return;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        queueStatusText.text = string.Empty;

        queueTimerText.text = string.Empty;
	}

	private void Update()
	{
        if (isMatchmaking)
        {
            timeInQueue += Time.deltaTime;

            TimeSpan ts = TimeSpan.FromSeconds(timeInQueue);

            queueTimerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
        }
	}

	public async void FindMatchPressed()
    {
        if (isCanceling) return;

        if (isMatchmaking)
        {
            queueStatusText.text = "Canceling...";

            isCanceling = true;

            await ClientSingleton.Instance.GameManager.CancelMatchmaking();

            isCanceling = false;

            isMatchmaking = false;

            isBusy = false;

            findMatchButtonText.text = "Find Match";

            queueStatusText.text = string.Empty;

            queueTimerText.text = string.Empty;

            return;
        }

        if (isBusy) return;

        ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade); 

        findMatchButtonText.text = "Cancel";

        queueStatusText.text = "Searching...";

        timeInQueue = 0f;

        isMatchmaking = true;

        isBusy = true;
    }

    private void OnMatchMade(MatchmakerPollingResult result)
    {
        switch (result)
        {
            case MatchmakerPollingResult.Success:
                queueStatusText.text = "Connecting...";
                break;

			case MatchmakerPollingResult.TicketCreationError:
				queueStatusText.text = "TicketCreationError";
				break;

			case MatchmakerPollingResult.TicketCancellationError:
				queueStatusText.text = "TicketCancellationError";
				break;

			case MatchmakerPollingResult.TicketRetrievalError:
				queueStatusText.text = "TicketRetrievalError";
				break;

			case MatchmakerPollingResult.MatchAssignmentError:
				queueStatusText.text = "MatchAssignmentError";
				break;
		}
	}

	public async void StartHost()
    {
        if (isBusy) return;

        isBusy = true;

        await HostSingleton.Instance.GameManager.StartHostAsync();

        isBusy = false;
    }

    public async void StartClient()
    {
		if (isBusy) return;

		isBusy = true;

		await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);

        isBusy = false;
    }

	public async void JoinAsync(Lobby lobby)
	{
		if (isBusy) return;

		isBusy = true;

		try
		{
			Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);

			string joinCode = joiningLobby.Data["JoinCode"].Value;

			await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}

		isBusy = false;
	}
}
