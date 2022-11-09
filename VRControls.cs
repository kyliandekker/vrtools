using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using VRTools.Utils;

namespace VRTools.Interaction
{
	public class GripReleasedEventArgs : EventArgs
	{
		public Grabber grabber = null;
		public float value = 0.0f;
	}

	public class TriggerReleasedEventArgs : EventArgs
	{
		public Grabber grabber = null;
		public float value = 0.0f;
	}

	public class TriggerPressedEventArgs : EventArgs
	{
		public Grabber grabber = null;
		public float value = 0.0f;
	}

	class FloatInputAction
    {
		public XRNode Hand;
		public float Value;
		public FloatInputAction(XRNode hand) => Hand = hand;
    }

	public class VRControls : UnitySingleton<VRControls>
	{
		/* 
		* Name: OnObjectGrabbed.
		* Description: Called when a player picks up an object.
		*/
		public delegate void ObjectGrabbedHandler(object sender, GrabbedEventArgs e);
		public event ObjectGrabbedHandler ObjectGrabbed = delegate { };

		public void OnObjectGrabbed(object sender, GrabbedEventArgs e)
		{
			ObjectGrabbed.Invoke(sender, e);
		}

		/* 
		* Name: OnObjectReleased.
		* Description: Called when a player releases a picked up an object.
		*/
		public delegate void ObjectReleasedHandler(object sender, ReleasedEventArgs e);
		public event ObjectReleasedHandler ObjectReleased = delegate { };

		public void OnObjectReleased(object sender, ReleasedEventArgs e)
		{
			ObjectReleased.Invoke(sender, e);
		}

		/* 
		* Name: OnGripReleased.
		* Description: When the player presses the pick up button.
		*/
		public delegate void GripReleasedHandler(object sender, GripReleasedEventArgs e);
		public event GripReleasedHandler GripReleased = delegate { };

		public void OnGripReleased(object sender, GripReleasedEventArgs e)
		{
			GripReleased.Invoke(sender, e);
		}

		/* 
		* Name: OnTriggerReleased.
		* Description: When the player releases the pick up button.
		*/
		public delegate void TriggerReleasedHandler(object sender, TriggerReleasedEventArgs e);
		public event TriggerReleasedHandler TriggerReleased = delegate { };

		public void OnTriggerReleased(object sender, TriggerReleasedEventArgs e)
		{
			TriggerReleased.Invoke(sender, e);
		}

		/* 
		* Name: OnTriggerPressed.
		* Description: When the player releases the pick up button.
		*/
		public delegate void TriggerPressedHandler(object sender, TriggerPressedEventArgs e);
		public event TriggerPressedHandler TriggerPressed = delegate { };

		public void OnTriggerPressed(object sender, TriggerPressedEventArgs e)
		{
			TriggerPressed.Invoke(sender, e);
		}

		private List<Grabber> _hands = new List<Grabber>();
		private Hand XRNodeToHand(XRNode node) => node == XRNode.RightHand ? Hand.Hand_Right : Hand.Hand_Left;
		public Grabber GetHand(Hand hand) => _hands.Find(x => x.HandType == hand);

		private List<InputDevice> Devices = new List<InputDevice>();

		private FloatInputAction
			_rightHandGrip = new FloatInputAction(XRNode.RightHand), _leftHandGrip = new FloatInputAction(XRNode.LeftHand),
			_rightHandTrigger = new FloatInputAction(XRNode.RightHand), _leftHandTrigger = new FloatInputAction(XRNode.LeftHand);

		public float ThrowForceMultiplication = 1.0f;

		new private void Awake()
		{
			base.Awake();
			_hands = GetComponentsInChildren<Grabber>().ToList();
		}

		private void Update()
		{
			InputDevices.GetDevices(Devices);
			CheckInput();
		}

        private void CheckInput()
		{
			FloatInputAction[] grip =
			{
				_rightHandGrip,
				_leftHandGrip,
			};
            for (int i = 0; i < grip.Length; i++)
				CheckPinchInput(grip[i]);

			FloatInputAction[] trigger =
			{
				_rightHandTrigger,
				_leftHandTrigger,
			};
            for (int i = 0; i < trigger.Length; i++)
				CheckTriggerInput(trigger[i]);
		}

        private void CheckPinchInput(FloatInputAction floatInputAction)
		{
			InputDevices.GetDevicesAtXRNode(floatInputAction.Hand, Devices);
			if (Devices.Count > 0)
            {
				InputDevice controller = Devices[Devices.Count - 1];
				floatInputAction.Value = CheckPinch(controller, floatInputAction.Value, GetHand(XRNodeToHand(floatInputAction.Hand)));
			}
		}

        private void CheckTriggerInput(FloatInputAction floatInputAction)
		{
			InputDevices.GetDevicesAtXRNode(floatInputAction.Hand, Devices);
			if (Devices.Count > 0)
            {
				InputDevice controller = Devices[Devices.Count - 1];
				floatInputAction.Value = CheckTrigger(controller, floatInputAction.Value, GetHand(XRNodeToHand(floatInputAction.Hand)));
			}
		}

		private float CheckPinch(InputDevice controller, float prevValue, Grabber grabber)
		{
			if (Devices.Count > 0)
			{
				_ = controller.TryGetFeatureValue(CommonUsages.grip, out float tempGrip);
				if (prevValue != tempGrip && tempGrip == 0)
				{
					{
						GripReleasedEventArgs e = new GripReleasedEventArgs
						{
							grabber = grabber,
							value = tempGrip,
						};
						OnGripReleased(this, e);
					}

					_ = controller.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity);
					_ = controller.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out Vector3 angularVelocity);
					grabber.StartRelease(velocity * ThrowForceMultiplication, angularVelocity * ThrowForceMultiplication);
					return tempGrip;
				}
				if (tempGrip == 0)
                {
					grabber.ForceRelease();
					return tempGrip;
				}
				else if (tempGrip > 0.01)
					grabber.StartGrab();
				return tempGrip;
			}
			return 0.0f;
		}

		private float CheckTrigger(InputDevice controller, float prevValue, Grabber grabber)
		{
			if (Devices.Count > 0)
			{
				controller.TryGetFeatureValue(CommonUsages.trigger, out float tempTrigger);
				if (prevValue != tempTrigger && tempTrigger == 0)
				{
					{
						TriggerReleasedEventArgs e = new TriggerReleasedEventArgs
						{
							grabber = grabber,
							value = tempTrigger,
						};
						OnTriggerReleased(this, e);
					}

					return tempTrigger;
				}
                {
					TriggerPressedEventArgs e = new TriggerPressedEventArgs
					{
						grabber = grabber,
						value = tempTrigger,
					};
					OnTriggerPressed(this, e);
				}

				return tempTrigger;
			}
			return 0.0f;
		}
	}
}