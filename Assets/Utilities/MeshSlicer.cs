using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal static class MeshSlicer
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


    //public static CutResult SeperateByCut(CuttableObject cuttable, CutterBase cutter)
    //{
    //    VertexMesh mesh = new VertexMesh(cuttable.SharedMesh);
    //    mesh.MoveAround(cuttable.transform, cutter.transform);
    //    CutShape cut = cutter.GetShape();

    //    foreach (var plane in cut.planes)
    //    {
    //        SliceMesh(ref mesh, plane.Plane);
    //    }

    //    VertexMesh insideMesh = new VertexMesh();
    //    VertexMesh outsideMesh = new VertexMesh();
    //    foreach (var triangle in mesh.Triangles)
    //    {
    //        if (cut.IsInside(triangle))
    //            insideMesh.AddTriangle(triangle);
    //        else
    //            outsideMesh.AddTriangle(triangle);
    //    }

    //    FillTheInside(ref insideMesh, true, cut);
    //    FillTheInside(ref outsideMesh, false, cut);

    //    GameObject insideObject = CloneObject(cuttable.gameObject, cutter, insideMesh, "inside");
    //    GameObject outsideObject = CloneObject(cuttable.gameObject, cutter, outsideMesh, "outside");

    //    var result = new CutResult(insideObject, outsideObject);

    //    // reparent to base object
    //    foreach (var subobject in result)
    //    {
    //        subobject.transform.SetParent(cuttable.transform);
    //    }

    //    return result;
    //}

    private static GameObject CloneObject(GameObject baseObject, CutterBase cutter,VertexMesh submesh, string name)
    {

        string undoLable = "Cut " + baseObject.name;


        submesh.ReturnNormal(baseObject.transform, cutter.transform);
        Mesh mesh = submesh.ToMesh();
        mesh.RecalculateBounds();

        mesh.name = baseObject.name + " cut ";
        GameObject submeshObject = Object.Instantiate(baseObject);
        submeshObject.name = $"{baseObject.name} {name}";
        Undo.RegisterCreatedObjectUndo(submeshObject, undoLable);
        submeshObject.GetComponent<MeshFilter>().sharedMesh = mesh;

        return submeshObject;
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

    private static void Organize(ref List<VertexData> vertices, Vector3 normal)
    {
        Vector3 centerPoint = Vector3.zero;

        foreach (var vertex in vertices)
        {
            centerPoint += vertex.position;
        }
        centerPoint /= vertices.Count;

        vertices = vertices.OrderBy(v => Vector3.SignedAngle(Vector3.forward, v.position - centerPoint, normal)).ToList();
    }

    private static void FillTheInside(ref VertexMesh mesh, bool isPositive, CutShape cut)
    {
        Dictionary<LimitedPlane, List<VertexData>> newVertices = new Dictionary<LimitedPlane, List<VertexData>>();
        for (int i = 0; i < mesh.Vertices.Count; i++)
        {
            // check all vertices and assigne them with the plane which cut them
            if (cut.TryGetPlanesThatCross(mesh.Vertices[i], out List<LimitedPlane> planes))
            {

                foreach (var plane in planes)
                {
                    // to ensure sure no null errors happen
                    if (!newVertices.ContainsKey(plane))
                        newVertices.Add(plane, new List<VertexData>());

                    newVertices[plane].Add(mesh.Vertices[i]);
                }
            }
        }

        foreach (var pair in newVertices)
        {
            LimitedPlane plane = pair.Key;
            List<VertexData> vertices = pair.Value;

            Organize(ref vertices, plane.Plane.normal);

            JoinPointsAlongPlane(ref mesh, isPositive, plane.Plane.normal, vertices);
        }
    }

    #region Inside Filling
    private static void JoinPointsAlongPlane(ref VertexMesh mesh, bool isPositive, Vector3 cutNormal, List<VertexData> pointsAlongPlane)
    {

        Vector3 centerPoint = Vector3.zero;

        foreach (var vertex in pointsAlongPlane)
        {
            centerPoint += vertex.position;
        }
        centerPoint /= pointsAlongPlane.Count;

        VertexData halfway = new VertexData(-1, centerPoint, cutNormal, pointsAlongPlane[0].uv);

        for (int i = 0; i < pointsAlongPlane.Count; i++)
        {
            int nextindex = i + 1 < pointsAlongPlane.Count ? i + 1 : 0;
            VertexData firstVertex = pointsAlongPlane[i];
            VertexData secondVertex = pointsAlongPlane[nextindex];

            Vector3 normal = ComputeNormal(halfway, secondVertex, firstVertex);

            float dot = Vector3.Dot(normal, cutNormal);

            if (dot > 0)
            {
                //used if calculated normal aligns with plane normal
                if (isPositive)
                    mesh.AddTriangle(firstVertex.CloneWithNormal(-cutNormal), secondVertex.CloneWithNormal(-cutNormal), halfway.CloneWithNormal(-cutNormal));
                else
                    mesh.AddTriangle(secondVertex.CloneWithNormal(cutNormal), firstVertex.CloneWithNormal(cutNormal), halfway.CloneWithNormal(cutNormal));
            }
            else
            {
                //used if calculated normal is opposite to plane normal
                if (isPositive)
                    mesh.AddTriangle(secondVertex.CloneWithNormal(-cutNormal), firstVertex.CloneWithNormal(-cutNormal), halfway.CloneWithNormal(-cutNormal));
                else
                    mesh.AddTriangle(firstVertex.CloneWithNormal(cutNormal), secondVertex.CloneWithNormal(cutNormal), halfway.CloneWithNormal(cutNormal));
            }
        }
    }


    private static Vector3 GetHalfwayPoint(List<VertexData> pointsAlongPlane)
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

    private static Vector3 ComputeNormal(VertexData a, VertexData b, VertexData c)
    {
        Vector3 sideL = b.position - a.position;
        Vector3 sideR = c.position - a.position;

        Vector3 normal = Vector3.Cross(sideL, sideR);

        return normal.normalized;
    }
    #endregion


    public static CutResult SeperateByCut(CuttableObject cuttable, CutterBase cutter)
    {
        VertexMesh mesh = new VertexMesh(cuttable.SharedMesh);
        mesh.MoveAround(cuttable.transform, cutter.transform);
        Polygon cut = cutter.ToPolygon();
        PolyTree hole = new PolyTree();
        hole.shape = cut;

        // remove tringles that cross the cut,
        // or at least contain vertices closest to the cut
        List<Triangle> removedTriangles = new List<Triangle>();
        foreach (var triangle in mesh.Triangles)
        {
            if (cut.IsAroundTheShape(triangle))
                removedTriangles.Add(triangle);
        }

        foreach (var triangle in removedTriangles)
        {
            // remove triangles from the mesh
            mesh.RemoveTriangle(triangle);
        }



        List<Triangle> newTriangles = new List<Triangle>();
        List<Triangle> innerTriangles = new List<Triangle>();
        

        
        while (removedTriangles.Count > 0)
        {
            // re-construct all polygons, one by one


            // search for unique vertices
            Triangle startTriangle = null;
            VertexData uniqueVertex = null;
            List<VertexData> uniqueVertices = new List<VertexData>();
            foreach (var triangle in removedTriangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (triangle[i].trianglesContainingIt.Count == 1)
                    {
                        startTriangle = triangle;
                        uniqueVertex = triangle[i];
                        uniqueVertices.Add(uniqueVertex);
                        break;
                    }
                }

            }

            List<VertexData> verticesInCurrentPlane = new List<VertexData>();
            VertexData startVertex = uniqueVertices[0];
            VertexData currentVertex = startVertex;
            Triangle currentTriangle = startVertex.trianglesContainingIt[0];

            List<Triangle> currentShapeTriangles = new List<Triangle>(); // will be used to raycast the cut forward onto the faces 
            do
            {
                // a vertex after a unique one
                currentVertex = currentTriangle.VertexAfter(currentVertex);
                verticesInCurrentPlane.Add(currentVertex);
                

                // unique vertex
                currentVertex = uniqueVertices.Find(v => v.trianglesContainingIt[0] != currentTriangle && v.trianglesContainingIt[0].Containt(currentVertex.position));
                verticesInCurrentPlane.Add(currentVertex);
                uniqueVertices.Remove(currentVertex);
                currentTriangle = currentVertex.trianglesContainingIt[0];
                removedTriangles.Remove(currentTriangle);
                currentShapeTriangles.Add(currentTriangle);

            } while (currentVertex != startVertex);


            PolyTree holeCopy = hole.Copy();
            foreach (var triangle in currentShapeTriangles)
            {
                foreach (VertexData vertex in holeCopy.shape)
                {
                    // try to see both forward and backward, since the cut might be inside the mesh
                    triangle.TryRaycastVertexIntoThisTriangle(vertex, Vector3.forward);
                    triangle.TryRaycastVertexIntoThisTriangle(vertex, Vector3.back);
                }
            }

            PolyTree polyTree = new PolyTree();
            polyTree.shape = new Polygon(verticesInCurrentPlane);
            polyTree.AddChild(holeCopy);

            

            newTriangles.AddRange(EarClipper.FillPolygoneTree(polyTree));
            holeCopy.shape.Reverse();
            innerTriangles.AddRange(EarClipper.FillPolygoneTree(holeCopy));
        }

        VertexMesh outsideMesh = mesh;
        foreach (var triangle in newTriangles)
        {
            mesh.AddTriangle(triangle);
        }

        VertexMesh insideMesh = new VertexMesh(innerTriangles);


        GameObject insideObject = CloneObject(cuttable.gameObject, cutter, insideMesh, "inside");
        GameObject outsideObject = CloneObject(cuttable.gameObject, cutter, outsideMesh, "outside");

        var result = new CutResult(insideObject, outsideObject);

        return result;
    }

}
