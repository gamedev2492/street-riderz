using UnityEngine;

namespace Supragma
{
    public class CarAnimation : MonoBehaviour
    {

        [HideInInspector]
        public Animator Animator;
        [HideInInspector]
        public GameObject AnimEnterPosition;
        [HideInInspector]
        public GameObject AnimDrivePosition;
        [HideInInspector]
        public CarMechanics CarPhysics;
        [HideInInspector]
        public AudioSource AudioSource;
        public AudioClip OpenClip;
        public AudioClip CloseClip;
        protected Rigidbody Rigidbody;

        // Use this for initialization
        void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
            AnimEnterPosition = transform.Find("CarMesh").Find("AnimEnterPosition").gameObject;
            AnimDrivePosition = transform.Find("CarMesh").Find("AnimDrivePosition").gameObject;
            CarPhysics = GetComponent<CarMechanics>();
            AudioSource = GetComponent<AudioSource>();
            Rigidbody = GetComponent<Rigidbody>();
        }

        void Update()
        {
            AudioSource.pitch = 0.8f + Rigidbody.velocity.magnitude / 20f;
        }


    }
}
