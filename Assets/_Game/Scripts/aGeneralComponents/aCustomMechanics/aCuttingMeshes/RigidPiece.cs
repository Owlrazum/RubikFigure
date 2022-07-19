using System.Collections;

using UnityEngine;

namespace CustomMechanincs.CuttingMeshes
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidPiece : MonoBehaviour
    {
        [SerializeField]
        private float _forceAmount;

        private Rigidbody rigid;

        private void Awake()
        {
            TryGetComponent(out rigid);

            Deactivate();
        }

        public void ActivateMovable()
        {
            StartCoroutine(ActivateMovableSequence());
        }

        private IEnumerator ActivateMovableSequence()
        {
            yield return new WaitForFixedUpdate();
            rigid.isKinematic = false;
            rigid.useGravity = true;
            rigid.AddForce(new Vector3(0, 0.5f, -0.1f) * _forceAmount);
            var box = gameObject.AddComponent<BoxCollider>();
            box.size *= 0.8f;
            yield return new WaitForSeconds(1);
            gameObject.SetActive(false);
        }

        public void ActivateRestrained()
        {
            StartCoroutine(ActivateRestrainedSequence());
        }

        private IEnumerator ActivateRestrainedSequence()
        {
            yield return new WaitForFixedUpdate();
            //rigid.isKinematic = false;
            var box = gameObject.AddComponent<BoxCollider>();
            box.size *= 0.8f;
        }


        public void Deactivate()
        {
            rigid.isKinematic = true;
        }
    }
}