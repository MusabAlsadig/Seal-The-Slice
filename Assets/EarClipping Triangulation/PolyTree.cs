using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PolyTree
{
    public Polygon shape;
    [SerializeField] private List<PolyTree> children = new List<PolyTree>();

    public int ChildrenCount => children.Count;

    public PolyTree Copy()
    {
        PolyTree copy = new PolyTree();
        copy.shape = shape.Copy();
        foreach (PolyTree child in children)
        {
            copy.children.Add(child.Copy());
        }

        return copy;
    }

    public void AddChild(PolyTree child)
    {
        if (child.shape.direction == shape.direction)
            child.shape.Reverse();

        children.Add(child);
    }

    public PolyTree GetChild(int index)
    {
        return children[index];
    }

    
}
