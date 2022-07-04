using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpookySea
{
    public class DiverAnimationController : MonoBehaviour
    {
        [SerializeField]
        private DiverPlayerInput m_DiverPlayerInput;

        [SerializeField]
        private Animator m_FaceAnimator;

        [Space]

        [SerializeField]
        private Transform m_AppearanceTransform;

        [SerializeField]
        private Transform m_RightHandTransform;

        [SerializeField]
        private Transform m_RightHandTransformOrigin;

        [SerializeField]
        private Transform m_LeftHandTransform;

        [SerializeField]
        private Transform m_LeftHandTransformOrigin;

        [Space]

        [SerializeField]
        private float m_HandScaleDuration = 0.25f;
        
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("m_HandShowCurve")]
        private AnimationCurve m_HandShowScaleCurve;
        
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("m_HandHideCurve")]
        private AnimationCurve m_HandHideScaleCurve;
        
        [SerializeField]
        private AnimationCurve m_HandHidePositionCurve;

        [Space]
        [SerializeField]
        private float m_HandToTargetHalfLifeTime = 0.12f;

        private Vector2 m_CurrInputAimAxis;

        private Tweener m_HandScaleTweener;
        private Tweener m_HandPositionTweener;

        private Transform m_CurrRightHandAnchor;
        private Transform m_CurrLeftHandAnchor;

        private int m_AimAxisXId;
        private int m_AimAxisYId;

        private void Awake()
        {
            m_AimAxisXId = Animator.StringToHash("AimAxisX");
            m_AimAxisYId = Animator.StringToHash("AimAxisY");
        }

        private void Reset()
        {
            m_DiverPlayerInput = GetComponent<DiverPlayerInput>();
        }

        private void FixedUpdate()
        {
            float timeStep = Time.fixedDeltaTime;

            if (m_CurrLeftHandAnchor != null && m_CurrRightHandAnchor != null)
            {
                // Lerp the hands to the anchors
                m_LeftHandTransform.position = Vector3.Lerp(
                    m_LeftHandTransform.position, 
                    m_CurrLeftHandAnchor.position, 
                    1 - Mathf.Pow(0.5f, timeStep / m_HandToTargetHalfLifeTime));

                m_RightHandTransform.position = Vector3.Lerp(
                    m_RightHandTransform.position,
                    m_CurrRightHandAnchor.position,
                    1 - Mathf.Pow(0.5f, timeStep / m_HandToTargetHalfLifeTime));

                // Look at blocking target
                Vector2 lookAt = new Vector2(
                    m_CurrRightHandAnchor.position.x + m_CurrLeftHandAnchor.position.x, 
                    m_CurrRightHandAnchor.position.y + m_CurrLeftHandAnchor.position.y) / 2 - 
                    new Vector2(transform.position.x, transform.position.y);

                Vector2 inputAimAxis = lookAt.normalized;
                inputAimAxis = inputAimAxis.Rotate(-m_AppearanceTransform.localRotation.eulerAngles.z);

                m_CurrInputAimAxis = Vector2.LerpUnclamped(m_CurrInputAimAxis, inputAimAxis, 1 - Mathf.Pow(0.5f, timeStep / 0.05f));

                m_FaceAnimator.SetFloat(m_AimAxisXId, m_CurrInputAimAxis.x);
                m_FaceAnimator.SetFloat(m_AimAxisYId, m_CurrInputAimAxis.y);
            }
            else
            {
                Vector2 inputAimAxis = m_DiverPlayerInput.AimAxis;
                inputAimAxis = inputAimAxis.Rotate(-m_AppearanceTransform.localRotation.eulerAngles.z);

                m_CurrInputAimAxis = Vector2.LerpUnclamped(m_CurrInputAimAxis, inputAimAxis, 1 - Mathf.Pow(0.5f, timeStep / 0.05f));

                m_FaceAnimator.SetFloat(m_AimAxisXId, m_CurrInputAimAxis.x);
                m_FaceAnimator.SetFloat(m_AimAxisYId, m_CurrInputAimAxis.y);
            }
        }
    }
}