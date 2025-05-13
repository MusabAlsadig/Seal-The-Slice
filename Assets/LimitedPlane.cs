using UnityEngine;
using System;

public class LimitedPlane
{
    private Plane plane;

    private Vector2 pointA;
    private Vector2 pointB;

    public Plane Plane => plane;

    public LimitedPlane(Vector2 a, Vector2 b)
    {
        pointA = a;
        pointB = b;

        Vector3 normal = PerpendicularCounterClockwise(pointB - pointA);
        normal.Normalize();
        plane = new Plane(normal, pointA);
    }

    public bool IsWithinRange(Vector2 point)
    {
        Vector2 min = Vector2.Min(pointA, pointB);
        Vector2 max = Vector2.Max(pointA, pointB);

        bool withinX = point.x - min.x > -0.0001f && point.x - max.x < 0.0001f;
        bool withinY = point.y - min.y > -0.0001f && point.y - max.y < 0.0001f;
        return withinX && withinY;
    }

    public bool GetSideWithinLimits(Vector2 point)
    {
        if (!IsWithinRange(point))
            throw new InvalidOperationException("don't check a plane for a point out side it's limit ");


        return Vector3.Dot(plane.normal, point) + plane.distance + 0.0001f > 0;
    }

    public bool GetSide(Vector3 point)
    {
        return plane.GetSide(point);
    }

    private Vector2 PerpendicularCounterClockwise(Vector2 vector2)
    {
        return new Vector2(-vector2.y, vector2.x);
    }
}
