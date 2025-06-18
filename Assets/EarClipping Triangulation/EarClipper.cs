using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utilities;

public static class EarClipper
{

    private static List<Point> reflexVertices = new List<Point>();
    private static List<Point> convexVertices = new List<Point>();
    private static List<Point> earVertices = new List<Point>();
    private static Polygon polygon;
    

    private static Dictionary<PointType, List<Point>> groups;


    /// <summary>
    /// fill a polygon with children and grandchildren....etc
    /// </summary>
    /// <param name="rootShape"></param>
    /// <returns></returns>
    public static List<Triangle> FillPolygoneTree(PolyTree rootPolygon)
    {
        List<Triangle> fullTringles = new List<Triangle>();
        Queue<PolyTree> queue = new Queue<PolyTree>();
        
        queue.Enqueue(rootPolygon);
        while (queue.Count > 0)
        {
            PolyTree outerNode = queue.Dequeue();

            int numChildren = outerNode.ChildrenCount;
            if (numChildren == 0)
            {
                // this a simple polygon with no holes
                fullTringles.AddRange(FillSimplePolygon(outerNode.shape));
            }
            else
            {
                // this polygon have holes
                List<PolyTree> innerHoles = new List<PolyTree>();
                for (int i = 0; i < numChildren; i++)
                {
                    PolyTree hole = outerNode.GetChild(i);
                    int numGrandchildren = hole.ChildrenCount;
                    innerHoles.Add(hole);
                    for (int j = 0; j < numGrandchildren; j++)
                    {
                        // these are another shapes inside this hole,
                        // save them to be review as seperate shapes
                        queue.Enqueue(hole.GetChild(j));
                    }
                }

                fullTringles.AddRange(FillWithHoles(outerNode, innerHoles));
            }
        }

        return fullTringles;

    }
         
    
    public static List<Triangle> FillWithHoles(PolyTree outterPolygon,  List<PolyTree> holes)
    {
        Initialize(outterPolygon.shape);

        

        // order by X value from right to left
        holes = holes.OrderByDescending(shape => shape.shape.points.OrderByDescending(p => p.Position.x).Last().Position.x).ToList();

        
        foreach (var hole in holes)
        {
            InsertHole(hole.shape);
            RefreshPolygon();
        }

        return TriangulatePolygon();
    }

    public static List<Triangle> FillSimplePolygon(Polygon p)
    {
        Initialize(p);
        RefreshPolygon();
        return TriangulatePolygon();
    }

    #region Utilities
    private static void Initialize(Polygon p)
    {
        if (groups == null)
        {
            // initialize
            groups = new Dictionary<PointType, List<Point>>();
            groups[PointType.Reflex] = reflexVertices;
            groups[PointType.Convex] = convexVertices;
            groups[PointType.Ear] = earVertices;
        }

        polygon = p;

        RefreshPolygon();
    }

    private static List<Triangle> TriangulatePolygon()
    {
        List<Triangle> result = new List<Triangle>();

        int saftyCounter = 0;
        while (polygon.PointsCount >= 3)
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

            Triangle Triangle = new Triangle(ear);
            result.Add(Triangle);

            if (polygon.PointsCount == 3)
                return result;

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
        for (int i = 0; i < polygon.PointsCount; i++)
        {
            int lastIndex = i - 1;
            if (lastIndex < 0)
                lastIndex = polygon.PointsCount - 1;

            Point lastPoint = polygon[lastIndex];
            Point currentPoint = polygon[i];
            Point nextPoint = polygon[(i + 1) % polygon.PointsCount];

            currentPoint.lastPoint = lastPoint;
            currentPoint.nextPoint = nextPoint;


            if (IsReflex(currentPoint.GetAngle()))
            {
                ChangeGroup(currentPoint, PointType.Reflex);
            }
        }


        for (int i = 0; i < polygon.points.Count; i++)
        {
            Point currentPoint = polygon[i];

            PointType type = GetPointType(currentPoint);
            ChangeGroup(currentPoint, type);
        }

    }

