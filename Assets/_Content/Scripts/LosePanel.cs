using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LosePanel : MonoBehaviour
{
    [Header("Scene References")]
    public RectTransform Panel;

    private void Start()
    {
        Panel.DOScaleY(0f, 0f);
    }

    private void OnEnable()
    {
        Spelunker.SpelunkerDeathEvent += SpelunkerDeathEvent;
    }
    private void OnDisable()
    {
        Spelunker.SpelunkerDeathEvent -= SpelunkerDeathEvent;
    }

    private void SpelunkerDeathEvent()
    {
        Panel.DOScaleY(1f, 0.2f).SetEase(Ease.OutFlash);
    }
}
