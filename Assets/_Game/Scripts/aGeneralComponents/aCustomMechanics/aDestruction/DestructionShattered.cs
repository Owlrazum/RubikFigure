using System.Collections;
using UnityEngine;

namespace CustomMechanics.Destruction
{
    [RequireComponent(typeof(Rigidbody))]
    public class DestructionShattered : MonoBehaviour
    {
        private Rigidbody rb;

        private Vector3 minimumScale = new Vector3(0.1f, 0.1f, 0.1f);
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }
        public void StartBreaking()
        {
            rb.isKinematic = false;
            StartCoroutine(ScaleDownCoroutine());
        }

        private IEnumerator ScaleDownCoroutine()
        {
            while (transform.localScale.x > minimumScale.x)
            {
                float delta = Time.deltaTime / 2;
                transform.localScale -= new Vector3(delta, delta, delta);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
