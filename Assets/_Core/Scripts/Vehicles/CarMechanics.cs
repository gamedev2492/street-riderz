using UnityEngine;
using System;

namespace Supragma
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarMechanics : MonoBehaviour{

        [HideInInspector]
        public InputStr input;
        public struct InputStr
        {
            public float forward;
            public float steer;
        }

        protected Rigidbody rigidBody;
        public Vector3 centerOfMass;

        [HideInInspector]
        public CarState state;

        public WheelInfo[] wheels;

        public float motorPower = 5000f;
        public float steerAngle = 35f;

        [Range(0, 1)]
        public float keepGrip = 1f;
        public float grip = 5f;

        // Use this for initialization
        void Awake () {
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.centerOfMass = centerOfMass;
            OnValidate();
        }

        private void Update()
        {
            input.forward = Input.GetAxis("Vertical");
            input.steer = Input.GetAxis("Horizontal");
        }

        void FixedUpdate()
        {
            for(int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i].motor)
                    wheels[i].wheelCollider.motorTorque = input.forward * motorPower;
                if (wheels[i].steer)
                    wheels[i].wheelCollider.steerAngle = input.steer * steerAngle;

                wheels[i].rotation += wheels[i].wheelCollider.rpm / 60 * 360 * Time.fixedDeltaTime;
                wheels[i].meshRenderer.localRotation = wheels[i].meshRenderer.parent.localRotation * Quaternion.Euler(wheels[i].rotation, -wheels[i].wheelCollider.steerAngle, 0);

            }

            rigidBody.AddForceAtPosition(transform.up * rigidBody.velocity.magnitude * -0.1f * grip, transform.position + transform.rotation * centerOfMass);

            AntiRoll();
        }

        void AntiRoll()
        {

        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + centerOfMass, .1f);
            Gizmos.DrawWireSphere(transform.position + centerOfMass, .11f);
        }

        void OnValidate()
        {
            Debug.Log("Validate");
            for (int i = 0; i < wheels.Length; i++)
            {
                //settings
                var ffriction = wheels[i].wheelCollider.forwardFriction;
                var sfriction = wheels[i].wheelCollider.sidewaysFriction;
                ffriction.asymptoteValue = wheels[i].wheelCollider.forwardFriction.extremumValue * keepGrip * 0.998f + 0.002f;
                sfriction.extremumValue = 1f;
                ffriction.extremumSlip = 1f;
                ffriction.asymptoteSlip = 2f;
                ffriction.stiffness = grip;
                sfriction.extremumValue = 1f;
                sfriction.asymptoteValue = wheels[i].wheelCollider.sidewaysFriction.extremumValue * keepGrip * 0.998f + 0.002f;
                sfriction.extremumSlip = 0.5f;
                sfriction.asymptoteSlip = 1f;
                sfriction.stiffness = grip;
                wheels[i].wheelCollider.forwardFriction = ffriction;
                wheels[i].wheelCollider.sidewaysFriction = sfriction;
            }
        } 

        [Serializable]
        public struct WheelInfo
        {
            public WheelCollider wheelCollider;
            public Transform meshRenderer;
            public bool steer;
            public bool motor;
            [HideInInspector]
            public float rotation;
        }

        [Serializable]
        public enum CarState
        {
            FREE = 0,
            OCCUPIED = 1
        }

    }
}
