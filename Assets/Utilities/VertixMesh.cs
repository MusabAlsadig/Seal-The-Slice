using Extentions;
using System.Collections.Generic;
using UnityEngine;

public class VertexMesh
{
    private List<VertexData> vertices = new List<VertexData>();
    private List<Triangle> triangles = new List<Triangle>();

    public List<VertexData> Vertices => vertices;
    public List<Triangle> Triangles => triangles;
    public VertexMesh() 
    {
        // nothing
    }
    public VertexMesh(Mesh mesh)
    {
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            VertexData vertexData = new VertexData(i, mesh.vertices[i], mesh.normals[i], mesh.uv[i]);
            vertices.Add(vertexData);
        }

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            VertexData vertexA = vertices[mesh.triangles[i]];
            VertexData vertexB = vertices[mesh.triangles[i + 1]];
            VertexData vertexC = vertices[mesh.triangles[i + 2]];
            Triangle triangle = new Triangle(vertexA, vertexB, vertexC);

            triangles.Add(triangle);
        }
    }

    public VertexMesh(List<Triangle> triangles)
    {
        foreach (var triangle in triangles)
        {
            AddTriangle(triangle);
        }
    }

    public void AddTriangle(VertexData a, VertexData b, VertexData c)
    {
        AddTriangle(new Triangle(a, b, c));
    }

    public void AddTriangle(Triangle triangle)
    {

        if (TryFind(triangle.vertexA, out VertexData vertexA))
            triangle.vertexA = vertexA;
        else
            vertices.Add(triangle.vertexA);

        if (TryFind(triangle.vertexB, out VertexData vertexB))
            triangle.vertexB = vertexB;
        else
            vertices.Add(triangle.vertexB);

        if (TryFind(triangle.vertexC, out VertexData vertexC))
            triangle.vertexC = vertexC;
        else
            vertices.Add(triangle.vertexC);

        triangles.Add(triangle);
    }

    public void RemoveTriangle(Triangle triangle)
    {
        triangles.Remove(triangle);
    }
    
    

    public VertexData CreateIntersectionVertex(VertexData vertexA, VertexData vertexB, Plane plane)
    {
        // TODO : make a check when inserting a new vertix
        // to prevent both tringles of the same face of doing creating new vertix
        
        Ray ray = new Ray(vertexA.position, vertexB.position - vertexA.position);
        plane.Raycast(ray, out float distance);
        Vector3 position = ray.GetPoint(distance);

        float distanceA = Vector3.Distance(vertexA.position, position);
        float distanceB = Vector3.Distance(vertexB.position, position);
        float t = distanceA / (distanceA + distanceB);

        Vector2 uv = Vector2.Lerp(vertexA.uv, vertexB.uv, t);
        Vector3 normal = Vector3.Lerp(vertexA.normal, vertexB.normal, t);
        VertexData vertexData = new VertexData(vertices.Count, position, normal, uv);

        if (TryFind(vertexData, out VertexData oldVertex))
            return oldVertex;
        else
        {
            vertices.Add(vertexData);
            return vertexData;
        }
    }

    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh();
        int length = vertices.Count;
        var positions = new Vector3[length];
        var normals = new Vector3[length];
        var uv = new Vector2[length];
        var trianglesList = new List<int>();

        for (int i = 0; i < length; i++)
        {
            positions[i] = vertices[i].position;
            normals[i] = vertices[i].normal;
            uv[i] = vertices[i].uv;

            // refreash indexes
            vertices[i].index = i;
        }
        for (int i = 0; i < triangles.Count; i++)
        {
            trianglesList.Add(triangles[i].vertexA.index);
            trianglesList.Add(triangles[i].vertexB.index);
            trianglesList.Add(triangles[i].vertexC.index);
        }

        mesh.vertices = positions;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = trianglesList.ToArray();

        return mesh;
    }


    
    private Vector3 GetHalfwayPoint(List<VertexData> pointsAlongPlane)
    {
        if (pointsAlongPlane.Count > 0)
        {
            Vector3 firstPoint = pointsAlongPlane[0].position;
            Vector3 furthestPoint = Vector3.zero;
            float distance = 0f;

            for (int index = 0; index < pointsAlongPlane.Count; index++)
            {
                Vector3 point = pointsAlongPlane[index].position;
                float currentDistance = 0f;
                currentDistance = Vector3.Distance(firstPoint, point);

                if (currentDistance > distance)
                {
                    distance = currentDistance;
                    furthestPoint = point;
                }
            }

            return Vector3.Lerp(firstPoint, furthestPoint, 0.5f);
        }
        else
        {
            return Vector3.zero;
        }
    }
    private Vector3 ComputeNormal(VertexData a, VertexData b, VertexData c)
    {
        Vector3 sideL = b.position - a.position;
        Vector3 sideR = c.position - a.position;

        Vector3 normal = Vector3.Cross(sideL, sideR);

        return normal.normalized;
    }

    private bool TryFind(VertexData vertex, out VertexData result)
    {
        result = vertices.Find(v => v.position == vertex.position && v.normal == vertex.normal);

        return result != null;
    }
}

