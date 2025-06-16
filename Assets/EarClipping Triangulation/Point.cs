using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public Vector2 Position => vertex.position;

    public readonly VertexData vertex;

    public Point lastPoint;
    public Point nextPoint;

    public List<Point> polygon;
    
    public EarClipper.PointType pointType;

    public Point(Vector2 position)
    {
        vertex = new VertexData(0, position, Vector3.up, Vector2.up);
    }

    public Point (VertexData vertex)
    {
        this.vertex  = vertex;
    }

    public float GetAngle()
    {
        Vector2 side1 = lastPoint.Position - Position;
        Vector2 side2 = nextPoint.Position - Position;

        float result = Vector2.SignedAngle(side2, side1);
        return result;
    }


    
    public void PrepareToRemove()
    {
        // re-link neighbours as if this point didn't exist
        lastPoint.nextPoint = nextPoint;
        nextPoint.lastPoint = lastPoint;
    }

}
