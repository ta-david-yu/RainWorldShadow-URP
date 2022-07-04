using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpookySea
{
    public class DiverPlayerInput : MonoBehaviour
    {
        [Header("Reference")]

        [SerializeField]
        [Tooltip("For mouse input")]
        private Camera m_Camera;

        [SerializeField]
        private MarineMovementController m_Controller;

        [SerializeField]
        private Transform m_RotateCenter;

        [Header("Settings")]

        [SerializeField]
        private bool m_IsEnabled = true;

        public bool IsEnabled
        {
            get { return m_IsEnabled; }
            set
            {
                m_IsEnabled = value;
            }
        }

        public bool UsingMouseKeyboard { get; private set; } = true;
        public Vector2 AimAxis { get; private set; } = Vector2.zero;


        // Update is called once per frame
        void Update()
        {
            if (!IsEnabled)
            {
                m_Controller.InputMovement(new Vector2(0, 0));
                return;
            }

            if (UsingMouseKeyboard)
            {
                var mouseWorldPos = m_Camera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mouseWorldPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
                Vector2 center = m_RotateCenter.position;

                float angle = Vector2.SignedAngle(Vector2.right, mouseWorldPos2D - center);

                // For animation purpose, look at DiverFaceAnimationController
                AimAxis = mouseWorldPos2D - center;
                if (AimAxis.sqrMagnitude > 1)
                {
                    AimAxis = AimAxis.normalized;
                }
            }

            var moveHorizontal = Input.GetAxis("Horizontal");
            if (Mathf.Abs(moveHorizontal) > 0.3f)
                moveHorizontal = Mathf.Sign(moveHorizontal);
            else
                moveHorizontal = 0;

            var moveVertical = Input.GetAxis("Vertical");
            if (Mathf.Abs(moveVertical) > 0.3f)
                moveVertical = Mathf.Sign(moveVertical);
            else
                moveVertical = 0;

            m_Controller.InputMovement(new Vector2(moveHorizontal, moveVertical));
        }
    }
}