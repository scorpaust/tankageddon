using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField]
    private TankPlayer player;

    [SerializeField]
    private TMP_Text playerNameText;

    // Start is called before the first frame update
    void Start()
    {
        HandleDisplayNameChanged(string.Empty, player.PlayerName.Value);

        player.PlayerName.OnValueChanged += HandleDisplayNameChanged;
    }

	private void HandleDisplayNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
	{
        playerNameText.text = newName.ToString();
	}

	private void OnDestroy()
	{
		player.PlayerName.OnValueChanged -= HandleDisplayNameChanged;
	}
}
