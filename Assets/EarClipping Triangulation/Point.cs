using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public Vector3 Position => vertex.position;

    public readonly VertexData vertex;

    public Point lastPoint;
    public Point nextPoint;

    
    public EarClipper.PointType pointType;

    public Point Copy()
    {
        Point copy = new Point(vertex.Copy());
        return copy;
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
