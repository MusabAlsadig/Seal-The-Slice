using System.Collections.Generic;
using UnityEngine;

namespace SealTheSlice
{
    public static class Utilities
    {
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

        public static PolygonDirection GetPolygonDirection(List<Point> Points)
        {
            float area = CalculateArea(Points);

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

        private static float CalculateArea(List<Point> Points)
        {
            float area = 0;
            for (int i = 0; i < Points.Count; i++)
            {
                int nextIndex = i + 1 < Points.Count ? i + 1 : 0;
                Vector3 point = Points[i].Position;
                Vector3 nextPoint = Points[nextIndex].Position;
                area += point.x * nextPoint.y - nextPoint.x * point.y;
            }

            area /= 2;

            return area;
        }


        public static Vector2 GetCenter(List<Vector2> vectors)
        {
            Vector2 center = Vector2.zero;

            vectors.ForEach(v => center += v);

            center /= vectors.Count;
            return center;
        }

        public static Vector2 Rotate(Vector2 vectorToRotate, Vector2 centerPoint, float angleInDegrees, PolygonDirection direction)
        {
            if (direction == PolygonDirection.Clockwise)
                angleInDegrees *= -1;

            float angleInRadians = angleInDegrees * (Mathf.PI / 180);
            float cosTheta = Mathf.Cos(angleInRadians);
            float sinTheta = Mathf.Sin(angleInRadians);
            return new Vector2
            {
                x =
                    cosTheta * (vectorToRotate.x - centerPoint.x) -
                    sinTheta * (vectorToRotate.y - centerPoint.y) + centerPoint.x,
                y =
                    sinTheta * (vectorToRotate.x - centerPoint.x) +
                    cosTheta * (vectorToRotate.y - centerPoint.y) + centerPoint.y
            };
        }

        public static Polygon CreatePolygon(List<Triangle> triangles, out List<Edge> edgesBetweenFaces)
        {
            // get all none repeated edges
            List<Edge> edges = new List<Edge>();
            List<Edge> multiNormalEdges = new List<Edge>();
            foreach (var triangle in triangles)
            {

                for (int i = 0; i < 3; i++)
                {
                    Edge edge = new Edge(triangle[i], triangle[(i + 1) % 3]);

                    if (edges.Contains(edge))
                    {
                        Edge oldEdge = edges[edges.IndexOf(edge)];
                        edges.Remove(edge);

                        if (oldEdge.Normal != edge.Normal)
                        {
                            multiNormalEdges.Add(edge);
                            multiNormalEdges.Add(oldEdge);
                        }
                    }
                    else
                        edges.Add(edge);
                }
            }
            edgesBetweenFaces = multiNormalEdges;

            Edge currentEdge = edges.Find(e => !multiNormalEdges.Contains(e));

            VertexData startVertex = currentEdge.a;
            VertexData currentVertex = startVertex;
            List<VertexData> vertices = new List<VertexData>();
            for (int saftyCounter = 0; saftyCounter <= 1000; saftyCounter++)
            {
                vertices.Add(currentVertex);
                currentEdge = edges.Find(e => e.Contain(currentVertex));
                edges.Remove(currentEdge);

                if (currentEdge.Normal != currentVertex.normal)
                {
                    // add 2nd vertex on the same position with different normal
                    vertices.Add(currentEdge.a);
                }

                currentVertex = currentEdge.b;

                if (currentVertex.position == startVertex.position)
                    break;

                if (saftyCounter == 1000)
                {
                    Debug.Log("too much trys");
                }
            }

            return new Polygon(vertices);
        }
    }
}