using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    public class GrabHandler : MonoBehaviour
    {
        [SerializeField]
        private Transform parentTransform;

        [System.NonSerialized]
        public bool hooked;

        public UnityEvent onHooked;

        public System.Action onRelease;

        public SubmarineControl.MovementCapabilities disableMovement;

        public void GrabOntoHook(ClawCollider claw)
        {
            parentTransform.transform.SetParent(claw.grabTransform, true);
            SubmarineControl.Instance.movementCapabilities &= ~disableMovement;
            hooked = true;
            onHooked?.Invoke();
        }

        public void LetGo()
        {
            parentTransform.transform.SetParent(null, true);
            SubmarineControl.Instance.movementCapabilities |= disableMovement;
            hooked = false;
            onRelease?.Invoke();
        }
    }
}
