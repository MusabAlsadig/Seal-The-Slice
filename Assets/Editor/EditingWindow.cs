using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static SealTheSlice.Utilities;

namespace SealTheSlice
{
    public class EditingWindow : EditorWindow
    {
        private static CutterBase selectedCutter;


        private int selectedType = 1;
        private float radius;
        private int pointsCount;
        private float rotation;

        [MenuItem("Tool/CutterEditWindow")]
        private static void CreateWindow()
        {
            GetWindow<EditingWindow>().CheckSelection();
        }

        public static void OpenWith(CutterBase cutter)
        {
            GetWindow<EditingWindow>();
            selectedCutter = cutter;
        }

        private void Awake()
        {
            Selection.selectionChanged += CheckSelection;
        }

        private void OnDestroy()
        {
            Selection.selectionChanged -= CheckSelection;
        }

        private void OnGUI()
        {
            selectedCutter = (CutterBase)EditorGUILayout.ObjectField(selectedCutter, typeof(CutterBase), true);
            if (selectedCutter == null)
            {
                if (Button("Create new"))
                {
                    GameObject newCutter = new GameObject("new Cutter");
                    newCutter.AddComponent<CutterBase>();
                    Selection.activeObject = newCutter;
                }
                return;
            }

            radius = EditorGUILayout.FloatField("Radius", radius);
            pointsCount = EditorGUILayout.IntField("Points count", pointsCount);
            rotation = EditorGUILayout.Slider("Rotation", rotation, 0, 360);

            if (Button("Edit"))
            {
                Undo.RecordObject(selectedCutter, "Edited shape of cutter");
                selectedCutter.points = MakeAsCircle(radius, PolygonDirection.CounterClockwise, rotation);
                EditorUtility.SetDirty(selectedCutter);
            }
        }

        private void CheckSelection()
        {
            if (Selection.activeObject == null)
            {
                selectedCutter = null;
                return;
            }

            bool isCutter = Selection.activeGameObject.TryGetComponent(out CutterBase cutter);

            if (!isCutter)
                selectedCutter = null;
            else
                selectedCutter = cutter;

            Repaint();
        }

        private bool Button(string lable)
        {
            return GUILayout.Button(lable);
        }

        private List<Vector2> MakeAsCircle(float radius, PolygonDirection direction, float startRotation)
        {
            Vector2 center = GetCenter(selectedCutter.points);
            if (float.IsNaN(center.x))
                center.x = 0;
            if (float.IsNaN(center.y))
                center.y = 0;

            float angle = 360 / (float)pointsCount;


            List<Vector2> points = new List<Vector2>();
            Vector2 baseVector = center + Vector2.right * radius;
            for (int i = 1; i <= pointsCount; i++)
            {
                points.Add(Rotate(baseVector, center, angle * i + startRotation, direction));
            }

            return points;
        }

    }
}