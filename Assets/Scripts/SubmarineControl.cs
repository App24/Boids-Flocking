using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Boids
{
    [RequireComponent(typeof(Rigidbody))]
    public class SubmarineControl : MonoBehaviour
    {
        private static SubmarineControl instance;
        public static SubmarineControl Instance => instance;

        private Rigidbody rb;

        private Vector3 moveVector;

        [SerializeField]
        private float moveSpeed;

        [SerializeField]
        private float ascendVelocity;

        [SerializeField]
        private float turningSpeed;

        [SerializeField]
        private Transform rotorTransform;

        [SerializeField]
        private float rotorRotationSpeed;

        [SerializeField]
        private ParticleSystem rotorParticleSystem;

        [Header("UI")]

        [SerializeField]
        private float alphaReduce;

        [SerializeField]
        private CanvasGroup wKeyCanvasGroup;

        [SerializeField]
        private CanvasGroup sKeyCanvasGroup;

        [SerializeField]
        private CanvasGroup aKeyCanvasGroup;

        [SerializeField]
        private CanvasGroup dKeyCanvasGroup;

        [SerializeField]
        private CanvasGroup spaceKeyCanvasGroup;

        [SerializeField]
        private CanvasGroup shiftKeyCanvasGroup;

        [SerializeField]
        private CanvasGroup leftClickKeyCanvasGroup;

        public HookController hookController;

        private bool particlePlaying;

        [System.NonSerialized]
        public MovementCapabilities movementCapabilities = MovementCapabilities.All;

        private void Awake()
        {
            instance = this;
            rb = GetComponent<Rigidbody>();
        }

        private void OnMove(InputValue inputValue)
        {
            var vector = inputValue.Get<Vector2>();
            moveVector.x = vector.x;
            moveVector.y = vector.y;
        }

        private void OnAscend(InputValue inputValue)
        {
            moveVector.z = inputValue.Get<float>();
        }

        private void OnFire(InputValue inputValue)
        {
            var value = inputValue.Get<float>();
            if (value > 0)
            {
                if (hookController.CanExtendHook)
                {
                    hookController.ExtendHook();
                    leftClickKeyCanvasGroup.alpha = 0;
                }
                else if (hookController.IsHooked)
                {
                    hookController.clawCollider.ReleaseGrab();
                }
            }
        }

        private void FixedUpdate()
        {
            //rb.MovePosition(moveVector * moveSpeed * Time.fixedDeltaTime);

            var moveVector = new Vector3(Mathf.Clamp(this.moveVector.x, movementCapabilities.HasFlag(MovementCapabilities.RotateLeft) ? -1 : 0, movementCapabilities.HasFlag(MovementCapabilities.RotateRight) ? 1 : 0), Mathf.Clamp(this.moveVector.y, movementCapabilities.HasFlag(MovementCapabilities.Backward) ? -1 : 0, movementCapabilities.HasFlag(MovementCapabilities.Forward) ? 1 : 0), Mathf.Clamp(this.moveVector.z, movementCapabilities.HasFlag(MovementCapabilities.Down) ? -1 : 0, movementCapabilities.HasFlag(MovementCapabilities.Up) ? 1 : 0));

            if (moveVector.y > 0)
            {
                var canvasGroup = wKeyCanvasGroup;
                if (canvasGroup.alpha > 0)
                    canvasGroup.alpha -= Time.deltaTime * alphaReduce;
            }
            else if (moveVector.y < 0)
            {
                var canvasGroup = sKeyCanvasGroup;
                if (canvasGroup.alpha > 0)
                    canvasGroup.alpha -= Time.deltaTime * alphaReduce;
            }

            if (moveVector.x > 0)
            {
                var canvasGroup = dKeyCanvasGroup;
                if (canvasGroup.alpha > 0)
                    canvasGroup.alpha -= Time.deltaTime * alphaReduce;
            }
            else if (moveVector.x < 0)
            {
                var canvasGroup = aKeyCanvasGroup;
                if (canvasGroup.alpha > 0)
                    canvasGroup.alpha -= Time.deltaTime * alphaReduce;
            }

            if (moveVector.z > 0)
            {
                var canvasGroup = spaceKeyCanvasGroup;
                if (canvasGroup.alpha > 0)
                    canvasGroup.alpha -= Time.deltaTime * alphaReduce;
            }
            else if (moveVector.z < 0)
            {
                var canvasGroup = shiftKeyCanvasGroup;
                if (canvasGroup.alpha > 0)
                    canvasGroup.alpha -= Time.deltaTime * alphaReduce;
            }

            rb.AddRelativeForce(new Vector3(0, 0, moveVector.y) * moveSpeed);
            rb.AddRelativeForce(new Vector3(0, moveVector.z, 0) * ascendVelocity);
            rb.AddRelativeTorque(new Vector3(0, moveVector.x, 0) * turningSpeed);

            var motorSpeed = moveVector.normalized.magnitude;

            float rotorSpeed = rotorRotationSpeed * motorSpeed;

            rotorTransform.Rotate(new Vector3(0, 0, 1), rotorSpeed * Time.fixedDeltaTime);

            if (motorSpeed > 0 && !particlePlaying)
            {
                rotorParticleSystem.Play();
                particlePlaying = true;
            }
            else if (motorSpeed <= 0 && particlePlaying)
            {
                rotorParticleSystem.Stop();
                particlePlaying = false;
            }
        }

        public void KillMomentum()
        {
            rb.velocity = Vector3.zero;
            rb.ResetInertiaTensor();
            rb.angularVelocity = Vector3.zero;
        }

        [System.Flags]
        public enum MovementCapabilities
        {
            Forward = 1,
            Backward = 2,
            RotateLeft = 4,
            RotateRight = 8,
            Up = 16,
            Down = 32,
            All = Forward | Backward | RotateLeft | RotateRight | Up | Down,
        }
    }
}
