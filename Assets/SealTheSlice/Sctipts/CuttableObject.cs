using UnityEditor;
using UnityEngine;

namespace SealTheSlice
{
    public abstract class CuttableObject : MonoBehaviour
    {
        [SerializeField]
        private MeshFilter meshFilter;

        public bool BeenCuted { get; private set; }

        public Mesh SharedMesh => meshFilter.sharedMesh;

        public Bounds OriginalBounds { get; private set; }

        public abstract CuttableRootObject Root { get; }

        protected void OnValidate()
        {
            meshFilter = GetComponent<MeshFilter>();
            OriginalBounds = SharedMesh.bounds;
        }



        public void AfterCutCleanup()
        {
            string undoLable = "Cut " + name;

            Undo.RecordObject(gameObject, undoLable);
            name += " (sliced)";
            BeenCuted = true;
            if (Application.isPlaying)
            {
                Destroy(meshFilter);
                Destroy(GetComponent<Collider>());
                Destroy(GetComponent<MeshRenderer>());
                Destroy(this);
            }
            else
            {
                Undo.DestroyObjectImmediate(meshFilter);
                Undo.DestroyObjectImmediate(GetComponent<Collider>());
                Undo.DestroyObjectImmediate(GetComponent<MeshRenderer>());
                Undo.DestroyObjectImmediate(this);
            }
        }



    }
}