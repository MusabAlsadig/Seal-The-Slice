using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public Vector2 position;

    public Point lastPoint;
    public Point nextPoint;

    public List<Point> polygon;
    
    public EarClipper.PointType pointType;

    public Point (Vector2 position)
    {
        this.position = position;
    }

    public float GetAngle()
    {
        Vector2 side1 = lastPoint.position - position;
        Vector2 side2 = nextPoint.position - position;

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
