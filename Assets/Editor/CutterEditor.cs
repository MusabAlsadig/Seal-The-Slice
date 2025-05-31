using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
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


        for (int i = 0; i < cutter.points.Count; i++)
        {
            int nextIndex = i + 1 < cutter.points.Count? i + 1 : 0;
            Vector3 nextPoint = cutter.points[nextIndex];

            Vector2 changedPoint = Handles.FreeMoveHandle(cutter.points[i],0.08f, Vector3.zero ,Handles.SphereHandleCap);

            if (changedPoint != cutter.points[i])
            {
                Undo.RecordObject(cutter, "");
                Undo.SetCurrentGroupName($"Edit the cut shape of {cutter.name}");
                cutter.points[i] = changedPoint;
            }
        }

        Handles.color = new Color(0, 1, 0, 0.5f);
        Handles.DrawAAConvexPolygon(cutter.points.ConvertAll<Vector3>((vec2) => (Vector3)vec2).ToArray());
        
        Handles.color = Color.white;

        Vector2 center = GetCenter(cutter.points);
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


    private Vector2 GetCenter(List<Vector2> vectors)
    {
        Vector2 center = Vector2.zero;

        vectors.ForEach(v => center += v);

        center /= vectors.Count;
        return center;
    }
}
