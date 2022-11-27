using System;
using UnityEngine;
using TMPro;

public class PlayerScreenManagerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text blinkCooldownText;

    private float blinkCooldown;

    private void Update()
    {
        //ShowBlinkCooldown();
    }

    private void ShowBlinkCooldown()
    {
        //blinkCooldown = PlayerBlink.GetBlinkCooldown();
        //print($"Player {PlayerBlink.GetBlinkPlayer()} UI_Blink cd = {blinkCooldown}");
        //blinkCooldownText.text = $"Cooldown: {Math.Round(blinkCooldown, 2)}";
    }
}