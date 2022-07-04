using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpookySea
{
    public class ChangeVelocityControllerSettings : MonoBehaviour
    {
        [SerializeField]
        private HasVelocityControllerBase m_Controller;

        [SerializeField]
        private VelocityControllerSettings m_Settings;

        public void Execute()
        {
            m_Controller.ChangeSettings(m_Settings);
        }

        [ContextMenu("Set Settings To Controller's Default Value")]
        [NaughtyAttributes.Button("Set Settings To Controller's Default Value")]
        private void _setSettingsToControllerDefaultValue()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Set Settings To Controller's Default Value");

            m_Settings.FullSpeed = m_Controller.FullSpeed;
            m_Settings.Acceleration = m_Controller.Acceleration;
            m_Settings.Deceleration = m_Controller.Deceleration;
#endif
        }
    }
}