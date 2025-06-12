using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EarClipper
{

    private static List<Point> reflexVertices = new List<Point>();
    private static List<Point> convexVertices = new List<Point>();
    private static List<Point> earVertices = new List<Point>();
    private static List<Point> polygon;

    private static Dictionary<PointType, List<Point>> groups;


    public static List<TempTriangle> FillWithHoles(List<Vector2> outterShape,  List<CutShape> holes)
    {
        Initialize(outterShape);


        // order by X value from right to left
        holes = holes.OrderByDescending(shape => shape.points.OrderByDescending(p => p.x).Last().x).ToList();

        foreach (var hole in holes)
        {
            InsertHole(hole.points);
            RefreshPolygon();
        }

        return TriangulatePolygon();
    }

    public static List<TempTriangle> FillPoligon(List<Vector2> shape, List<Vector2> innerHole)
    {
        
        Initialize(shape);

        InsertHole(innerHole);

        RefreshPolygon();

        return TriangulatePolygon();
    }

    private static void Initialize(List<Vector2> outterShape)
    {
        if (groups == null)
        {
            // initialize
            groups = new Dictionary<PointType, List<Point>>();
            groups[PointType.Reflex] = reflexVertices;
            groups[PointType.Convex] = convexVertices;
            groups[PointType.Ear] = earVertices;
        }

        polygon = outterShape.ConvertAll<Point>(v => new Point(v));
        RefreshPolygon();
    }

    private static List<TempTriangle> TriangulatePolygon()
    {
        List<TempTriangle> result = new List<TempTriangle>();

        int saftyCounter = 0;
        while (polygon.Count >= 3)
        {
            Point ear = null;
            try
            {

                ear = earVertices[0];
            }
            catch
            {
                ear = null;
            }

            TempTriangle tempTriangle = new TempTriangle(ear);
            result.Add(tempTriangle);

            ear.PrepareToRemove();
            earVertices.Remove(ear);
            polygon.Remove(ear);

            UpdatePoint(ear.lastPoint);
            UpdatePoint(ear.nextPoint);



            saftyCounter++;

            if (saftyCounter > 10000)
            {
                Debug.LogError("this loop been running for 10k times, safty break activated \nis this intentional?");
                break;
            }
        }


        return result;
    }

    private static void RefreshPolygon()
    {
        reflexVertices.Clear();
        convexVertices.Clear();
        earVertices.Clear();
        for (int i = 0; i < polygon.Count; i++)
        {
            int lastIndex = i - 1;
            if (lastIndex < 0)
                lastIndex = polygon.Count - 1;

            Point lastPoint = polygon[lastIndex];
            Point currentPoint = polygon[i];
            Point nextPoint = polygon[(i + 1) % polygon.Count];

            currentPoint.lastPoint = lastPoint;
            currentPoint.nextPoint = nextPoint;

            currentPoint.polygon = polygon;
        }


        for (int i = 0; i < polygon.Count; i++)
        {
            Point currentPoint = polygon[i];

            PointType type = GetPointType(currentPoint);
            currentPoint.pointType = type;
            groups[type].Add(currentPoint);
        }

    }

    private static void InsertHole(List<Vector2> inner)
    {

        Vector2 rightMostInnerPoint = inner.OrderBy(v => v.x).Last(); // also known as (M)

        float shortestDistance = float.PositiveInfinity;
        Vector2 closesIntersection = Vector2.zero; // also known as (I)
        Point closestVertex = null;
        foreach (var vertex in polygon)
        {
            float sign = Sign(vertex.position, vertex.nextPoint.position, rightMostInnerPoint);

            // skip outter edges
            if (sign < 0)
                continue;

            if (rightMostInnerPoint.y > vertex.position.y &&
                rightMostInnerPoint.y < vertex.nextPoint.position.y)
            {
                Line line = new Line(vertex.position, vertex.nextPoint.position);
                // shot a ray to the right and get intersection point in the line
                Vector2 intersectionPoint = line.GetPointAtY(rightMostInnerPoint.y);

                float distance = intersectionPoint.x - rightMostInnerPoint.x;
                if (distance < shortestDistance)
                {
                    // found a closer point
                    closesIntersection = intersectionPoint;
                    closestVertex = vertex;
                    shortestDistance = distance;

                }
            }
        }


        // intersection is a vertex, finish here

        Point outterVertexToConnectTo;
        if (closesIntersection == closestVertex.position)
            outterVertexToConnectTo = closestVertex;
        else if (closesIntersection == closestVertex.nextPoint.position)
            outterVertexToConnectTo = closestVertex.nextPoint;
        else
        {
            // intersection isn't a vertex


            // select the vertex on the right from this edge
            Point rightMostOutterVertexWithinEdge; // also known as (P)
            if (closestVertex.position.x > closestVertex.nextPoint.position.x)
                rightMostOutterVertexWithinEdge = closestVertex;
            else
                rightMostOutterVertexWithinEdge = closestVertex.nextPoint;

            Point bestReflex = null;
            float smallestAngle = float.PositiveInfinity;
            Vector2 direction_MI = closesIntersection - rightMostInnerPoint;
            foreach (var reflex in reflexVertices)
            {
                if (IsPointInTriangle(reflex.position, rightMostInnerPoint, closesIntersection, rightMostOutterVertexWithinEdge.position))
                {
                    Vector2 direction_MP = rightMostOutterVertexWithinEdge.position - rightMostInnerPoint;
                    float angle = Vector2.Angle(direction_MI, direction_MP);

                    if (angle < smallestAngle)
                    {
                        bestReflex = reflex;
                        smallestAngle = angle;
                    }

                }
            }

            if (bestReflex == null)
            {
                // no reflex inside tringle (M,I,P)
                outterVertexToConnectTo = rightMostOutterVertexWithinEdge;
            }
            else
            {
                outterVertexToConnectTo = bestReflex;
            }
        }

        int startIndexInPolygon = polygon.IndexOf(outterVertexToConnectTo) + 1;// insert after this vertex
        int indexOffsetForInner = inner.IndexOf(rightMostInnerPoint);

        List<Point> innerPoints = new List<Point>();
        for (int i = 0; i < inner.Count; i++)
        {
            int index = (indexOffsetForInner + i) % inner.Count;
            innerPoints.Add(new Point(inner[index]));
        }

        innerPoints.Add(new Point(rightMostInnerPoint));
        innerPoints.Add(new Point(outterVertexToConnectTo.position));
        
        polygon.InsertRange(startIndexInPolygon, innerPoints);

        
        
    }


    private static PointType GetPointType(Point point)
    {
        float angle = point.GetAngle();

        // unity count angles > 180 as negative angles
        if (angle < 0)
        {
            return PointType.Reflex;
        }
        else
        {
            if (IsEar(point))
                return PointType.Ear;
            else
                return PointType.Convex;
        }

    }

    private static bool IsEar(Point vertex)
    {
        foreach (Point p in polygon)
        {
            // no need to check thoes three if they are inside themselves;
            if (p.position == vertex.position)
                continue;
            if (p.position == vertex.lastPoint.position)
                continue;
            if (p.position == vertex.nextPoint.position)
                continue;

            // an ear can't have a point inside it's tringle
            if (IsPointInTriangle(p.position, vertex))
                return false;
        }

        return true;
    }


    private static void UpdatePoint(Point vertex)
    {
        PointType newType = GetPointType(vertex);
        if (newType == vertex.pointType)
            return;

        groups[vertex.pointType].Remove(vertex);
        groups[newType].Add(vertex);
        vertex.pointType = newType;
    }



    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    private static bool IsPointInTriangle(Vector2 pt, Vector2 triangle_1, Vector2 triangle_2, Vector2 triangle_3)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = Sign(pt, triangle_1, triangle_2);
        d2 = Sign(pt, triangle_2, triangle_3);
        d3 = Sign(pt, triangle_3, triangle_1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }


    private static bool IsPointInTriangle(Vector2 pt, Point vertex)
        => IsPointInTriangle(pt, vertex.lastPoint.position, vertex.position, vertex.nextPoint.position);


    public enum PointType
    {
        None, // just to make sure i assigned all of them
        Reflex,
        Convex,
        Ear
    }
}

public class Line
{
    private readonly float slop;
    private readonly float intercept;

    public Line (Vector2 point1, Vector2 point2)
    {
        slop = (point1.y - point2.y) / (point1.x - point2.x);

        intercept = point1.y - slop * point1.x;
    }

    public Vector2 GetPointAtY(float y)
    {
        float x = (y - intercept) / slop;

        return new Vector2 (x, y);
    }
}

public class Intersection
{
    public readonly Vector2 position;
    /// <summary>
    /// source vertex before the intersection
    /// </summary>
    public readonly Point vertex;

    public Intersection(Vector2 position, Point sourceVertex)
    {
        this.position = position;
        this.vertex = sourceVertex;
    }

}
