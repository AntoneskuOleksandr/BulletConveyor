using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningVisual : MonoBehaviour
{
    private float lineWidth;
    private int segmentsPerSection;
    private float baseDisplacement;
    private float distanceFactor;
    private float lineDuration;
    private Color color;
    private float brightness;

    private LineRenderer line;
    private Material lineMaterial;

    public void Configure(
        float lineWidth,
        int segmentsPerSection,
        float baseDisplacement,
        float distanceFactor,
        float lineDuration,
        Color color,
        float brightness)
    {
        this.lineWidth = lineWidth;
        this.segmentsPerSection = segmentsPerSection;
        this.baseDisplacement = baseDisplacement;
        this.distanceFactor = distanceFactor;
        this.lineDuration = lineDuration;
        this.color = color;
        this.brightness = brightness;
    }

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
        int totalSegments = segmentsPerSection * (positions.Count - 1);
        Vector3[] points = new Vector3[totalSegments + 1];

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

                if (s != 0)
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

        points[index] = positions[positions.Count - 1];

        line.positionCount = points.Length;
        line.enabled = true;

        float elapsed = 0f;
        while (elapsed < lineDuration)
        {
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
