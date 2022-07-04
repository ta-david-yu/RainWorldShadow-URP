using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DampedTail2D : MonoBehaviour
{
    const float k_FixedDt = 0.01667f; // 60Hz simulation step
    const float k_DampFactor = 40f;

    [System.Serializable]
    class ChildDampedBone
    {
        public Transform Transform;

        /// <summary> The initial bone transform local position. </summary>
        [NaughtyAttributes.ReadOnly]
        public Vector3 InitialLocalPosition;

        /// <summary> The initial bone transform local rotation. </summary>
        [NaughtyAttributes.ReadOnly]
        public Quaternion InitialLocalRotation;

        [NaughtyAttributes.ReadOnly]
        public Vector3 AimParentAxis;

        public bool ApplySineWave = false;

        [NaughtyAttributes.ShowIf(nameof(ApplySineWave))]
        [Tooltip("Used When Apply Sine Wave is true: for instance, if angle is 15, the wave goes from 0 -> 15 -> 0 -> -15 -> 0 ...")]
        [Range(0, 90)]
        public float SineWaveAngleRange = 15;

        [NaughtyAttributes.ShowIf(nameof(ApplySineWave))]
        [Tooltip("Used When Apply Sine Wave is true")]
        public float SineWaveSpeed = 20;

        public bool RandomizeInitialSineWaveAngle = true;

        /// <summary> Previous frame bone transform world position. </summary>
        [NaughtyAttributes.ReadOnly]
        public Vector3 PrevWorldPosition;

        /// <summary> Previous frame bone transform world rotation. </summary>
        [NaughtyAttributes.ReadOnly]
        public Quaternion PrevWorldRotation;

        [NaughtyAttributes.ReadOnly]
        public float PrevSineWaveAngle = 0;
    }

    [Header("Reference")]

    [SerializeField]
    private Transform m_TailEnd;

    [SerializeField]
    private Transform m_TailHead;

    [SerializeField]
    [UnityEngine.Serialization.FormerlySerializedAs("m_HierarchyTransforms")]
    private List<ChildDampedBone> m_HierarchyBones = new List<ChildDampedBone>();

    [Header("Settings")]

    [SerializeField]
    private bool m_ResetBonesTransformRecordOnAwake = true;

    [SerializeField]
    private bool m_ResetBonesTransformRecordOnEnable = false;

    [SerializeField]
    private bool m_InFixedUpdate = true;

    [SerializeField]
    private bool m_AimAtParentTransform = true;

    [SerializeField]
    [Range(0, 1)]
    private float m_Weight = 1;

    [SerializeField]
    [Range(0, 1)]
    private float m_PositionDamp = 0.5f;
    [SerializeField]
    [Range(0, 1)]
    private float m_RotationDamp = 0.5f;

    [SerializeField]
    [Range(2, 9)]
    private int m_MaxNumOfTransformNodes = 9;

    private void Awake()
    {
        if (m_ResetBonesTransformRecordOnAwake)
        {
            ResetBonesPrevWorldTransform();
        }
    }

    private void OnEnable()
    {
        if (m_ResetBonesTransformRecordOnEnable)
        {
            ResetBonesPrevWorldTransform();
        }
    }

    private void FixedUpdate()
    {
        if (m_InFixedUpdate)
            _update(Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        if (!m_InFixedUpdate)
            _update(Time.deltaTime);
    }

    private void _update(float timeStep)
    {
        if (m_Weight <= 0)
        {
            return;
        }

        while (timeStep > 0.0f)
        {
            float dt = Mathf.Min(k_FixedDt, timeStep);
            float factoredDt = k_DampFactor * dt;

            for (int i = 0; i < m_HierarchyBones.Count; i++)
            {
                // the constrainted bone and transform (the moving target)
                var drivenBone = m_HierarchyBones[i];

                // get the parent transform
                Transform sourceTransform = (i == 0) ? m_TailHead : m_HierarchyBones[i - 1].Transform;

                // get target global position and rotation
                var targetPos = sourceTransform.TransformPoint(drivenBone.InitialLocalPosition);
                var targetRot = sourceTransform.rotation * drivenBone.InitialLocalRotation;

                targetPos = Vector3.Lerp(drivenBone.Transform.position, targetPos, m_Weight);
                targetRot = Quaternion.Lerp(drivenBone.Transform.rotation, targetRot, m_Weight);

                // get damp factor value
                var dampPosWeight = (1f - m_PositionDamp) * (1f - m_PositionDamp);
                var dampRotWeight = (1f - m_RotationDamp) * (1f - m_RotationDamp);

                // calculate bone's next frame position and rotation (lerping)
                drivenBone.PrevWorldPosition += (targetPos - drivenBone.PrevWorldPosition) * dampPosWeight * factoredDt;
                drivenBone.PrevWorldRotation *= Quaternion.Lerp(
                    Quaternion.identity,
                    Quaternion.Inverse(drivenBone.PrevWorldRotation) * targetRot,
                    dampRotWeight * factoredDt);

                // do aim parent adjustment when m_AimAtParentTransform is enabled + the length of AimParentAxis > 0
                if (m_AimAtParentTransform && drivenBone.AimParentAxis.sqrMagnitude > 0.0f)
                {
                    var fromDir = drivenBone.PrevWorldRotation * drivenBone.AimParentAxis;
                    var toDir = sourceTransform.position - drivenBone.PrevWorldPosition;
                    drivenBone.PrevWorldRotation =
                        Quaternion.AngleAxis(Vector3.Angle(fromDir, toDir), Vector3.Cross(fromDir, toDir).normalized) * drivenBone.PrevWorldRotation;
                }

                // update sine wave rotation offset if enabled
                if (drivenBone.ApplySineWave)
                {
                    drivenBone.PrevSineWaveAngle += drivenBone.SineWaveSpeed * dt;
                }
            }

            timeStep -= k_FixedDt;
        }

        // apply the last PrevWorldPosition and PrevWorldRotation to the bone transforms
        for (int i = 0; i < m_HierarchyBones.Count; i++)
        {
            var drivenBone = m_HierarchyBones[i];
            drivenBone.Transform.position = drivenBone.PrevWorldPosition;

            var rotation = drivenBone.PrevWorldRotation;

            if (drivenBone.ApplySineWave)
            {
                // Normalized Sine goes from 0 ~ 1
                float normalizedSine = (Mathf.Sin(drivenBone.PrevSineWaveAngle) + 1) / 2;
                Quaternion offsetRot = Quaternion.Euler(0, 0, Mathf.LerpUnclamped(-drivenBone.SineWaveAngleRange, drivenBone.SineWaveAngleRange, normalizedSine));
                rotation *= offsetRot;
            }

            drivenBone.Transform.rotation = rotation;
        }
    }

#if UNITY_EDITOR
    [NaughtyAttributes.Button("Setup Transforms Hierarchy")]
    private void _collectTransformsInTheHierarchy()
    {
        UnityEditor.Undo.RecordObject(this, "Collect Transforms in the Hierarchy");

        m_HierarchyBones = new List<ChildDampedBone>();
        var currNode = m_TailEnd;
        Transform prevNode = null;
        do
        {
            var newDampedBone =
                new ChildDampedBone()
                {
                    Transform = currNode,
                    InitialLocalPosition = currNode.localPosition,
                    InitialLocalRotation = currNode.localRotation,
                    PrevWorldPosition = currNode.position,
                    PrevWorldRotation = currNode.rotation,
                };

            m_HierarchyBones.Insert(0, newDampedBone);
            prevNode = currNode;
            currNode = currNode.parent;
        }
        while (
        m_HierarchyBones.Count < m_MaxNumOfTransformNodes && 
        currNode != null &&
        prevNode != this.transform);
        
        // Calculate AimParentAxis. Ignore head (index 0), cuz it doesn't have a parent
        for (int i = 1; i < m_HierarchyBones.Count; i++)
        {
            var parentBone = m_HierarchyBones[i - 1];
            var currBone = m_HierarchyBones[i];

            currBone.AimParentAxis = Quaternion.Inverse(currBone.Transform.rotation) * (parentBone.Transform.position - currBone.Transform.position).normalized;
        }

        // Move the head to m_TailHead
        m_TailHead = m_HierarchyBones[0].Transform;
        m_HierarchyBones.RemoveAt(0);

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    [NaughtyAttributes.Button("Reset Transforms Previous World Position")]
    public void ResetBonesPrevWorldTransform()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.Undo.RecordObject(this, "Reset Transforms Previous World Position");
        }
#endif
        foreach (var bone in m_HierarchyBones)
        {
            bone.PrevWorldPosition = bone.Transform.position;
            bone.PrevWorldRotation = bone.Transform.rotation;

            if (bone.ApplySineWave && bone.RandomizeInitialSineWaveAngle)
            {
                bone.PrevSineWaveAngle = Random.Range(-180, 180);
            }
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    private void OnDrawGizmos()
    {
        if (m_TailHead != null)
        {
            Gizmos.DrawWireSphere(m_TailHead.position, 0.2f);
        }

        var prevTrans = m_TailHead;
        for (int i = 0; i < m_HierarchyBones.Count; i++)
        {
            var currTrans = m_HierarchyBones[i].Transform;
            Gizmos.DrawWireSphere(currTrans.position, 0.15f);            
            Gizmos.DrawLine(currTrans.position, prevTrans.position);
            prevTrans = currTrans;
        }
    }
}
