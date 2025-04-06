using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UI_RunQualityTracker : MonoBehaviour
{
    private TextMeshProUGUI _text;

    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        UI_HealthTracker.RunQualityChangedEvent += RunQualityChangedEvent;
        Spelunker.SpelunkerTookDamageEvent += SpelunkerTookDamageEvent;
    }

    void OnDisable()
    {
        UI_HealthTracker.RunQualityChangedEvent -= RunQualityChangedEvent;
        Spelunker.SpelunkerTookDamageEvent -= SpelunkerTookDamageEvent;
    }

    private void RunQualityChangedEvent(UI_HealthTracker.RunQuality currentRunQuality, Color color)
    {
        _text.color = color;
        _text.text = currentRunQuality.ToString();
    }

    private void SpelunkerTookDamageEvent(int damage, int currentHealth)
    {
        _text.rectTransform.DOShakeScale(0.1f, 0.4f);
    }
}
