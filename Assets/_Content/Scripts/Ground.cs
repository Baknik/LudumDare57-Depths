using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    private enum DeepGroundTileType
    {
        Normal,
        Narrow,
        Wide,
        ExtraWide
    }

    [Header("Scene References")]
    public Transform DeepGroundTileParent;

    [Header("Prefabs")]
    public Transform NarrowWidthDeepGroundPrefab;
    public Transform NormalWidthDeepGroundPrefab;
    public Transform WideWidthDeepGroundPrefab;
    public Transform ExtraWideWidthDeepGroundPrefab;
    public GoalCavern GoalCavernPrefab;

    [Header("Settings")]
    public float DeepGroundTileHeight;
    public float StartYGenerationPosition;
    public float XIncrementSize;
    public int NumDeepGroundLayers;
    public int IterationsBetweenNarrows;
    public float MinX;
    public float MaxX;

    private float _xGenerationPosition;
    private float _yGenerationPosition;
    private int _iterationsSinceLastNarrow;
    private Dictionary<DeepGroundTileType, Transform> DeepGroundPrefabDictionary;

    private void Start()
    {
        _xGenerationPosition = 0f;
        _yGenerationPosition = StartYGenerationPosition;
        _iterationsSinceLastNarrow = 0;

        DeepGroundPrefabDictionary = new Dictionary<DeepGroundTileType, Transform>();
        DeepGroundPrefabDictionary[DeepGroundTileType.Narrow] = NarrowWidthDeepGroundPrefab;
        DeepGroundPrefabDictionary[DeepGroundTileType.Normal] = NormalWidthDeepGroundPrefab;
        DeepGroundPrefabDictionary[DeepGroundTileType.Wide] = WideWidthDeepGroundPrefab;
        DeepGroundPrefabDictionary[DeepGroundTileType.ExtraWide] = ExtraWideWidthDeepGroundPrefab;

        GenerateDeepGround();
    }

    private void GenerateDeepGround()
    {
        PlaceDeepGroundTile(DeepGroundTileType.Normal);
        RandomlyIncrementXGenerationPosition();
        IncrementYGenerationPosition();

        for (int i = 0; i < NumDeepGroundLayers - 1; i++)
        {
            if (_iterationsSinceLastNarrow >= IterationsBetweenNarrows)
            {
                _iterationsSinceLastNarrow = 0;
                var randomTileType = RandomlySelectDeepGroundTileType();
                PlaceDeepGroundTile(randomTileType);
            }
            else
            {
                _iterationsSinceLastNarrow++;
                var randomTileType = RandomlySelectNonNarrowDeepGroundTileType();
                PlaceDeepGroundTile(randomTileType);
            }

            RandomlyIncrementXGenerationPosition();
            IncrementYGenerationPosition();
        }

        PlaceGoalCavern();
    }

    private DeepGroundTileType RandomlySelectDeepGroundTileType()
    {
        int[] values = { 0, 0, 0, 0, 0, 2, 2, 2, 2, 3, 3, 3, 1, 1 };
        var randomEnumValue = Random.Range(0, values.Length);
        return (DeepGroundTileType)values.GetValue(randomEnumValue);
    }

    private DeepGroundTileType RandomlySelectNonNarrowDeepGroundTileType()
    {
        int[] values = { 0, 0, 0, 0, 0, 2, 2, 2, 2, 3, 3, 3 };
        var randomEnumValue = Random.Range(0, values.Length);
        return (DeepGroundTileType)values.GetValue(randomEnumValue);
    }

    private void PlaceDeepGroundTile(DeepGroundTileType deepGroundTileType)
    {
        Instantiate(DeepGroundPrefabDictionary[deepGroundTileType], new Vector2(_xGenerationPosition, _yGenerationPosition), Quaternion.identity, DeepGroundTileParent);
    }

    private void PlaceGoalCavern()
    {
        Instantiate(GoalCavernPrefab, new Vector2(_xGenerationPosition, _yGenerationPosition), Quaternion.identity, DeepGroundTileParent);
    }

    private void IncrementYGenerationPosition()
    {
        _yGenerationPosition -= DeepGroundTileHeight;
    }

    private void RandomlyIncrementXGenerationPosition()
    {
        var xDelta = ((float)Random.Range(0, 2) * 2f) - 1f;
        _xGenerationPosition += (XIncrementSize * xDelta);
        _xGenerationPosition = Mathf.Clamp(_xGenerationPosition, MinX, MaxX);
    }
}
