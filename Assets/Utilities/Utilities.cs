using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utilities
{
    public static PolygonDirection GetPolygonDirection(params Vector2[] vectors)
        => GetPolygonDirection(vectors.ToList());

    public static PolygonDirection GetPolygonDirection(List<Vector2> vectors)
    {
        float area = CalculateArea(vectors);

        if (area < 0)
            return PolygonDirection.Clockwise;
        else if (area > 0)
            return PolygonDirection.CounterClockwise;
        else
            return PolygonDirection.Coliner;
    }

    private static float CalculateArea(List<Vector2> vectors)
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


    public enum PolygonDirection
    {
        Clockwise = -1,
        Coliner = 0,
        CounterClockwise = 1,
    }
}


