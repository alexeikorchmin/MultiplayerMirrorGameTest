using System;
using UnityEngine;
using TMPro;
using Mirror;

public class PlayerManagerUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text blinkCooldownText;

    private float blinkCooldown;

    private void Update()
    {
        ShowBlinkCooldown();
    }

    private void ShowBlinkCooldown()
    {
        blinkCooldown = PlayerBlink.GetBlinkCooldown();
        blinkCooldownText.text = $"Cooldown: {Math.Round(blinkCooldown, 2).ToString()}";
    }
}