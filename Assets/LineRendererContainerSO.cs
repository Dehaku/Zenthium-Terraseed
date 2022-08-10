using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Containers/LineRenderer")]
public class LineRendererContainerSO : ScriptableObject
{
    public LineRenderer lineRenderer;
    public Gradient gradiant;

    public LineRenderer GetLineRenderer()
    {
        LineRenderer newLine = new LineRenderer();
        newLine.colorGradient = gradiant;
        return newLine;
    }

    private void OnValidate()
    {
        if (!lineRenderer)
            lineRenderer = new LineRenderer();
    }
}
