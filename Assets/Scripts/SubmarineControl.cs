using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Boids
{
    [RequireComponent(typeof(Rigidbody))]
    public class SubmarineControl : MonoBehaviour
    {
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

        [SerializeField]
        private HookController hookController;

        private bool particlePlaying;

        private void Awake()
        {
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
            if(value > 0 && hookController.CanExtendHook)
            {
                hookController.ExtendHook();
            }
        }

        private void FixedUpdate()
        {
            //rb.MovePosition(moveVector * moveSpeed * Time.fixedDeltaTime);
            rb.AddRelativeForce(new Vector3(0, 0, moveVector.y) * moveSpeed);
            rb.AddRelativeForce(new Vector3(0, moveVector.z, 0) * ascendVelocity);
            rb.AddRelativeTorque(new Vector3(0, moveVector.x, 0) * turningSpeed);

            var motorSpeed = moveVector.magnitude;

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
    }
}
