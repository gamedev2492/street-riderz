using UnityEngine;

namespace Supragma
{
    public class CarCam : MonoBehaviour
    {
        Transform followObject;
        public Transform target;
        public Rigidbody carRigidBody;

        [Tooltip("If car speed is below this value, then the camera will default to looking forwards.")]
        public float rotationThreshold = 1f;

        [Tooltip("How closely the camera follows the car's position. The lower the value, the more the camera will lag behind.")]
        public float cameraStickiness = 10.0f;

        [Tooltip("How closely the camera matches the car's velocity vector. The lower the value, the smoother the camera rotations, but too much results in not being able to see where you're going.")]
        public float cameraRotationSpeed = 5.0f;

        void Awake()
        {
            followObject = Camera.main.transform;
        }

        void Start()
        {
            // Detach the camera so that it can move freely on its own.
            followObject.parent = null;
        }

        void Update()
        {
            Quaternion look;

            // Moves the camera to match the car's position.
            followObject.position = Vector3.Lerp(followObject.position, target.position, cameraStickiness * Time.deltaTime);

            // If the car isn't moving, default to looking forwards. Prevents camera from freaking out with a zero velocity getting put into a Quaternion.LookRotation
            if (carRigidBody.velocity.magnitude < rotationThreshold)
                look = Quaternion.LookRotation(target.forward);
            else
                look = Quaternion.LookRotation(carRigidBody.velocity.normalized);

            // Rotate the camera towards the velocity vector.
            look = Quaternion.Slerp(followObject.rotation, look, cameraRotationSpeed * Time.deltaTime);
            followObject.rotation = look;
        }
    }
}