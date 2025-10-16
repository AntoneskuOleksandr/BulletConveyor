using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Менеджер, который рисует цепочки молний через всех врагов.
/// </summary>
public class LightningManager : MonoBehaviour
{
    [Header("Lightning Settings")]
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private int segmentsPerSection = 10;
    [SerializeField] private float baseDisplacement = 0.05f;
    [SerializeField] private float distanceFactor = 0.02f;
    [SerializeField] private float lineDuration = 0.2f;
    [SerializeField] private Color color = Color.yellow;
    [SerializeField] private float brightness = 2f;

    /// <summary>
    /// Отрисовывает цепочку между всеми врагами в списке.
    /// </summary>
    public void DrawChain(List<Enemy> enemies)
    {
        if (enemies == null || enemies.Count < 2) return;

        List<Vector3> positions = new List<Vector3>();

        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.HitTransform != null)
            {
                positions.Add(enemy.HitTransform.position);
            }
        }

        if (positions.Count < 2) return;

        GameObject lightningGO = new GameObject("LightningVisual");
        lightningGO.transform.SetParent(transform, false);

        LightningVisual visual = lightningGO.AddComponent<LightningVisual>();
        visual.Configure(lineWidth, segmentsPerSection, baseDisplacement, distanceFactor, lineDuration, color, brightness);

        visual.DrawChain(positions);

        Destroy(lightningGO, lineDuration + 0.1f);
    }

}
