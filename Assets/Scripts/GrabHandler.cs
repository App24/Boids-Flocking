using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class GrabHandler : MonoBehaviour
    {
        [SerializeField]
        private Transform parentTransform;

        public void GrabOntoHook(ClawCollider claw)
        {
            var rotation = parentTransform.rotation;
            parentTransform.transform.SetParent(claw.grabTransform, false);
            parentTransform.transform.localPosition = -transform.localPosition;
            parentTransform.transform.rotation = rotation;
        }

        public void LetGo()
        {
            parentTransform.transform.SetParent(null, true);
        }
    }
}
