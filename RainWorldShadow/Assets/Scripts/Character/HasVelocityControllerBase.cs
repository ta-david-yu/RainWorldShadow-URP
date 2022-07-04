using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpookySea
{
    [System.Serializable]
    public class VelocityControllerSettings
    {
        public float FullSpeed;
        public float Acceleration;
        public float Deceleration;
    }

    public abstract class HasVelocityControllerBase : MonoBehaviour
    {
        public abstract float FullSpeed { get; protected set; }
        public abstract float Acceleration { get; protected set; }
        public abstract float Deceleration { get; protected set; }
        public abstract Vector2 Velocity { get; }
    
        public void ChangeSettings(VelocityControllerSettings settings)
        {
            FullSpeed = settings.FullSpeed;
            Acceleration = settings.Acceleration;
            Deceleration = settings.Deceleration;
        }
    }
}