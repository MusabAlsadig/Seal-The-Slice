using Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class CutShape
{
    public List<Vector2> points = new List<Vector2>();


    public List<Plane> planes = new List<Plane>();

    public CutShape(List<Vector2> points)
    {
        this.points = points;
        for (int i = 0; i < points.Count; i++)
        {
            int nextIndex = i + 1 < points.Count ? i + 1 : 0;

            Vector2 currentPoint = points[i];
            Vector2 nextPoint = points[nextIndex];

            Vector3 normal = PerpendicularCounterClockwise(nextPoint - currentPoint);
            normal.Normalize();
            Plane plane = new Plane(normal, currentPoint);
            planes.Add(plane);
        }
    }

    public bool IsInside(Triangle triangle)
    {
        bool isInsideA = GetSide(triangle.vertexA.position);
        bool isInsideB = GetSide(triangle.vertexB.position);
        bool isInsideC = GetSide(triangle.vertexC.position);

        bool isInside = isInsideA && isInsideB && isInsideC;

        if (!isInside)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("A");
            //planes.ForEach(plane => message.AppendLine(plane.GetDistanceToPoint(triangle.vertexA.position).ToString()));
            message.Append(triangle.vertexA.position);

            message.AppendLine("");

            message.AppendLine("B");
            //planes.ForEach(plane => message.AppendLine(plane.GetDistanceToPoint(triangle.vertexB.position).ToString()));
            message.Append(triangle.vertexB.position);

            message.AppendLine("");

            message.AppendLine("C");
            message.Append(triangle.vertexC.position);

            Debug.Log(message.ToString());
        }
        return isInside;
    }

    public bool GetSide(Vector2 point)
    {
        
        return planes.All(plane => Vector3.Dot(plane.normal, point) + plane.distance + 0.0001f > 0);
    }


    Vector2 PerpendicularCounterClockwise(Vector2 vector2)
    {
        return new Vector2(-vector2.y, vector2.x);
    }
}
