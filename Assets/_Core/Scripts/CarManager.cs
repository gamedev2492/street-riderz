using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supragma
{

	//This script will handle car changing etc operations
	public class CarManager : MonoBehaviour
	{
		public RCC_CarControllerV3[] availableVehicles;

		internal int selectedVehicleIndex = 0;
		internal int selectedBehaviorIndex = 0; // An integer index value used for setting behavior mode.

		private void Start()
		{
			AudioListener.volume = 0f;
		}

		// An integer index value used for spawning a new vehicle.
		public void SelectVehicle(int index)
		{

			selectedVehicleIndex = index;

		}

		public void Spawn()
		{

			// Last known position and rotation of last active vehicle.
			Vector3 lastKnownPos = new Vector3();
			Quaternion lastKnownRot = new Quaternion();

			// Checking if there is a player vehicle on the scene.
			if (RCC_SceneManager.Instance.activePlayerVehicle)
			{

				lastKnownPos = RCC_SceneManager.Instance.activePlayerVehicle.transform.position;
				lastKnownRot = RCC_SceneManager.Instance.activePlayerVehicle.transform.rotation;

			}

			// If last known position and rotation is not assigned, camera's position and rotation will be used.
			if (lastKnownPos == Vector3.zero)
			{

				if (RCC_SceneManager.Instance.activePlayerCamera)
				{

					lastKnownPos = RCC_SceneManager.Instance.activePlayerCamera.transform.position;
					lastKnownRot = RCC_SceneManager.Instance.activePlayerCamera.transform.rotation;

				}

			}

			// We don't need X and Z rotation angle. Just Y.
			lastKnownRot.x = 0f;
			lastKnownRot.z = 0f;

			RCC_CarControllerV3 lastVehicle = RCC_SceneManager.Instance.activePlayerVehicle;

			// If we have controllable vehicle by player on scene, destroy it.
			if (lastVehicle)
				Destroy(lastVehicle.gameObject);

			// Here we are creating our new vehicle.
			RCC.SpawnRCC(availableVehicles[selectedVehicleIndex], lastKnownPos, lastKnownRot, true, true, true);

		}

        public void SetBehavior(int index)
		{

			selectedBehaviorIndex = index;

		}

		// Here we are setting new selected behavior to corresponding one.
		public void InitBehavior()
		{

			RCC.SetBehavior(selectedBehaviorIndex);

		}
		//	Sets the main controller type.
		public void SetController(int index)
		{

			RCC.SetController(index);

		}

		// Sets the mobile controller type.
		public void SetMobileController(int index)
		{

			switch (index)
			{

				case 0:
					RCC.SetMobileController(RCC_Settings.MobileController.TouchScreen);
					break;
				case 1:
					RCC.SetMobileController(RCC_Settings.MobileController.Gyro);
					break;
				case 2:
					RCC.SetMobileController(RCC_Settings.MobileController.SteeringWheel);
					break;
				case 3:
					RCC.SetMobileController(RCC_Settings.MobileController.Joystick);
					break;

			}
		}
	}
}
