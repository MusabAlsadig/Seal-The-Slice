using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CutShape
{
    public List<Vector2> points = new List<Vector2>();


    public List<LimitedPlane> planes = new List<LimitedPlane>();

    private float min_X;
    private float max_X;
    private float min_Y;
    private float max_Y;

    public CutShape() { }

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
        var planesToCheck = planes.Where(p => p.IsWithinRange(point));
        if (planesToCheck.Count() == 0)
            return false;
        return planesToCheck.All(plane => plane.GetSideWithinLimits(point));
    }


    public void RecalculateBounds()
    {
        min_X = float.PositiveInfinity;
        min_Y = float.PositiveInfinity;
        max_X = float.NegativeInfinity;
        max_Y = float.NegativeInfinity;

        foreach (var point in points)
        {
            if (point.x < min_X)
                min_X = point.x;
            if (point.y < min_Y)
                min_Y = point.y;
            if (point.x > max_X)
                max_X = point.x;
            if (point.y > max_Y)
                max_Y = point.y;
        }
    }


    public bool IsAroundTheShape(Triangle triangle)
    {
        bool IsSameSideX = IsSameSide(triangle, min_X, max_X);
        bool IsSameSideY = IsSameSide(triangle, min_Y, max_Y);

        return !IsSameSideX && !IsSameSideY;
    }                                              
    private bool IsSameSide(Triangle triangle, float min, float max)
    {
        Side sideA = GetSide(triangle.vertexA, min, max);
        Side sideB = GetSide(triangle.vertexB, min, max);
        Side sideC = GetSide(triangle.vertexC, min, max);


        return sideA == sideB && sideB == sideC;
    }


    private Side GetSide(VertexData vertex, float min, float max)
    {

        Side side;
        if (vertex.position.x < min)
            side = Side.Negative;
        else if (vertex.position.x > max)
            side = Side.Positive;
        else
            side = Side.Inside;

        return side;
    }

    private enum Side
    {
        Positive,
        Negative,
        Inside,
    }

}
