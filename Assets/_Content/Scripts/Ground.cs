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
    public Transform NarrowWidthDeepGround;
    public Transform NormalWidthDeepGround;
    public Transform WideWidthDeepGround;
    public Transform ExtraWideWidthDeepGround;

    [Header("Settings")]
    public float DeepGroundTileHeight;
    public float StartYGenerationPosition;
    public int NumDeepGroundLayers;

    private float _xGenerationPosition;
    private float _yGenerationPosition;
    private Dictionary<DeepGroundTileType, Transform> DeepGroundPrefabDictionary;

    private void Start()
    {
        _xGenerationPosition = 0f;
        _yGenerationPosition = StartYGenerationPosition;

        DeepGroundPrefabDictionary = new Dictionary<DeepGroundTileType, Transform>();
        DeepGroundPrefabDictionary[DeepGroundTileType.Narrow] = NarrowWidthDeepGround;
        DeepGroundPrefabDictionary[DeepGroundTileType.Normal] = NormalWidthDeepGround;
        DeepGroundPrefabDictionary[DeepGroundTileType.Wide] = WideWidthDeepGround;
        DeepGroundPrefabDictionary[DeepGroundTileType.ExtraWide] = ExtraWideWidthDeepGround;

        GenerateDeepGround();
    }

    private void GenerateDeepGround()
    {
        PlaceDeepGroundTile(DeepGroundTileType.Normal);
        IncrementYGenerationDepth();

        for (int i=0; i<NumDeepGroundLayers - 1; i++)
        {
            var randomTileType = RandomlySelectDeepGroundTileType();
            PlaceDeepGroundTile(randomTileType);
            IncrementYGenerationDepth();
        }
    }

    private DeepGroundTileType RandomlySelectDeepGroundTileType()
    {
        var values = System.Enum.GetValues(typeof(DeepGroundTileType));
        var randomEnumValue = UnityEngine.Random.Range(0, values.Length);
        return (DeepGroundTileType)values.GetValue(randomEnumValue);
    }

    private void PlaceDeepGroundTile(DeepGroundTileType deepGroundTileType)
    {
        Instantiate(DeepGroundPrefabDictionary[deepGroundTileType], new Vector2(_xGenerationPosition, _yGenerationPosition), Quaternion.identity, DeepGroundTileParent);
    }

    private void IncrementYGenerationDepth()
    {
        _yGenerationPosition -= DeepGroundTileHeight;
    }
}
