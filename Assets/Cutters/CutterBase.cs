using System.Collections.Generic;
using UnityEngine;


public class CutterBase : MonoBehaviour
{
    public List<Vector2> points = new List<Vector2>();
    [SerializeField]
    private CuttableObject defaultTarget;


    public void CutShapeInFront()
    {
        CuttableObject target;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo) 
            && hitInfo.transform.TryGetComponent(out target))
        {
            Debug.Log("slicing " + target.name);
        }
        else
        {
            Debug.Log("no object there, will use default");
            target = defaultTarget;
        }

        Cut(target);
    }

    public CutResult Cut(CuttableObject target)
    {
        CutResult cutResult = MeshSlicer.SeperateByCut(target, this);
        target.AfterCutCleanup();
        return cutResult;
    }

    public virtual CutShape GetShape()
    {
        return new CutShape(points);
    }

    public Polygon ToPolygon()
    {
        var vertices = points.ConvertAll(p => new VertexData(-1, p, Vector3.zero, Vector2.zero));
        return new Polygon(vertices);
    }

    #region Utilites
    public void ReversDirection()
    {
        points.Reverse();
    }
    #endregion
}
