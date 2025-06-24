
public class CuttableSubObject : CuttableObject
{
    public override CuttableRootObject Root => root;
    private CuttableRootObject root;
    public void Setup(CuttableRootObject root)
    {
        this.root = root;
    }
}
