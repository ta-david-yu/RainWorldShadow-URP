using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpookySea
{
    /// <summary>
    /// A rigidbody-based controller
    /// </summary>
    public class MarineMovementController : HasVelocityControllerBase
    {
        [Header("Reference")]

        [SerializeField]
        private Rigidbody2D m_Rigidbody;
        public Rigidbody2D Rigidbody { get { return m_Rigidbody; } }

        [Header("Settings")]

        [SerializeField]
        [Tooltip("The full speed of the ghost, unit per second")]
        private float m_FullSpeed = 2.5f;
        public override float FullSpeed { get { return m_FullSpeed; } protected set { m_FullSpeed = value; } }

        [SerializeField]
        [Tooltip("The speed acceleration when input, increse in speed per second")]
        private float m_Acceleration = 6.0f;
        public override float Acceleration { get { return m_Acceleration; } protected set { m_Acceleration = value; } }

        [SerializeField]
        [Tooltip("The speed deceleration when no input is presented, decrease in speed per second")]
        private float m_Deceleration = 4.0f;
        public override float Deceleration { get { return m_Deceleration; } protected set { m_Deceleration = value; } }

        public override Vector2 Velocity { get { return Rigidbody.velocity; } }

        private Vector2 m_DesiredMovement = Vector2.zero;
        public Vector2 DesiredMovement { get { return m_DesiredMovement; } }

        private Vector2 m_MovementInput = Vector2.zero;
        public Vector2 MovementInput { get { return m_MovementInput; } }

        public bool IsFrozen { get; private set; } = false;

        public bool IsBeingPushExternally { get; private set; } = false;

        private void Reset()
        {
            m_Rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (IsFrozen)
            {
                return;
            }

            float timeStep = Time.fixedDeltaTime;

            if (IsBeingPushExternally)
            {
                if (Rigidbody.velocity.sqrMagnitude <= FullSpeed * FullSpeed)
                {
                    IsBeingPushExternally = false;
                }
            }
            else
            {
                // Constriant Rigidbody.velocity
                if (Rigidbody.velocity.sqrMagnitude > FullSpeed * FullSpeed)
                {
                    Rigidbody.velocity = Rigidbody.velocity.normalized * FullSpeed;
                }
            }

            Vector2 acceleration = Vector2.zero;

            // Use input to accel the character if it's not being pushed
            if (!IsBeingPushExternally && MovementInput.sqrMagnitude > 0.001f)
            {
                var accel = MovementInput * Acceleration;
                acceleration = accel;
                m_MovementInput = Vector2.zero;
            }
            // Apply drag to slow down the character
            else
            {
                // slowing down!
                if (Rigidbody.velocity.sqrMagnitude > 0.001f)
                {
                    var accel = -Rigidbody.velocity.normalized * Deceleration;
                    acceleration = accel;
                }
                // completely stop!
                else
                {
                    Rigidbody.velocity = Vector2.zero;
                    acceleration = Vector2.zero;
                }
            }

            Rigidbody.AddForce(acceleration * Rigidbody.mass, ForceMode2D.Force);
        }

        public void ForceFullSpeedInDirection(Vector2 direction)
        {
            Rigidbody.velocity = direction.normalized * FullSpeed;
        }

        public void Freeze()
        {
            IsFrozen = true;
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        }

        public void UnFreeze()
        {
            IsFrozen = false;
            Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        }

        public void SetVelocityToZero()
        {
            Rigidbody.velocity = Vector3.zero;
        }

        /// <summary>
        /// Delegate too, called by InterfaceUser Enter
        /// </summary>
        public void SetDesiredMovementToZero()
        {
            m_DesiredMovement = Vector3.zero;
        }

        public void InputMovement(Vector2 axis)
        {
            // Update input buffer, reset every frame
            m_MovementInput = new Vector2(axis.x, axis.y);

            // This is not reset every frame
            m_DesiredMovement = new Vector2(axis.x, axis.y);
        }

        /// <summary>
        /// Make an external push that immediately change the velocity of the controller
        /// </summary>
        /// <param name="velocityChange"></param>
        public void ExternalPush(Vector2 velocityChange)
        {
            IsBeingPushExternally = true;
            Rigidbody.velocity = velocityChange;
        }

        [NaughtyAttributes.Button("Random External Push")]
        private void _randExternalPush()
        {
            var randVec = Random.insideUnitSphere * Random.Range(5, 15);
            ExternalPush(randVec);
        }
    }
}