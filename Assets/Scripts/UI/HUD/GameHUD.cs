using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameHUD : NetworkBehaviour
{
    [SerializeField]
    private TMP_Text lobbyCodeText;

    private NetworkVariable<FixedString32Bytes> lobbyCode = new NetworkVariable<FixedString32Bytes>("");

    private void HandleLobbyCodeChanged(FixedString32Bytes oldCode, FixedString32Bytes newCode)
    {
        lobbyCodeText.text = newCode.ToString();
    }

	public override void OnNetworkSpawn()
	{
        if (IsClient)
        {
            lobbyCode.OnValueChanged += HandleLobbyCodeChanged;

            HandleLobbyCodeChanged("", lobbyCode.Value);
        }

        if (!IsHost) return;

        lobbyCode.Value = HostSingleton.Instance.GameManager.JoinCode;
	}

	public void LeaveGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.GameManager.Shutdown();
        }

        ClientSingleton.Instance.GameManager.Disconnect();
    }

	public override void OnNetworkDespawn()
	{
		if (IsClient)
		{
			lobbyCode.OnValueChanged -= HandleLobbyCodeChanged;
		}
	}
}