    private static void InsertHole(Polygon hole)
    {

        Point rightMostInnerPoint = hole.points.OrderBy(p => p.Position.x).Last(); // also known as (M)

        float shortestDistance = float.PositiveInfinity;
        Vector3 closesIntersection = Vector3.zero; // also known as (I)
        Point closestVertex = null;
        foreach (var point in polygon.points)
        {
            float sign = Sign(point.Position, point.nextPoint.Position, rightMostInnerPoint.Position);

            // ignore edges facing outward
            if (IsReflex(sign))
                continue;

            if (polygon.direction == PolygonDirection.CounterClockwise)
            {
                // for counter-clockwise v[i] must be below, and v[i+1] must be above
                if (rightMostInnerPoint.Position.y < point.Position.y ||
                    rightMostInnerPoint.Position.y > point.nextPoint.Position.y)
                    continue;
            }
            if (polygon.direction == PolygonDirection.Clockwise)
            {
                // for clockwise v[i] must be above, and v[i+1] must be below
                if (rightMostInnerPoint.Position.y > point.Position.y ||
                    rightMostInnerPoint.Position.y < point.nextPoint.Position.y)
                    continue;
            }
            Line line = new Line(point.Position, point.nextPoint.Position);
            // shot a ray to the right and get intersection point in the line
            Vector3 intersectionPoint = line.GetPointAtY(rightMostInnerPoint.Position.y);

            float distance = intersectionPoint.x - rightMostInnerPoint.Position.x;
            if (distance < shortestDistance)
            {
                // found a closer point
                closesIntersection = intersectionPoint;
                closestVertex = point;
                shortestDistance = distance;

            }
        }


        // intersection is a vertex, finish here

        Point outterVertexToConnectTo;
        if (closesIntersection == closestVertex.Position)
            outterVertexToConnectTo = closestVertex;
        else if (closesIntersection == closestVertex.nextPoint.Position)
            outterVertexToConnectTo = closestVertex.nextPoint;
        else
        {
            // intersection isn't a vertex


            // select the vertex on the right from this edge
            Point rightMostOutterVertexWithinEdge; // also known as (P)
            if (closestVertex.Position.x > closestVertex.nextPoint.Position.x)
                rightMostOutterVertexWithinEdge = closestVertex;
            else
                rightMostOutterVertexWithinEdge = closestVertex.nextPoint;

            Point bestReflex = null;
            float smallestAngle = float.PositiveInfinity;
            Vector3 direction_MI = closesIntersection - rightMostInnerPoint.Position;
            foreach (var reflex in reflexVertices)
            {
                if (IsPointInTriangle(reflex.Position, rightMostInnerPoint.Position, closesIntersection, rightMostOutterVertexWithinEdge.Position))
                {
                    Vector3 direction_MP = rightMostOutterVertexWithinEdge.Position - rightMostInnerPoint.Position;
                    float angle = Vector3.Angle(direction_MI, direction_MP);

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

        int startIndexInPolygon = polygon.points.IndexOf(outterVertexToConnectTo) + 1;// insert after this vertex
        int indexOffsetForInner = hole.points.IndexOf(rightMostInnerPoint);

        List<Point> innerPoints = new List<Point>();
        for (int i = 0; i < hole.points.Count; i++)
        {
            int index = (indexOffsetForInner + i) % hole.points.Count;
            innerPoints.Add(new Point(hole.points[index].vertex));
        }

        innerPoints.Add(new Point(rightMostInnerPoint.vertex));
        innerPoints.Add(new Point(outterVertexToConnectTo.vertex));
        
        polygon.points.InsertRange(startIndexInPolygon, innerPoints);   
    }

    private static PointType GetPointType(Point point)
    {
        float angle = point.GetAngle();


        // unity count angles > 180 as negative angles
        if (IsReflex(angle))
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

    private static void ChangeGroup(Point point, PointType newType)
    {
        groups[point.pointType].Remove(point);
        groups[newType].Add(point);
        point.pointType = newType;
    }

    private static bool IsReflex(float angle)
    {
        if (polygon.direction == PolygonDirection.CounterClockwise)
            return angle < 0;
        else if (polygon.direction == PolygonDirection.Clockwise)
            return angle > 0;
        else
            return angle == 0;
    }

    private static bool IsEar(Point vertex)
    {
        foreach (Point p in reflexVertices)
        {
            // no need to check thoes three if they are inside themselves;
            if (p.Position == vertex.Position)
                continue;
            if (p.Position == vertex.lastPoint.Position)
                continue;
            if (p.Position == vertex.nextPoint.Position)
                continue;

            // an ear can't have a point inside it's tringle
            if (IsPointInTriangle(p.Position, vertex))
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

    private static float Sign(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    private static bool IsPointInTriangle(Vector3 pt, Point vertex)
        => IsPointInTriangle(pt, vertex.lastPoint.Position, vertex.Position, vertex.nextPoint.Position);

    private static bool IsPointInTriangle(Vector3 pt, Vector3 triangle_1, Vector3 triangle_2, Vector3 triangle_3)
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

#endregion

    public enum PointType
    {
        Reflex,
        Convex,
        Ear
    }
}

public class Line
{
    private readonly float slop;
    private readonly float intercept;

    /// <summary>
    /// this is used only when the slop is infinity, and any point will have the same X in this line
    /// </summary>
    private float potintial_X;
    private float z;

    public Line (Vector3 point1, Vector3 point2)
    {
        slop = (point1.y - point2.y) / (point1.x - point2.x);

        intercept = point1.y - slop * point1.x;

        potintial_X = point1.x;
        z = point1.z;
    }

    public Vector3 GetPointAtY(float y)
    {
        float x;
        if (float.IsInfinity(slop))
            x = potintial_X;
        else
            x = (y - intercept) / slop;

        return new Vector3 (x, y, z);
    }
}

public class Intersection
{
    public readonly Vector3 position;
    /// <summary>
    /// source vertex before the intersection
    /// </summary>
    public readonly Point vertex;

    public Intersection(Vector3 position, Point sourceVertex)
    {
        this.position = position;
        this.vertex = sourceVertex;
    }

}