[System.Serializable]
public class VertexData
{
    public int index;
    public Vector3 position;
    public Vector3 normal;
    public Vector2 uv;

    public bool isOnCurrectPosition;

    public List<Triangle> trianglesContainingIt = new List<Triangle>();

    public VertexData Copy()
    {
        return new VertexData(index, position, normal, uv);
    }

    public VertexData(int index, Vector3 position, Vector3 normal, Vector2 uv)
    {
        this.index = index;
        this.position = position;
        this.normal = normal;
        this.uv = uv;
    }

    public VertexData CloneWithNormal(Vector3 newNormal)
    {
        return new VertexData(index, position, newNormal, uv);

    }
}

public class Triangle
{
    public VertexData vertexA;
    public VertexData vertexB;
    public VertexData vertexC;

    public Vector3 Normal => vertexA.normal;

    public Triangle(VertexData vertexA, VertexData vertexB, VertexData vertexC)
    {
        this.vertexA = vertexA;
        this.vertexB = vertexB;
        this.vertexC = vertexC;

        vertexA.trianglesContainingIt.Add(this);
        vertexB.trianglesContainingIt.Add(this);
        vertexC.trianglesContainingIt.Add(this);
    }

    ~Triangle()
    {
        vertexA.trianglesContainingIt.Remove(this);
        vertexB.trianglesContainingIt.Remove(this);
        vertexC.trianglesContainingIt.Remove(this);
    }

    public Triangle (Point point)
    {
        vertexA = point.lastPoint.vertex;
        vertexB = point.vertex;
        vertexC = point.nextPoint.vertex;
    }

    public VertexData this[int i]
    {
        get
        {
            switch (i)
            {
                case 0: return vertexA;
                case 1: return vertexB;
                case 2: return vertexC;
                default: return null;
            }
        }

        set
        {
            switch (i)
            {
                case 0: vertexA = value; break;
                case 1: vertexB = value; break;
                case 2: vertexC = value; break;
                default: return;
            }
        }
    }

    public bool Containt(VertexData vertexData)
    {
        for (int i = 0; i < 3; i++)
        {
            if (this[i] == vertexData)
            {
                return true;
            }

        }

        // Vertex isn't part from this triangle
        return false;
    }
    
    public bool Containt(Vector3 position)
    {
        for (int i = 0; i < 3; i++)
        {
            if (this[i].position == position)
            {
                return true;
            }

        }

        // Vertex isn't part from this triangle
        return false;
    }

    public VertexData VertexAfter(VertexData vertex)
    {
        if (vertex == vertexA)
            return vertexB;
        else if (vertex == vertexB)
            return vertexC;
        else if (vertex == vertexC)
            return vertexA;
        else
            return null;
    }

    public Vector3 GetPointInTheMiddle()
    {
        return new Vector3
        {
            x = (vertexA.position.x + vertexB.position.x + vertexC.position.x) / 3,
            y = (vertexA.position.y + vertexB.position.y + vertexC.position.y) / 3,
            z = (vertexA.position.z + vertexB.position.z + vertexC.position.z) / 3,
        };
    }

    public bool TryRaycastVertexIntoThisTriangle(VertexData vertex, Vector3 direction)
    {
        Plane plane = new Plane(vertexA.position, vertexB.position, vertexC.position);

        Ray ray = new Ray(vertex.position, direction);
        if (!plane.Raycast(ray, out float distance))
        {
            return false;
        }

        vertex.position += (direction * distance);
        EditUV(vertex);
        return true;
    }

    private void EditUV(VertexData raycastedVertex)
    {
        float totalwight = 0;
        for (int i = 0; i < 3; i++)
        {
            VertexData vertex = this[i];
            float distance = Vector3.Distance(raycastedVertex.position, vertex.position);

            raycastedVertex.uv += vertex.uv * distance;
            raycastedVertex.normal += vertex.normal * distance;
            totalwight += distance;
        }

        raycastedVertex.uv /= totalwight;
        raycastedVertex.normal /= totalwight;
    }

    public Vector3[] ToArray()
    {
        return new[]
        {
            vertexA.position,
            vertexB.position,
            vertexC.position
        };
    }
}
