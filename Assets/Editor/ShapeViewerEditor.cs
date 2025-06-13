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

        var tempTriangles = EarClipper.FillPolygoneTree(shapeViewer);

        for (int i = 0; i < tempTriangles.Count; i++)
        {
            Handles.color = new Color(.7f, .7f, .7f, .5f);
            Handles.DrawAAConvexPolygon(tempTriangles[i].points);
        }

        
        

        HandlePoints(shapeViewer.shape.points, Color.white);

        foreach (var shape in shapeViewer.children)
        {
            HandlePoints(shape.shape.points, Color.yellow);
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
