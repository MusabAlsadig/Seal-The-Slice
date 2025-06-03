using System.Collections.Generic;
using UnityEngine;

public class ColliderBasedCutter : CutterBase
{
    private List<CuttableObject> objectsCurrentlyCutting = new List<CuttableObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CuttableObject cuttableObject))
        {
            if (objectsCurrentlyCutting.Contains(cuttableObject))
                return;

            objectsCurrentlyCutting.Add(cuttableObject);
            var newObjects = cuttableObject.CutWith(this);

            foreach (var submeshObject in newObjects)
            {
                objectsCurrentlyCutting.Add(submeshObject.GetComponent<CuttableObject>());
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CuttableObject cuttableObject))
            objectsCurrentlyCutting.Remove(cuttableObject);
    }

}
