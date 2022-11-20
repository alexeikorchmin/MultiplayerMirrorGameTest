using System;
using UnityEngine;
using TMPro;

public class PlayerManagerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text blinkCooldownText;

    private void Awake()
    {
        PlayerBlink.OnBlinkValue += OnBlinkValueHandler;
    }

    private void OnDestroy()
    {
        PlayerBlink.OnBlinkValue -= OnBlinkValueHandler;
    }

    private void OnBlinkValueHandler(float blinkClickTime)
    {
        blinkCooldownText.text = $"Cooldown: {Math.Round(blinkClickTime, 2).ToString()}";
    }
}