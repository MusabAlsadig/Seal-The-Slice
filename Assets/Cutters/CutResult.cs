using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutResult : IEnumerable<GameObject>
{
    public GameObject outterShape;
    public GameObject innerShape;

    public CutResult(GameObject outterShape, GameObject innerShape)
    {
        this.outterShape = outterShape;
        this.innerShape = innerShape;
    }

    public List<GameObject> GetAllShapes()
    {
        return new List<GameObject> { outterShape, innerShape };
    }

    public IEnumerator<GameObject> GetEnumerator()
    {
        yield return outterShape;
        yield return innerShape;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new System.NotImplementedException();
    }
}
