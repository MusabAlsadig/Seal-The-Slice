using Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class CutShape
{
    public List<Vector2> points = new List<Vector2>();


    public List<LimitedPlane> planes = new List<LimitedPlane>();

    public CutShape(List<Vector2> points)
    {
        this.points = points;
        for (int i = 0; i < points.Count; i++)
        {
            int nextIndex = i + 1 < points.Count ? i + 1 : 0;

            Vector2 currentPoint = points[i];
            Vector2 nextPoint = points[nextIndex];

            LimitedPlane plane = new LimitedPlane(currentPoint, nextPoint);
            planes.Add(plane);
        }
    }

    public bool IsInside(Triangle triangle)
    {
        bool isInsideA = GetSide(triangle.vertexA.position);
        bool isInsideB = GetSide(triangle.vertexB.position);
        bool isInsideC = GetSide(triangle.vertexC.position);

        bool isInside = isInsideA && isInsideB && isInsideC;

        return isInside;
    }

    public bool GetSide(Vector2 point)
    {
        var planesToCheck = planes.Where(p => p.IsWithinRange(point));
        if (planesToCheck.Count() == 0)
            return false;
        return planesToCheck.All(plane => plane.GetSideWithinLimits(point));
    }


}
