using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UI_DepthTracker : MonoBehaviour
{
    [Header("Scene References")]
    public Spelunker Spelunker;
    public Ground Ground;

    [Header("Settings")]
    public float DepthPerDeepGroundTile;
    public float StartYValue;

    private TextMeshProUGUI _text;

    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        var depthPerYUnit = DepthPerDeepGroundTile / Ground.DeepGroundTileHeight;
        var currentSpelunkerDepth = (Spelunker.transform.position.y - StartYValue) * depthPerYUnit * -1f;
        _text.text = $"Depth {Mathf.RoundToInt(currentSpelunkerDepth)}m / {Mathf.RoundToInt(Ground.NumDeepGroundLayers * DepthPerDeepGroundTile)}m";
    }
}
