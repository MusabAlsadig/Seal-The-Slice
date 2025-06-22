using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

internal static class MeshSlicer
{

    public static CutResult SeperateByCut(CuttableObject cuttable, CutterBase cutter)
    {
        VertexMesh mesh = new VertexMesh(cuttable.SharedMesh);
        mesh.MoveAround(cuttable.transform, cutter.transform);
        CutShape cut = cutter.GetShape();

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

        VertexMesh insideMeshFill = new VertexMesh();
        VertexMesh outsideMeshFill = new VertexMesh();
        FillTheInside(insideMesh, true, cut, insideMeshFill);
        FillTheInside(outsideMesh, false, cut, outsideMeshFill);

        GameObject insideObject = CloneObject(cuttable.gameObject, cutter, insideMesh, "inside");
        GameObject outsideObject = CloneObject(cuttable.gameObject, cutter, outsideMesh, "outside");

        GameObject insideObjectFill = CloneObject(cuttable.gameObject, cutter, insideMeshFill, "inside fill");
        GameObject outsideObjectFill = CloneObject(cuttable.gameObject, cutter, outsideMeshFill, "outside fill");


        if (cutter.Material != null)
        {
            insideObjectFill.GetComponent<MeshRenderer>().material = cutter.Material;
            outsideObjectFill.GetComponent<MeshRenderer>().material = cutter.Material;
        }

        var result = new CutResult(outsideObject, insideObject, outsideObjectFill, insideObjectFill);

        // reparent to base object
        foreach (var subobject in result)
        {
            subobject.transform.SetParent(cuttable.transform);
        }

        outsideObjectFill.transform.SetParent(outsideObject.transform);
        insideObjectFill.transform.SetParent(insideObject.transform);

        return result;
    }

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

    private static void Organize(ref List<VertexData> vertices)
    {

        Vector2 centerPoint = Vector2.zero;

        foreach (var vertex in vertices)
        {
            centerPoint += (Vector2)vertex.position;
        }
        centerPoint /= vertices.Count;
        vertices = vertices.OrderByDescending(v => Vector2.SignedAngle(Vector2.right, (Vector2)v.position - centerPoint)).ToList();
    }
    
    private static void Organize(ref List<Point> points)
    {

        Vector2 centerPoint = Vector2.zero;

        foreach (var point in points)
        {
            centerPoint += (Vector2)point.Position;
        }
        centerPoint /= points.Count;
        points = points.OrderByDescending(p => Vector2.SignedAngle(Vector2.right, (Vector2)p.Position - centerPoint)).ToList();
    }




    private static void FillTheInside(VertexMesh mesh, bool isPositive, CutShape cut, VertexMesh otherMeshToPutInto = null)
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

