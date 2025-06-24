using System.Collections.Generic;
using UnityEngine;

public class ColliderBasedCutter : CutterBase
{
    private List<CuttableRootObject> objectsCurrentlyCutting = new List<CuttableRootObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (objectsCurrentlyCutting.Count > 10)
        {
            Debug.Log("is this an infinit loop?, there is more than 100 objects getting cut at once");
            return;
        }
        if (other.TryGetComponent(out CuttableObject cuttableObject))
        {
            if (objectsCurrentlyCutting.Contains(cuttableObject.Root))
                return;

            CutResult cutResult = Cut(cuttableObject);
            if (cutResult == null)
                return;

            objectsCurrentlyCutting.Add(cuttableObject.Root);
            Debug.Log("cutting" + other.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CuttableObject cuttableObject))
            objectsCurrentlyCutting.Remove(cuttableObject.Root);
    }

}
