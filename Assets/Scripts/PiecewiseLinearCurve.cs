using System.Collections.Generic;
using UnityEngine;

public class PiecewiseLinearCurve : Curve
{
    private List<ControlVertex> controlVertices = new List<ControlVertex>();
    
    void AddVertex(Vector2 position)
    {
        GameObject vertexObject = Instantiate(controlVertexPrefab, position, Quaternion.identity, transform);
        controlVertices.Add(vertexObject.GetComponent<ControlVertex>());

        lineRenderer.positionCount = controlVertices.Count;
        DrawCurveAtIndex(controlVertices.Count - 1);
    }

    protected void DrawCurveAtIndex(int index)
    {
        lineRenderer.SetPosition(index, controlVertices[index].transform.position - transform.position);
    }
    
    public override void AddNodeAtPosition(Vector2 position)
    {
        AddVertex(position);
    }

    public override void UpdateNodeAtIndex(int index, Vector2 position)
    {
        controlVertices[index].transform.position = position;
        DrawCurveAtIndex(index);
    }

    public override void UpdateLastNode(Vector2 position)
    {
        UpdateNodeAtIndex(controlVertices.Count - 1, position);
    }
}