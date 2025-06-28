using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static SealTheSlice.Utilities;

namespace SealTheSlice
{
    public class EditingWindow : EditorWindow
    {
        private static CutterBase selectedCutter;


        private int selectedType;
        private float radius;
        private int pointsCount;

        [MenuItem("Tool/CutterEditWindow")]
        private static void CreateWindow()
        {
            GetWindow<EditingWindow>();
            if (Selection.activeGameObject.TryGetComponent(out CutterBase cutter))
                selectedCutter = cutter;
        }

        public static void OpenWith(CutterBase cutter)
        {
            GetWindow<EditingWindow>();
            selectedCutter = cutter;
        }

        private void OnGUI()
        {
            selectedCutter = (CutterBase)EditorGUILayout.ObjectField(selectedCutter, typeof(CutterBase), true);
            if (selectedCutter == null)
                return;
            string[] s = new string[] { "squire", "circle" };
            selectedType = GUILayout.SelectionGrid(selectedType, s, 2);

            radius = EditorGUILayout.FloatField("Radius", radius);
            pointsCount = EditorGUILayout.IntField("Points count", pointsCount);

            if (Button("Edit"))
            {
                Undo.RecordObject(selectedCutter, "Edited shape of cutter");
                selectedCutter.points = MakeAsCircle(radius, PolygonDirection.CounterClockwise);
            }
        }

        private bool Button(string lable)
        {
            return GUILayout.Button(lable);
        }

        private List<Vector2> MakeAsCircle(float radius, PolygonDirection direction)
        {
            Vector2 center = GetCenter(selectedCutter.points);

            float angle = 360 / (float)pointsCount;


            List<Vector2> points = new List<Vector2>();
            Vector2 baseVector = center + Vector2.right * radius;
            for (int i = 1; i <= pointsCount; i++)
            {
                points.Add(Rotate(baseVector, center, angle * i, direction));
            }

            return points;
        }

    }
}