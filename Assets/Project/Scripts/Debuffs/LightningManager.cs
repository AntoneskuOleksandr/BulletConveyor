using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Менеджер, который рисует цепочки молний через всех врагов
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
    /// Отрисовывает цепочку между всеми врагами в списке
    /// </summary>
    public void DrawChain(List<Enemy> enemies)
    {
        if (enemies == null || enemies.Count < 2) return;

        List<Vector3> positions = new List<Vector3>();
        foreach (var e in enemies)
            positions.Add(e.transform.position);

        // Создаём GameObject под молнию
        GameObject lightningGO = new GameObject("LightningVisual");
        lightningGO.transform.SetParent(transform, false);

        // Добавляем компонент и настраиваем его
        LightningVisual visual = lightningGO.AddComponent<LightningVisual>();
        visual.Configure(lineWidth, segmentsPerSection, baseDisplacement, distanceFactor, lineDuration, color, brightness);

        // Запускаем отрисовку
        visual.DrawChain(positions);

        // Уничтожаем объект после анимации
        Destroy(lightningGO, lineDuration + 0.1f);
    }
}