                    newVertices[plane].Add(mesh.Vertices[i].Copy());
                }
            }
        }

        if (otherMeshToPutInto != null)
            mesh = otherMeshToPutInto;

        foreach (var pair in newVertices)
        {
            LimitedPlane plane = pair.Key;
            List<VertexData> vertices = pair.Value;

            Organize(ref vertices, plane.Plane.normal);

            GenerateTrianglesForPlane(ref mesh, isPositive, plane.Plane.normal, vertices);
        }
    }

    #region Inside Filling
    private static void GenerateTrianglesForPlane(ref VertexMesh mesh, bool isPositive, Vector3 cutNormal, List<VertexData> pointsAlongPlane, bool makeSquareUV = true)
    {

        Vector3 centerPoint = Vector3.zero;

        Vector3 minPoint = Vector3.one * float.MaxValue;
        Vector3 maxPoint = Vector3.one * float.MinValue;
        foreach (var vertex in pointsAlongPlane)
        {
            centerPoint += vertex.position;

            minPoint = Vector3.Min(vertex.position, minPoint);
            maxPoint = Vector3.Max(vertex.position, maxPoint);
        }
        centerPoint /= pointsAlongPlane.Count;

        float totalDistance_z = maxPoint.z - minPoint.z;

        Vector2 distance_xy = (Vector2)(maxPoint - minPoint);
        float totalDistance_r = distance_xy.sqrMagnitude;

        float highestTotalDistance = Mathf.Max(totalDistance_r, totalDistance_z);

        foreach (var vertex in pointsAlongPlane)
        {
            if (makeSquareUV)
                FixUV(vertex, minPoint, highestTotalDistance, highestTotalDistance);
            else
                FixUV(vertex, minPoint, totalDistance_z, totalDistance_r);
        }

        VertexData halfway = new VertexData(-1, centerPoint, cutNormal, -Vector2.one);

        if (makeSquareUV)
            FixUV(halfway, minPoint, highestTotalDistance, highestTotalDistance);
        else
            FixUV(halfway, minPoint, totalDistance_z, totalDistance_r);

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

    private static void FixUV(VertexData vertex, Vector3 minPoint, float totalDistance_z, float totalDistance_r)
    {
        float distance_z = vertex.position.z - minPoint.z;
        float distance_r = ((Vector2)(vertex.position - minPoint)).sqrMagnitude;


        float zRatio = distance_z / totalDistance_z;
        float rRatio = distance_r / totalDistance_r;
        vertex.uv.x = Mathf.Lerp(0, 1, zRatio);
        vertex.uv.y = Mathf.Lerp(0, 1, rRatio);
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


    public static CutResult SeperateByCut_InsertWithEarClipper(CuttableObject cuttable, CutterBase cutter)
    {
        VertexMesh mesh = new VertexMesh(cuttable.SharedMesh);
        mesh.MoveAround(cuttable.transform, cutter.transform);
        Polygon cut = cutter.ToPolygon();
        cut.Refresh();
        PolyTree hole = new PolyTree();
        hole.shape = cut;

        // remove tringles that cross the cut,
        // or at least contain vertices closest to the cut
        List<Triangle> removedTriangles = new List<Triangle>();
        foreach (var triangle in mesh.Triangles)
        {
            if (cut.Intersect(triangle))
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



            VertexData startVertex = removedTriangles[0].vertexA;
            VertexData currentVertex = startVertex;
            Triangle currentTriangle = startVertex.trianglesContainingIt[0];

            
            List<VertexData> verticesInCurrentPlane = new List<VertexData>();
            var leftTriangles = new List<Triangle>();
            AddVerticesTouchingTriangle(ref verticesInCurrentPlane, currentTriangle, ref removedTriangles, ref leftTriangles);
            var tringles = SeperateByNormals(leftTriangles);
            Dictionary<Vector3, Polygon> polygons = SeperateByNormals(verticesInCurrentPlane);
            

            while (polygons.Count > 0)
            {
                Vector3 normal = polygons.Keys.First();
                Polygon polygon = polygons[normal];
                Organize(ref polygon.points);
                List<Triangle> currentShapeTriangles = tringles[normal];
                PolyTree holeCopy = hole.Copy();
                List<Point> pointsOutside = new List<Point>();
                Dictionary<Point, Triangle> pointsInside = new Dictionary<Point, Triangle>();
                foreach (Point point in holeCopy.shape)
                {
                    bool foundTriangle = false;
                    foreach (var triangle in currentShapeTriangles)
                    {
                        // try to see both forward and backward, since the cut might be inside the mesh
                        if (EarClipper.IsPointInTriangle(point.Position, triangle.vertexA.position, triangle.vertexB.position, triangle.vertexC.position))
                        {
                            if (triangle.TryRaycastVertexIntoThisTriangle(point.vertex, Vector3.forward) ||
                                triangle.TryRaycastVertexIntoThisTriangle(point.vertex, Vector3.back))
                            {
                                pointsInside.Add(point, triangle);
                                foundTriangle = true;
                                break;
                            }

                        }

                    }
                    if (!foundTriangle)
                        pointsOutside.Add(point);
                }

                List<Vector3> intersections = new List<Vector3>();
                List<Point> points = new List<Point>(polygon.points);
                for (int i = 0; i < pointsOutside.Count; i++)
                {
                    Point point = pointsOutside[i];

                    Point lastPoint = point.lastPoint;
                    Point nextPoint = point.nextPoint;
                    
                    for (int j = 0; j < points.Count; j++)
                    {
                        VertexData currentVertexInPolygon = points[j].vertex;
                        VertexData nextVertexInPolygon = points[(j + 1) % points.Count].vertex;

                        if (TryFindIntesectionPoint(currentVertexInPolygon.position, nextVertexInPolygon.position, lastPoint.Position, point.Position, out Vector3 intersectionPosition) &&
                            !intersections.Contains(intersectionPosition))
                        {
                            intersections.Add(intersectionPosition);

                            VertexData intersectionVertex = new VertexData(-1, intersectionPosition, currentVertexInPolygon.normal, currentVertexInPolygon.uv);
                            int holeIndex = holeCopy.shape.points.IndexOf(point);
                            holeCopy.shape.points.Insert(holeIndex, new Point(intersectionVertex));

                            int polygonIndex = polygon.points.FindIndex(p => p.vertex.position == currentVertexInPolygon.position);
                            polygon.points.Insert(polygonIndex + 1, new Point(intersectionVertex));
                        }


                        if (TryFindIntesectionPoint(currentVertexInPolygon.position, nextVertexInPolygon.position, nextPoint.Position, point.Position, out Vector3 intersectionPosition2) &&
                            !intersections.Contains(intersectionPosition2))
                        {
                            intersections.Add(intersectionPosition2);


                            VertexData intersectionVertex = new VertexData(-1, intersectionPosition2, currentVertexInPolygon.normal, currentVertexInPolygon.uv);
                            int holeIndex = holeCopy.shape.points.IndexOf(point);
                            holeCopy.shape.points.Insert((holeIndex + 1) % polygon.points.Count, new Point(intersectionVertex));

                            int polygonIndex = polygon.points.FindIndex(p => p.vertex.position == nextVertexInPolygon.position);
                            polygon.points.Insert((polygonIndex - 1 + polygon.points.Count) % polygon.points.Count, new Point(intersectionVertex));
                        }
                    }



                        if (i == pointsOutside.Count - 1)
                            Debug.Log("");
                    holeCopy.shape.points.Remove(point);
                }

                Organize(ref polygon.points);
                polygon.Refresh();
                if (holeCopy.shape.direction == polygon.direction)
                    holeCopy.shape.Reverse();
                else
                    holeCopy.shape.Refresh();


                innerTriangles.AddRange(EarClipper.FillPolygoneTree(holeCopy));

                // seperate polygon into multiple if needed
                Polygon currentShape = polygon;
                Polygon otherShape = holeCopy.shape;

                for (int saftyCounter1 = 0; saftyCounter1 < 100; saftyCounter1++)
                {
                    if (currentShape.PointsCount == 0 && otherShape.PointsCount == 0)
                        break;

                    Point startPoint = currentShape.points.Find(p => holeCopy.shape.points.Find(p2 => p2.vertex == p.vertex) == null);
                    Point currentPoint = startPoint;
                    Polygon cuttedPolygon = new Polygon();


                    for (int saftyCounter = 0; saftyCounter < 1000; saftyCounter++)
                    {
                        cuttedPolygon.points.Add(currentPoint);

                        currentShape.Remove(currentPoint);
                        

                        Point pointOnOtherShape = otherShape.points.Find(p => p.vertex == currentPoint.vertex);
                        if (pointOnOtherShape != null)
                        {
                            currentPoint = pointOnOtherShape.nextPoint;
                            otherShape.Remove(pointOnOtherShape);

                            // switch shapes
                            if (otherShape == holeCopy.shape)
                            {
                                otherShape = polygon;
                                currentShape = holeCopy.shape;
                            }
                            else
                            {
                                otherShape = holeCopy.shape;
                                currentShape = polygon;
                            }
                        }
                        else
                            currentPoint = currentPoint.nextPoint;


                        if (currentPoint == startPoint)
                            break;
                    }

                    PolyTree polyTree = new PolyTree();
                    if (cuttedPolygon.points.Count != 0)
                    {
                        cuttedPolygon.Refresh();
                        polyTree.shape = cuttedPolygon;
                    }
                    else
                    {
                        polyTree.shape = polygon;
                        polyTree.AddChild(holeCopy);
                    }
                    polygons.Remove(normal);


                    newTriangles.AddRange(EarClipper.FillPolygoneTree(polyTree));
                    holeCopy.shape.Reverse();
                    
                }
            }
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

    private static void AddVerticesTouchingTriangle(ref List<VertexData> vertices , Triangle triangle, ref List<Triangle> trianglesToSearch, ref List<Triangle> currentTriangles)
    {

        for (int i = 0; i < 3; i++)
        {
            VertexData vertex = triangle[i];
            if (!vertices.Contains(vertex))
                vertices.Add(vertex);

            Triangle t = trianglesToSearch.Find(t => t.Containt(vertex.position));
            if (t != null)
            {

                trianglesToSearch.Remove(t);
                currentTriangles.Add(t);
                AddVerticesTouchingTriangle(ref vertices, t, ref trianglesToSearch, ref currentTriangles);
            }
        }
    }

    private static Dictionary<Vector3,Polygon> SeperateByNormals(List<VertexData> verts)
    {
        Dictionary<Vector3,Polygon> polygons = new Dictionary<Vector3, Polygon>();
        foreach (var v in verts)
        {
            if (polygons.ContainsKey(v.normal))
                polygons[v.normal].points.Add(new Point(v));
            else
            {
                Polygon p = new Polygon();
                p.points.Add(new Point(v));
                polygons.Add(v.normal, p);
            }
        }

        foreach (Polygon p in polygons.Values)
        {
            p.Refresh();
        }

        return polygons;
    }

    private static Dictionary<Vector3,List<Triangle>> SeperateByNormals(List<Triangle> triangles)
    {
        Dictionary<Vector3, List<Triangle>> result = new Dictionary<Vector3, List<Triangle>>();
        foreach (var t in triangles)
        {
                if (result.ContainsKey(t.Normal))
                    result[t.Normal].Add(t);
                else
                {
                    result.Add(t.Normal, new List<Triangle>() { t });
                }
        }

        return result;
    }

    public static bool TryFindIntesectionPoint(Vector3 p11, Vector3 p12, Vector3 p21, Vector3 p22, out Vector3 intersection)
    {
        intersection = Vector3.zero;


        Vector3 p = p11;
        Vector3 r = p12 - p11;

        Vector3 q = p21;
        Vector3 s = p22 - p21;

        float cross_rs = Cross(r, s);

        float t = Cross((q - p), s) / cross_rs;
        float u = Cross((q - p), r) / cross_rs;

        if (cross_rs != 0 &&
            t >= 0  && t <= 1 &&
            u >= 0 && u <= 1)

        {
            intersection = p + t * r;
            return true;
        }
        else 
            return false;

        float Cross(Vector2 v, Vector2 w)
        {
            return v.x * w.y - v.y * w.x;
        }
    }

}
