
using UnityEngine;

namespace SealTheSlice
{
    [RequireComponent(typeof(Rigidbody))]
    public class SimpleMover : MonoBehaviour
    {
        [SerializeField]
        private float speed = 20;
        [SerializeField]
        private float delay = 0;

        private void Awake()
        {
            Invoke(nameof(Move), delay);
        }

        private void Move()
        {
            GetComponent<Rigidbody>().AddForce(transform.forward * speed);
        }
    }
}
