using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CutterBase), true)]
public class CutterEditor : Editor
{
    private bool isCorrectDirection;

    private CutterBase cutter;
    private void OnEnable()
    {
        cutter = target as CutterBase;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Test Cut forward"))
        {
            cutter.CutShapeInFront();
        }

        if (!isCorrectDirection)
        {
            if (GUILayout.Button("fix to counter-clockwise"))
            {
                cutter.ReversDirection();
                OnSceneGUI();
                SceneView.RepaintAll();
            }
        }
    }

    private void OnSceneGUI()
    {
        Handles.matrix = Matrix4x4.TRS(cutter.transform.position, cutter.transform.rotation, cutter.transform.lossyScale);
        Vector2 center = GetCenter(cutter.points);
        isCorrectDirection = IsShapeDirectionCounterClockwise(cutter.points, center);

        DrawPolygon(cutter, isCorrectDirection);
        HandleCenter(cutter, center);
        HandlePoints(cutter);
    }


    private Vector2 GetCenter(List<Vector2> vectors)
    {
        Vector2 center = Vector2.zero;

        vectors.ForEach(v => center += v);

        center /= vectors.Count;
        return center;
    }

    #region Gizmos

    private void HandlePoints(CutterBase cutter)
    {
        Handles.color = Color.white;
        for (int i = 0; i < cutter.points.Count; i++)
        {
            int nextIndex = i + 1 < cutter.points.Count ? i + 1 : 0;
            Vector3 nextPoint = cutter.points[nextIndex];

            Vector2 changedPoint = Handles.FreeMoveHandle(cutter.points[i], 0.08f, Vector3.zero, Handles.SphereHandleCap);

            if (changedPoint != cutter.points[i])
            {
                Undo.RecordObject(cutter, "");
                Undo.SetCurrentGroupName($"Edit the cut shape of {cutter.name}");
                cutter.points[i] = changedPoint;
            }
        }
    }

    private void HandleCenter(CutterBase cutter, Vector2 center)
    {
        Handles.color = Color.blue;
        Vector2 newCenter = Handles.FreeMoveHandle(center, 0.08f, Vector3.zero, Handles.SphereHandleCap);
        Vector2 offset = newCenter - center;

        if (offset != Vector2.zero)
        {
            Undo.RecordObject(cutter, "");
            Undo.SetCurrentGroupName($"Moved the cut shape of {cutter.name}");

            EditorUtility.SetDirty(cutter);
            for (int i = 0; i < cutter.points.Count; i++)
            {
                cutter.points[i] += offset;
            }
        }
    }

    private void DrawPolygon(CutterBase cutter, bool isCorrect)
    {
        if (isCorrect)
            Handles.color = new Color(0, 1, 0, 0.5f);
        else
            Handles.color = new Color(1, 0, 0, 0.5f);

        Handles.DrawAAConvexPolygon(cutter.points.ConvertAll<Vector3>((vec2) => (Vector3)vec2).ToArray());
    }

    #endregion

    private bool IsShapeDirectionCounterClockwise(List<Vector2> vectors, Vector2 center)
    {
        float area = CalculateArea(vectors);

        return area > 0;
    }


    private float CalculateArea(List<Vector2> vectors)
    {
        float area = 0;
        for (int i = 0; i < vectors.Count; i++)
        {
            int nextIndex = i + 1 < vectors.Count ? i + 1 : 0;
            Vector2 point = vectors[i];
            Vector2 nextPoint = vectors[nextIndex];
            area += point.x * nextPoint.y - nextPoint.x * point.y;
        }

        area /= 2;

        return area;
    }
}
