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

        private float startHeight;

        private bool fullyOpened;

        public UnityEvent onFullyOpened;

        private void Start()
        {
            startHeight = transform.position.y;
        }

        private void Update()
        {
            if (!grabHandler.hooked) return;
            if (fullyOpened)
            {
                SubmarineControl.Instance.hookController.clawCollider.ReleaseGrab();
                return;
            }

            float distance = Mathf.Abs(transform.position.y - startHeight);

            if (distance >= openHeight)
            {
                var position = transform.position;
                position.y = startHeight + openHeight;
                transform.position = position;
                fullyOpened = true;
                SubmarineControl.Instance.hookController.clawCollider.ReleaseGrab();
                onFullyOpened.Invoke();
            }
        }
    }
}
