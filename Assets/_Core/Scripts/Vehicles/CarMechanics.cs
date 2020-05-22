using UnityEngine;
using System;
using System.Collections;

namespace Supragma
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarMechanics : MonoBehaviour{

        [HideInInspector]
        public PlayerInput input; //handles player input
        public struct PlayerInput
        {
            public float gas;
            public float steer;
        }

        protected Rigidbody rigidBody; // car rigidbody
        public Vector3 centerOfMass; // car center of mass

        [Serializable]
        public struct Wheel
        {
            public WheelCollider wheelCollider;
            public Transform wheelTransform;
            public bool useSteer;
            public bool useMotor;
            [HideInInspector]
            public float rotation;
        }

        public Wheel[] wheels;

        public float enginePower = 5000f;
        public float maxSteeringAngle = 35f;

        bool isControlled;

        //Gears
        int totalGears = 4;
        int currentGear = 1;
        float[] maxSpeedForGear;
        float[] targetSpeedForGear;

        //Anti roll
        float antiRollFrontHorizontal;
        float antiRollRearHorizontal;
        float antiRollVertical;

        //speed
        float maxSpeed = 100;

        float steerSensitivity = 10;
        // Use this for initialization
        void Awake () {
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.centerOfMass = centerOfMass;
            OnValidate();
        }

        private void Update()
        {
            input.gas = Mathf.Lerp(input.gas, Input.GetAxis("Vertical"), 10 * Time.deltaTime);
            input.steer = Mathf.Lerp(input.steer, Input.GetAxis("Horizontal"), steerSensitivity * Time.deltaTime);
            //brakeInput = Mathf.Clamp01(-Input.GetAxis(verticalInput));
            //handbrakeInput = Input.GetKey(.handbrakeKB) ? 1f : 0f;
            //steerInput = Input.GetAxis(.horizontalInput);
            //boostInput = Input.GetKey(.boostKB) ? 2.5f : 1f;
        }

        void FixedUpdate()
        {
            for(int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i].useMotor)
                    wheels[i].wheelCollider.motorTorque = input.gas * enginePower;
                if (wheels[i].useSteer)
                    wheels[i].wheelCollider.steerAngle = input.steer * maxSteeringAngle;

                wheels[i].rotation += wheels[i].wheelCollider.rpm / 60 * 360 * Time.fixedDeltaTime;
                wheels[i].wheelTransform.localRotation = wheels[i].wheelTransform.parent.localRotation * Quaternion.Euler(wheels[i].rotation, wheels[i].wheelCollider.steerAngle, 0);

            }

            //rigidBody.AddForceAtPosition(transform.up * rigidBody.velocity.magnitude * -0.1f * grip, transform.position + transform.rotation * centerOfMass);

            SetTorque();
            //Engine();

            if (isControlled)
            {
                //Gear();
                //Clutch();
            }

            //Drift();


            AntiRollBars();
        }

        //todo anti roll when car is out of control
        void AntiRollBars()
        {

            //Horizontal anti roll

            float travelFL = 1f;
            float travelFR = 1f;

            //if front left wheel is grounded
            bool groundedFL = wheels[0].wheelCollider.GetGroundHit(out WheelHit wheelHit);
            if (groundedFL)
                travelFL = (-wheels[0].wheelTransform.InverseTransformPoint(wheelHit.point).y - wheels[0].wheelCollider.radius) / wheels[0].wheelCollider.suspensionDistance;

            //if front right wheel is grounded
            bool groundedFR = wheels[1].wheelCollider.GetGroundHit(out wheelHit);
            if (groundedFR)
                travelFR = (-wheels[1].wheelTransform.InverseTransformPoint(wheelHit.point).y - wheels[1].wheelCollider.radius) / wheels[1].wheelCollider.suspensionDistance;

            float antiRollFrontHorizontal = (travelFL - travelFR) * this.antiRollFrontHorizontal;

            //anti roll force
            if (groundedFL)
                rigidBody.AddForceAtPosition(wheels[0].wheelTransform.up * -antiRollFrontHorizontal, wheels[0].wheelTransform.position);
            if (groundedFR)
                rigidBody.AddForceAtPosition(wheels[1].wheelTransform.up * antiRollFrontHorizontal, wheels[1].wheelTransform.position);


            float travelRL = 1f;
            float travelRR = 1f;

            //if rear left wheel is grounded
            bool groundedRL = wheels[2].wheelCollider.GetGroundHit(out wheelHit);
            if (groundedRL)
                travelRL = (-wheels[2].wheelTransform.InverseTransformPoint(wheelHit.point).y - wheels[2].wheelCollider.radius) / wheels[2].wheelCollider.suspensionDistance;

            //if rear right wheel is grounded
            bool groundedRR = wheels[3].wheelCollider.GetGroundHit(out wheelHit);
            if (groundedRR)
                travelRR = (-wheels[3].wheelTransform.InverseTransformPoint(wheelHit.point).y - wheels[3].wheelCollider.radius) / wheels[3].wheelCollider.suspensionDistance;

            float antiRollRearHorizontal = (travelRL - travelRR) * this.antiRollRearHorizontal;

            //anti roll force
            if (groundedRL)
                rigidBody.AddForceAtPosition(wheels[2].wheelTransform.up * -antiRollRearHorizontal, wheels[2].wheelTransform.position);
            if (groundedRR)
                rigidBody.AddForceAtPosition(wheels[3].wheelTransform.up * antiRollRearHorizontal, wheels[3].wheelTransform.position);


            //Vertical anti roll

            float antiRollFrontVertical = (travelFL - travelRL) * antiRollVertical;

            if (groundedFL)
                rigidBody.AddForceAtPosition(wheels[0].wheelTransform.up * -antiRollFrontVertical, wheels[0].wheelTransform.position);
            if (groundedRL)
                rigidBody.AddForceAtPosition(wheels[2].wheelTransform.up * antiRollFrontVertical, wheels[2].wheelTransform.position);

            float antiRollRearVertical = (travelFR - travelRR) * antiRollVertical;

            if (groundedFR)
                rigidBody.AddForceAtPosition(wheels[1].wheelTransform.up * -antiRollRearVertical, wheels[1].wheelTransform.position);
            if (groundedRR)
                rigidBody.AddForceAtPosition(wheels[3].wheelTransform.up * antiRollRearVertical, wheels[3].wheelTransform.position);

        }

        public void StartEngine()
        {

            StartCoroutine(StartEngineDelayed());

        }

        public void StartEngine(bool instantStart)
        {

            if (instantStart)
            {

                //fuelInput = 1f;
                //engineRunning = true;

            }
            else
            {

                StartCoroutine(StartEngineDelayed());

            }

        }

        public IEnumerator StartEngineDelayed()
        {

            //engineRunning = false;
            
            //if (engineStartSound.isPlaying)
            //    engineStartSound.Play();
            yield return new WaitForSeconds(1f);
            //engineRunning = true;
            //fuelInput = 1f;
            yield return new WaitForSeconds(1f);

        }

        public void KillEngine()
        {

            //fuelInput = 0f;
            //engineRunning = false;

        }

        public void KillOrStartEngine()
        {

            //if (engineRunning)
            //    KillEngine();
            //else
            //    StartEngine();

        }

        public void SetTorque()
        {

            if (maxSpeedForGear == null || maxSpeedForGear.Length != totalGears)
                maxSpeedForGear = new float[totalGears];

            if (targetSpeedForGear == null || targetSpeedForGear.Length != totalGears - 1)
                targetSpeedForGear = new float[totalGears - 1];

            for (int j = 0; j < totalGears; j++)
                maxSpeedForGear[j] = Mathf.Lerp(0f, maxSpeed * 1.1f, (float)(j + 1) / (float)(totalGears));

            for (int k = 0; k < totalGears - 1; k++)
                targetSpeedForGear[k] = Mathf.Lerp(0, maxSpeed * Mathf.Lerp(0f, 1f, 0.3f), ((float)(k + 1) / (float)(totalGears)));

            

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
                WheelFrictionCurve fFriction = wheels[i].wheelCollider.forwardFriction;
                WheelFrictionCurve sFriction = wheels[i].wheelCollider.sidewaysFriction;
                fFriction.asymptoteValue = wheels[i].wheelCollider.forwardFriction.extremumValue * 0.998f + 0.002f;

                sFriction.extremumValue = 1f;
                fFriction.extremumSlip = 1f;
                fFriction.asymptoteSlip = 2f;
                fFriction.stiffness = 5;
                sFriction.extremumValue = 1f;
                sFriction.asymptoteValue = wheels[i].wheelCollider.sidewaysFriction.extremumValue * 0.998f + 0.002f;
                sFriction.extremumSlip = 0.5f;
                sFriction.asymptoteSlip = 1f;
                sFriction.stiffness = 5;

                wheels[i].wheelCollider.forwardFriction = fFriction;
                wheels[i].wheelCollider.sidewaysFriction = sFriction;
            }
        } 
    }
}
