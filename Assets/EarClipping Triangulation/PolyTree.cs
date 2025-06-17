using System.Collections.Generic;

[System.Serializable]
public class PolyTree
{
    public CutShape shape;
    public List<PolyTree> children = new List<PolyTree>();
}
