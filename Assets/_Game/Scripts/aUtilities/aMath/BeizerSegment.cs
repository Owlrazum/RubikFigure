using UnityEngine;

namespace Orazum.Utilities
{ 
    /// <summary>
    /// Needs Initialization;
    /// </summary>
    public class BeizerSegment : MonoBehaviour
    {
        private CubicBeizerVector3Params _par;

        public void Initialize()
        {
            if (transform.childCount != 4)
            {
                Debug.LogError("Incorrect child amount");
            }

            _par = new CubicBeizerVector3Params();

            _par.initial = transform.GetChild(0).position;
            _par.anchor1 = transform.GetChild(1).position;
            _par.anchor2 = transform.GetChild(2).position;
            _par.target  = transform.GetChild(3).position;
        }

        public void Update()
        {
            _par.initial = transform.GetChild(0).position;
            _par.anchor1 = transform.GetChild(1).position;
            _par.anchor2 = transform.GetChild(2).position;
            _par.target  = transform.GetChild(3).position;
        }

        public Vector3 Target { get { return _par.target; } }

        public Vector3 GetLerpedPos(float lerpParam)
        {
            Vector3 toReturn = MathUtilities.ComputeCubicBeizerPos(_par, lerpParam); ;
            return toReturn;
        }

        public Vector3 GetSegmentStart()
        {
            return _par.initial;
        }

        public float GetApproximatedLength()
        {
            return Vector3.Distance(_par.initial, _par.target);
        }
    }
}
