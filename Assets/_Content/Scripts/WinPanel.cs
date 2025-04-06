using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class WinPanel : MonoBehaviour
{
    [Header("Scene References")]
    public RectTransform Panel;
    public TextMeshProUGUI CleanRunText;

    private UI_HealthTracker.RunQuality _currentRunQuality;

    private void Start()
    {
        Panel.DOScaleY(0f, 0f);
        CleanRunText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GoalCavern.GoalReachedEvent += GoalReachedEvent;
        UI_HealthTracker.RunQualityChangedEvent += RunQualityChangedEvent;
    }
    private void OnDisable()
    {
        GoalCavern.GoalReachedEvent -= GoalReachedEvent;
        UI_HealthTracker.RunQualityChangedEvent -= RunQualityChangedEvent;
    }

    private void RunQualityChangedEvent(UI_HealthTracker.RunQuality currentRunQuality, Color color)
    {
        _currentRunQuality = currentRunQuality;
    }

    private void GoalReachedEvent()
    {
        CleanRunText.gameObject.SetActive(_currentRunQuality == UI_HealthTracker.RunQuality.Clean);
        Panel.DOScaleY(1f, 0.2f).SetEase(Ease.OutFlash);
    }
}
