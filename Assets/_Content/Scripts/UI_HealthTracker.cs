using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.UI.ProceduralImage;

[RequireComponent(typeof(Slider))]
public class UI_HealthTracker : MonoBehaviour
{
    public delegate void RunQualityChangedHandler(RunQuality currentRunQuality, Color color);
    public static event RunQualityChangedHandler RunQualityChangedEvent;

    public enum RunQuality
    {
        Clean,
        Dicey,
        Erratic,
        Failed
    }

    [Header("Scene References")]
    public Spelunker Spelunker;
    public ProceduralImage Fill;

    [Header("Settings")]
    public int StartOfDicey;
    public int StartOfErratic;
    public Color CleanColor;
    public Color DiceyColor;
    public Color ErraticColor;
    public Color FailedColor;

    private Slider _slider;
    private RunQuality _currentRunQuality;

    void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.minValue = 0;
        _slider.maxValue = Spelunker.MaxHealth;
        _slider.value = Spelunker.MaxHealth;
    }

    void Start()
    {
        _currentRunQuality = RunQuality.Clean;
        Fill.color = CleanColor;
        if (RunQualityChangedEvent != null)
        {
            RunQualityChangedEvent(_currentRunQuality, CleanColor);
        }
    }

    void OnEnable()
    {
        Spelunker.SpelunkerTookDamageEvent += SpelunkerTookDamageEvent;
    }

    void OnDisable()
    {
        Spelunker.SpelunkerTookDamageEvent -= SpelunkerTookDamageEvent;
    }

    private void SpelunkerTookDamageEvent(int damage, int currentHealth)
    {
        _slider.DOValue(currentHealth, 0.2f);

        if (currentHealth <= StartOfDicey && _currentRunQuality == RunQuality.Clean)
        {
            _currentRunQuality = RunQuality.Dicey;
            Fill.color = DiceyColor;
            if (RunQualityChangedEvent != null)
            {
                RunQualityChangedEvent(_currentRunQuality, DiceyColor);
            }
        }
        else if (currentHealth <= StartOfErratic && _currentRunQuality == RunQuality.Dicey)
        {
            _currentRunQuality = RunQuality.Erratic;
            Fill.color = ErraticColor;
            if (RunQualityChangedEvent != null)
            {
                RunQualityChangedEvent(_currentRunQuality, ErraticColor);
            }
        }
        else if (currentHealth <= 0 && _currentRunQuality == RunQuality.Erratic)
        {
            _currentRunQuality = RunQuality.Failed;
            Fill.color = FailedColor;
            if (RunQualityChangedEvent != null)
            {
                RunQualityChangedEvent(_currentRunQuality, FailedColor);
            }
        }
    }
}
