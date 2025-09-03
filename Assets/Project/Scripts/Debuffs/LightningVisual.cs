using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightningVisual : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private int segmentsPerSection = 10; // сегментов на каждую линию
    [SerializeField] private float baseDisplacement = 0.05f; // базовое смещение
    [SerializeField] private float distanceFactor = 0.02f; // коэффициент нарастани€ смещени€ от рассто€ни€

    [Header("Animation Settings")]
    [SerializeField] private float lineDuration = 0.2f;

    [Header("Glow Settings")]
    [SerializeField] private Color color = Color.yellow;
    [SerializeField] private float brightness = 2f;

    private LineRenderer line;
    private Material lineMaterial;

    /// <summary>
    /// –исует цепочку молний через несколько точек
    /// </summary>
    public void DrawChain(List<Vector3> positions)
    {
        if (positions.Count < 2) return;

        if (line == null)
            CreateLineRenderer();

        StopAllCoroutines();
        StartCoroutine(DrawChainCoroutine(positions));
    }

    private void CreateLineRenderer()
    {
        line = gameObject.AddComponent<LineRenderer>();
        lineMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
        lineMaterial.color = color * brightness;
        line.material = lineMaterial;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.enabled = false;
    }

    private IEnumerator DrawChainCoroutine(List<Vector3> positions)
    {
        // считаем общее количество сегментов
        int totalSegments = segmentsPerSection * (positions.Count - 1);
        Vector3[] points = new Vector3[totalSegments + 1];

        // базовые точки и смещение дл€ каждого сегмента
        int index = 0;
        for (int i = 0; i < positions.Count - 1; i++)
        {
            Vector3 start = positions[i];
            Vector3 end = positions[i + 1];
            float sectionLength = Vector3.Distance(start, end);
            float displacement = baseDisplacement + sectionLength * distanceFactor;

            for (int s = 0; s < segmentsPerSection; s++)
            {
                float t = s / (float)segmentsPerSection;
                Vector3 point = Vector3.Lerp(start, end, t);

                // промежуточные точки смещаем случайно
                if (s != 0) // не трогаем первый сегмент точки
                {
                    point += new Vector3(
                        Random.Range(-displacement, displacement),
                        Random.Range(-displacement, displacement),
                        Random.Range(-displacement, displacement)
                    );
                }

                points[index++] = point;
            }
        }
        points[index] = positions[positions.Count - 1]; // последн€€ точка

        line.positionCount = points.Length;
        line.enabled = true;

        float elapsed = 0f;
        while (elapsed < lineDuration)
        {
            // плавное затухание
            Color currentColor = color * brightness;
            currentColor.a = Mathf.Lerp(1f, 0f, elapsed / lineDuration);
            line.material.color = currentColor;

            line.SetPositions(points);
            elapsed += Time.deltaTime;
            yield return null;
        }

        line.enabled = false;
    }
}