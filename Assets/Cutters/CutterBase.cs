using System.Collections.Generic;
using UnityEngine;


public class CutterBase : MonoBehaviour
{
    public List<Vector2> points = new List<Vector2>();
    [SerializeField]
    private Transform defaultTarget;


    public void CutShapeInFront()
    {
        Transform target;
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo))
        {
            Debug.Log("no object there");
            target = defaultTarget;
        }
        else
        {
            Debug.Log(hitInfo.transform.name);
            target = hitInfo.transform;
        }

        target.GetComponent<CuttableObject>().CutWith(this);
    }


    public virtual CutShape GetShape()
    {
        return new CutShape(points);
    }

    #region Utilites
    public void ReversDirection()
    {
        points.Reverse();
    }
    #endregion
}
