using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(ShapeViewer))]
public class ShapeViewerEditor : Editor
{
    ShapeViewer shapeViewer;

    private void OnEnable()
    {
        shapeViewer = (ShapeViewer)target;
    }

    private void OnSceneGUI()
    {
        Handles.matrix = Matrix4x4.TRS(shapeViewer.transform.position, shapeViewer.transform.rotation, shapeViewer.transform.lossyScale);

        var tempTriangles = EarClipper.FillWithHoles(shapeViewer.outterShape, shapeViewer.children);

        for (int i = 0; i < tempTriangles.Count; i++)
        {
            if (i % 3 == 0)
                Handles.color = Color.red;
            else if (i % 3 == 1)
                Handles.color = Color.green;
            else
                Handles.color = Color.blue;
            Handles.DrawAAConvexPolygon(tempTriangles[i].points);
        }

        
        

        HandlePoints(shapeViewer.outterShape.points, Color.white);

        foreach (var shape in shapeViewer.children)
        {
            HandlePoints(shape.points, Color.yellow);
        }
    }

    private void HandlePoints(List<Vector2> shape, Color color)
    {
        Handles.color = color;
        for (int i = 0; i < shape.Count; i++)
        {
            int nextIndex = i + 1 < shape.Count ? i + 1 : 0;
            Vector3 nextPoint = shape[nextIndex];

            Vector2 changedPoint = Handles.FreeMoveHandle(shape[i], 0.08f, Vector3.zero, Handles.SphereHandleCap);

            if (changedPoint != shape[i])
            {
                Undo.RecordObject(shapeViewer, "");
                Undo.SetCurrentGroupName($"Edit the cut shape of {shapeViewer}");
                shape[i] = changedPoint;
            }
        }
    }

}
