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
        bool IsSameSideX = IsSameSide_X(triangle);
        bool IsSameSideY = IsSameSide_Y(triangle);

        return !IsSameSideX && !IsSameSideY;
    }                                              
    private bool IsSameSide_X(Triangle triangle)
    {
        Side sideA = GetSide(triangle.vertexA.position.x, min_X, max_Y);
        Side sideB = GetSide(triangle.vertexB.position.x, min_X, max_Y);
        Side sideC = GetSide(triangle.vertexC.position.x, min_X, max_Y);


        return sideA == sideB && sideB == sideC;
    }
    
    private bool IsSameSide_Y(Triangle triangle)
    {
        Side sideA = GetSide(triangle.vertexA.position.y, min_Y, max_Y);
        Side sideB = GetSide(triangle.vertexB.position.y, min_Y, max_Y);
        Side sideC = GetSide(triangle.vertexC.position.y, min_Y, max_Y);


        return sideA == sideB && sideB == sideC;
    }


    private Side GetSide(float value, float min, float max)
    {

        Side side;
        if (value < min)
            side = Side.Negative;
        else if (value > max)
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
