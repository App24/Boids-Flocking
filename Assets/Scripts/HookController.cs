using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class HookController : MonoBehaviour
    {
        [SerializeField]
        private Transform clawTransform;

        [SerializeField]
        private Transform ropeTransform;

        private float ropeExtension;

        private enum RopeExtensionType
        {
            Still,
            Extending,
            Retreating
        }

        private RopeExtensionType ropeExtensionType;

        [SerializeField]
        private float maxExtensionLength;

        [SerializeField]
        private float extensionSpeed;

        private float ropeRenderSize;

        public bool CanExtendHook => ropeExtensionType == RopeExtensionType.Still;

        private void Awake()
        {
            ropeRenderSize = ropeTransform.GetChild(0).localScale.y * 2f;
        }

        private void SetRopeExtensionVisual()
        {
            clawTransform.localPosition = new Vector3(0, -ropeExtension);

            ropeTransform.localScale = new Vector3(1, ropeExtension / ropeRenderSize, 1);
        }

        private void Update()
        {
            if (ropeExtensionType == RopeExtensionType.Extending)
            {
                ropeExtension += Time.deltaTime * extensionSpeed;
                if (ropeExtension >= maxExtensionLength)
                {
                    ropeExtension = maxExtensionLength;
                    ropeExtensionType = RopeExtensionType.Retreating;
                }
                SetRopeExtensionVisual();
            }
            else if (ropeExtensionType == RopeExtensionType.Retreating)
            {
                ropeExtension -= Time.deltaTime * extensionSpeed;
                if (ropeExtension < 0)
                {
                    ropeExtension = 0;
                    ropeExtensionType = RopeExtensionType.Still;
                }
                SetRopeExtensionVisual();
            }
        }

        public void ExtendHook()
        {
            ropeExtensionType = RopeExtensionType.Extending;
        }

        public void RetractHook()
        {
            ropeExtensionType = RopeExtensionType.Retreating;
        }
    }
}
