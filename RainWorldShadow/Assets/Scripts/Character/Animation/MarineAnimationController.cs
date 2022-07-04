using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpookySea
{
    public class MarineAnimationController : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField]
        private MarineMovementController m_MovementController;

        [Space]

        [SerializeField]
        private bool m_RotateTheRoot = false;
        public bool RotateTheRoot { get { return m_RotateTheRoot; } }

        [NaughtyAttributes.HideIf(nameof(RotateTheRoot))]
        [SerializeField]
        private Transform m_AppearanceTransform;

        [Header("Settings")]
        [SerializeField]
        private float m_HeadRotationHalfLifeTime = 1.0f;

        [SerializeField]
        private bool m_ManualLookAt = false;

        private Vector2 m_ManualLookDir = Vector2.zero;

        private void Reset()
        {
            m_MovementController = GetComponent<MarineMovementController>();
        }

        private void FixedUpdate()
        {
            float timeStep = Time.fixedDeltaTime;

            if (m_ManualLookAt)
            {
                // Body rotation
                float targetBodyAngle = (m_ManualLookDir.sqrMagnitude > 0) ?
                    Vector2.SignedAngle(Vector2.right, m_ManualLookDir) - 90 :
                    0;
                Quaternion targetBodyRotation = Quaternion.Euler(0, 0, targetBodyAngle);

                if (RotateTheRoot)
                {
                    transform.localRotation =
                        Quaternion.LerpUnclamped(transform.localRotation, targetBodyRotation, 1 - Mathf.Pow(0.5f, timeStep / m_HeadRotationHalfLifeTime));
                }
                else
                {
                    m_AppearanceTransform.localRotation =
                        Quaternion.LerpUnclamped(m_AppearanceTransform.localRotation, targetBodyRotation, 1 - Mathf.Pow(0.5f, timeStep / m_HeadRotationHalfLifeTime));
                }
            }
            else
            {
                // Body rotation
                float targetBodyAngle = (m_MovementController.DesiredMovement.sqrMagnitude > 0) ?
                    Vector2.SignedAngle(Vector2.right, m_MovementController.DesiredMovement) - 90 :
                    0;
                Quaternion targetBodyRotation = Quaternion.Euler(0, 0, targetBodyAngle);

                if (RotateTheRoot)
                {
                    transform.localRotation =
                        Quaternion.LerpUnclamped(transform.localRotation, targetBodyRotation, 1 - Mathf.Pow(0.5f, timeStep / m_HeadRotationHalfLifeTime));
                }
                else
                {
                    m_AppearanceTransform.localRotation =
                        Quaternion.LerpUnclamped(m_AppearanceTransform.localRotation, targetBodyRotation, 1 - Mathf.Pow(0.5f, timeStep / m_HeadRotationHalfLifeTime));
                }
            }
        }
        
        public void SetManualLookDir(Vector2 dir)
        {
            m_ManualLookDir = dir;
        }
    }
}