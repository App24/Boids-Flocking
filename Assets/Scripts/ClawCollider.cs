using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class ClawCollider : MonoBehaviour
    {
        [SerializeField]
        private HookController hookController;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player") return;
            if (hookController.CanExtendHook) return;

            hookController.RetractHook();
        }
    }
}
