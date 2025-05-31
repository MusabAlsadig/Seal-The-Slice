using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cutter))]
public class CutterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Cut"))
        {
            (target as Cutter).CutShapeInFront();
        }
    }

    private void OnSceneGUI()
    {
        Cutter cutter = target as Cutter;

        Handles.matrix = Matrix4x4.TRS(cutter.transform.position, cutter.transform.rotation, cutter.transform.lossyScale);
        Vector2 center = GetCenter(cutter.points);
        bool isCorrectDirection = IsShapeDirectionCounterClockwise(cutter.points, center);

        DrawPolygon(cutter, isCorrectDirection);
        HandlePoints(cutter);
        HandleCenter(cutter, center);
    }


    private Vector2 GetCenter(List<Vector2> vectors)
    {
        Vector2 center = Vector2.zero;

        vectors.ForEach(v => center += v);

        center /= vectors.Count;
        return center;
    }

    #region Gizmos

    private void HandlePoints(Cutter cutter)
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

    private void HandleCenter(Cutter cutter, Vector2 center)
    {
        Handles.color = Color.white;
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

    private void DrawPolygon(Cutter cutter, bool isCorrect)
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
        var angles = vectors.ConvertAll(v => Vector2.SignedAngle(Vector2.right, v - center));

        float smallesAngle = int.MinValue;


        int startIndex = angles.FindIndex(a => a >= 0);

        for (int i = 0; i < angles.Count; i++)
        {
            int currentIndex = startIndex + i < angles.Count ? startIndex + i: 0;
            float angle = angles[currentIndex];

            // make all angles go from 0-360
            if (angle < 0)
                angle += 360;

            if (smallesAngle > angle)
                return false;

            smallesAngle = angle;
        }

        return true;
    }

}
