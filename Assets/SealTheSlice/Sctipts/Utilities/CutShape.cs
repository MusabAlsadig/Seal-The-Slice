using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SealTheSlice
{
    [Serializable]
    public class CutShape
    {
        public List<Vector2> points = new List<Vector2>();


        public List<LimitedPlane> planes = new List<LimitedPlane>();

        private Bounds bounds;

        public CutShape() { }

        public CutShape(List<Vector2> points)
        {
            this.points = points;
            Vector2 min = Vector2.one * float.MaxValue;
            Vector3 max = Vector2.one * float.MinValue;
            for (int i = 0; i < points.Count; i++)
            {
                int nextIndex = i + 1 < points.Count ? i + 1 : 0;

                Vector2 currentPoint = points[i];
                Vector2 nextPoint = points[nextIndex];

                LimitedPlane plane = new LimitedPlane(currentPoint, nextPoint, PolygonDirection.CounterClockwise);
                planes.Add(plane);

                min = Vector2.Min(currentPoint, min);
                max = Vector2.Max(currentPoint, max);
            }

            bounds = new Bounds();
            bounds.SetMinMax(min, max);
        }

        public bool IsInside(Triangle triangle)
        {
            return IsPointInside(triangle.GetPointInTheMiddle());
        }

        /// <summary>
        /// Try get a plane that go through <paramref name="vertex"/>
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool TryGetPlanesThatCross(VertexData vertex, out List<LimitedPlane> result)
        {
            result = planes.FindAll(p => p.IsWithinRange(vertex.position) && p.IsInsideThisPlane(vertex.position));
            return result != null;
        }

        private bool IsPointInside(Vector2 point)
        {
            if (!bounds.Contains(point))
                return false;

            var planesToCheck = planes.Where(p => p.IsWithinRange(point));
            if (planesToCheck.Count() == 0)
                return false;
            return planesToCheck.All(plane => plane.GetSideWithinLimits(point));
        }







    }
}