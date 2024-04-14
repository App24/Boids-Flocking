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

        private bool particlePlaying;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnMove(InputValue value)
        {
            var vector = value.Get<Vector2>();
            moveVector.x = vector.x;
            moveVector.y = vector.y;
        }

        private void OnAscend(InputValue value)
        {
            moveVector.z = value.Get<float>();
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
