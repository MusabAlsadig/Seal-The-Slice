using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutResult : IEnumerable<GameObject>
{
    public GameObject outterShape;
    public GameObject innerShape;
    public GameObject outerShapeFill;
    public GameObject innerShapeFill;

    public CutResult(GameObject outterShape, GameObject innerShape)
    {
        this.outterShape = outterShape;
        this.innerShape = innerShape;
    }
    
    public CutResult(GameObject outterShape, GameObject innerShape, GameObject outerShapeFill, GameObject innerShapeFill)
    {
        this.outterShape = outterShape;
        this.innerShape = innerShape;
        this.outerShapeFill = outerShapeFill;
        this.innerShapeFill = innerShapeFill;
    }

    public IEnumerator<GameObject> GetEnumerator()
    {
        yield return outterShape;
        yield return innerShape;
        yield return outerShapeFill;
        yield return innerShapeFill;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new System.NotImplementedException();
    }
}
