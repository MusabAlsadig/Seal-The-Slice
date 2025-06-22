using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Point
{
    public Vector3 Position => vertex.position;

    public VertexData vertex;

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
        Vector3 side1 = lastPoint.Position - Position;
        Vector3 side2 = nextPoint.Position - Position;

        float result = Vector3.SignedAngle(side2, side1, vertex.normal);
        return result;
    }


    
    public void PrepareToRemove()
    {
        // re-link neighbours as if this point didn't exist
        lastPoint.nextPoint = nextPoint;
        nextPoint.lastPoint = lastPoint;
    }

}
