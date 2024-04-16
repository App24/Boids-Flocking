using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class ClawCollider : MonoBehaviour
    {
        [SerializeField]
        private HookController hookController;

        public Transform grabTransform;

        private GrabHandler currentGrab;

        private void Awake()
        {
            hookController.clawCollider = this;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player") return;
            if (!hookController.IsExtending) return;
            if (hookController.CanExtendHook) return;

            var grabHandler = other.GetComponent<GrabHandler>();
            if (!grabHandler || hookController.IsHooked) hookController.RetractHook();
            else
            {
                SubmarineControl.Instance.KillMomentum();
                var distance = other.transform.position - hookController.transform.position;
                distance.y = Mathf.Abs(distance.y) - Mathf.Abs(grabTransform.localPosition.y);
                hookController.SetRopeExtension(distance.y);
                grabHandler.GrabOntoHook(this);
                hookController.GrabHook();
                currentGrab = grabHandler;
            }
        }

        public void ReleaseGrab()
        {
            if (!currentGrab) return;
            currentGrab.LetGo();
            hookController.RetractHook();
            currentGrab = null;
        }
    }
}
