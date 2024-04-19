using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    public class DoorHandler : MonoBehaviour
    {
        [SerializeField]
        private GrabHandler grabHandler;

        [SerializeField]
        private float openHeight;

        private Vector3 startPosition;

        private bool fullyOpened;

        public UnityEvent onFullyOpened;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            if (!grabHandler.hooked) return;
            if (fullyOpened)
            {
                SubmarineControl.Instance.hookController.clawCollider.ReleaseGrab();
                return;
            }

            float distance = Vector3.Distance(transform.position, startPosition);

            if (distance >= openHeight)
            {
                fullyOpened = true;
                SubmarineControl.Instance.hookController.clawCollider.ReleaseGrab();
                onFullyOpened.Invoke();
            }
        }
    }
}
