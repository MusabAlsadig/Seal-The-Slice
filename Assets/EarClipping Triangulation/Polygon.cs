using System.Collections;
using System.Collections.Generic;
using static Utilities;

[System.Serializable]
public class Polygon : IEnumerable<Point>
{
    public List<Point> points;

    public PolygonDirection direction;

    private float min_X;
    private float max_X;
    private float min_Y;
    private float max_Y;


    public Polygon Copy()
    {
        Polygon copy = new Polygon();

        foreach (Point p in points)
        {
            copy.points.Add(p.Copy());
        }

        copy.Refresh();

        return copy;
    }

    public Point this[int i] => points[i];

    public int PointsCount => points.Count;

    public Polygon()
    {
        points = new List<Point>();
    }

    public Polygon(List<VertexData> vertices)
    {
        points = vertices.ConvertAll(v => new Point(v));
        direction = GetPolygonDirection(points);
        RecalculateBounds();
    }

    public void Reverse()
    {
        points.Reverse();
        Refresh();
    }

    public void Refresh()
    {
        RecalculateBounds();
        direction = GetPolygonDirection(points);

        for (int i = 0; i < points.Count; i++)
        {
            int lastIndex = i - 1;
            if (lastIndex < 0)
                lastIndex = points.Count - 1;

            Point currentPoint = points[i];
            currentPoint.lastPoint = points[lastIndex];
            currentPoint.nextPoint = points[(i + 1) % points.Count];
        }
    }

    public void Remove(Point p)
    {
        points.Remove(p);
    }

    public bool IsAroundTheShape(Triangle triangle)
    {
        bool IsSameSideX = IsSameSide_X(triangle);
        bool IsSameSideY = IsSameSide_Y(triangle);

        return !IsSameSideX && !IsSameSideY;
    }

    public bool Intersect(Triangle triangle)
    {
        foreach (Point p in points)
        {

            for (int i = 0; i < 3; i++)
            {

                if (MeshSlicer.TryFindIntesectionPoint(triangle[i].position, triangle[(i + 1) % 3].position, p.Position, p.nextPoint.Position, out _))
                    return true;
            }

        }

        return false;
    }

    public void RecalculateBounds()
    {
        min_X = float.PositiveInfinity;
        min_Y = float.PositiveInfinity;
        max_X = float.NegativeInfinity;
        max_Y = float.NegativeInfinity;

        foreach (var point in points)
        {
            if (point.Position.x < min_X)
                min_X = point.Position.x;
            if (point.Position.y < min_Y)
                min_Y = point.Position.y;
            if (point.Position.x > max_X)
                max_X = point.Position.x;
            if (point.Position.y > max_Y)
                max_Y = point.Position.y;
        }
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

    public IEnumerator<Point> GetEnumerator()
    {
        for (int i = 0; i < points.Count; i++)
        {
            yield return points[i];
        }
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new System.NotImplementedException();
    }


    private enum Side
    {
        Positive,
        Negative,
        Inside,
    }
}
