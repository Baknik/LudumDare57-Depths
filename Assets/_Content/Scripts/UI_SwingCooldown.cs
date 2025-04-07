using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

[RequireComponent(typeof(ProceduralImage))]
public class UI_SwingCooldown : MonoBehaviour
{
    [Header("Scene References")]
    public Spelunker Spelunker;

    private ProceduralImage _proceduralImage;

    private void Awake()
    {
        _proceduralImage = GetComponent<ProceduralImage>();
    }

    private void Update()
    {
        var timeSinceLastSwing = Mathf.Clamp(Time.time - Spelunker.TimeOfLastSwing, 0f, Spelunker.SwingCooldown);
        _proceduralImage.fillAmount = timeSinceLastSwing / Spelunker.SwingCooldown;
    }
}
