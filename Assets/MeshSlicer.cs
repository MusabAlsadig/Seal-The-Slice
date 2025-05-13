using System.Collections.Generic;
using UnityEngine;

internal class MeshSlicer
{

    public static Mesh[] CutMeshTo2(Mesh normalMesh, Vector3 cutNormal, float distance)
    {
        Plane plane = new Plane(cutNormal, distance);
        cutNormal.Normalize();
        VertexMesh positiveMesh = new VertexMesh();
        VertexMesh negativeMesh = new VertexMesh();

        VertexMesh mesh = new VertexMesh(normalMesh);
        List<VertexData> pointsAlongPlane = new List<VertexData>();
        foreach (var triangle in mesh.Triangles)
        {
            VertexData vertexA = triangle.vertexA;
            VertexData vertexB = triangle.vertexB;
            VertexData vertexC = triangle.vertexC;
            bool isABSameSide = plane.SameSide( vertexA.position, vertexB.position);
            bool isBCSameSide = plane.SameSide( vertexB.position, vertexC.position);


            if (isABSameSide && isBCSameSide)
            {
                // all this tringle is in the same side
                VertexMesh helper = plane.GetSide(vertexA.position) ? positiveMesh : negativeMesh;
                helper.AddTriangle(vertexA, vertexB, vertexC);
            }
            else
            {
                //we have to find intersection between triangle and cutting plane 
                VertexData intersectionD;
                VertexData intersectionE;
                //Determine appropirate helper for each triangle corner
                VertexMesh helperA = plane.GetSide(vertexA.position) ? positiveMesh : negativeMesh;
                VertexMesh helperB = plane.GetSide(vertexB.position) ? positiveMesh : negativeMesh;
                VertexMesh helperC = plane.GetSide(vertexC.position) ? positiveMesh : negativeMesh;

                if (isABSameSide)
                {
                    intersectionD = mesh.CreateIntersectionVertex(vertexA, vertexC, plane);
                    intersectionE = mesh.CreateIntersectionVertex(vertexB, vertexC, plane);

                    helperA.AddTriangle(vertexA, vertexB, intersectionE);
                    helperA.AddTriangle(vertexA, intersectionE, intersectionD);
                    helperC.AddTriangle(intersectionE, vertexC, intersectionD);
                }
                else if (isBCSameSide)
                {
                    intersectionD = mesh.CreateIntersectionVertex(vertexB, vertexA, plane);
                    intersectionE = mesh.CreateIntersectionVertex(vertexC, vertexA, plane);

                    helperB.AddTriangle(vertexB, vertexC, intersectionE);
                    helperB.AddTriangle(vertexB, intersectionE, intersectionD);
                    helperA.AddTriangle(intersectionE, vertexA, intersectionD);
                }
                else
                {
                    intersectionD = mesh.CreateIntersectionVertex(vertexA, vertexB, plane);
                    intersectionE = mesh.CreateIntersectionVertex(vertexC, vertexB, plane);

                    helperA.AddTriangle(vertexA, intersectionE, vertexC);
                    helperA.AddTriangle(intersectionD, intersectionE, vertexA);
                    helperB.AddTriangle(vertexB, intersectionE, intersectionD);
                }


                pointsAlongPlane.Add(intersectionD);
                pointsAlongPlane.Add(intersectionE);
            }

        }

        // fill the faces inside of the shape
        mesh.JoinPointsAlongPlane(ref positiveMesh, ref negativeMesh, cutNormal, pointsAlongPlane);

        return new[] { positiveMesh.ToMesh(), negativeMesh.ToMesh() };
    }


    public static Mesh[] SeperateByCut(VertexMesh mesh, CutShape cut)
    {
        foreach (var plane in cut.planes)
        {
            SliceMesh(ref mesh, plane.Plane);
        }

        VertexMesh insideMesh = new VertexMesh();
        VertexMesh outsideMesh = new VertexMesh();
        foreach (var triangle in mesh.Triangles)
        {
            if (cut.IsInside(triangle))
                insideMesh.AddTriangle(triangle);
            else
                outsideMesh.AddTriangle(triangle);
        }

        return new[] { insideMesh.ToMesh(), outsideMesh.ToMesh()};
    }
    
    public static void SliceMesh(ref VertexMesh mesh, Plane plane)
    {
        List<Triangle> trianglesToRemove = new List<Triangle>();
        List<Triangle> trianglesToAdd = new List<Triangle>();
        foreach (var triangle in mesh.Triangles)
        {
            VertexData vertexA = triangle.vertexA;
            VertexData vertexB = triangle.vertexB;
            VertexData vertexC = triangle.vertexC;

            bool isABSameSide = plane.SameSide(vertexA.position, vertexB.position);
            bool isBCSameSide = plane.SameSide(vertexB.position, vertexC.position);

            // this tringle is outside this plane, just leave it
            if (isABSameSide && isBCSameSide)
                continue;

            // this tringle cross this plane, need to edit it
            trianglesToRemove.Add(triangle);
            VertexData intersectionD;
            VertexData intersectionE;

            if (isABSameSide)
            {
                intersectionD = mesh.CreateIntersectionVertex(vertexA, vertexC, plane);
                intersectionE = mesh.CreateIntersectionVertex(vertexB, vertexC, plane);

                trianglesToAdd.Add(new Triangle( vertexA, vertexB, intersectionE));
                trianglesToAdd.Add(new Triangle(vertexA, intersectionE, intersectionD));
                trianglesToAdd.Add(new Triangle(intersectionE, vertexC, intersectionD));
            }
            else if (isBCSameSide)
            {
                intersectionD = mesh.CreateIntersectionVertex(vertexB, vertexA, plane);
                intersectionE = mesh.CreateIntersectionVertex(vertexC, vertexA, plane);

                trianglesToAdd.Add(new Triangle(vertexB, vertexC, intersectionE));
                trianglesToAdd.Add(new Triangle(vertexB, intersectionE, intersectionD));
                trianglesToAdd.Add(new Triangle(intersectionE, vertexA, intersectionD));
            }
            else
            {
                intersectionD = mesh.CreateIntersectionVertex(vertexA, vertexB, plane);
                intersectionE = mesh.CreateIntersectionVertex(vertexC, vertexB, plane);

                trianglesToAdd.Add(new Triangle(vertexA, intersectionE, vertexC));
                trianglesToAdd.Add(new Triangle(intersectionD, intersectionE, vertexA));
                trianglesToAdd.Add(new Triangle(vertexB, intersectionE, intersectionD));
            }
        }

        foreach (var triangle in trianglesToRemove)
        {
            mesh.RemoveTriangle(triangle);
        }
        foreach(var triangle in trianglesToAdd)
        {
            mesh.AddTriangle(triangle);
        }
    }
}
