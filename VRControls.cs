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
		public float floatValue = 0.0f;
		public InputDevice device;
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
		public float floatValue = 0.0f;
		public InputDevice device;
	}

	public class FloatInputAction
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

		public FloatInputAction GetHandTrigger(Hand hand)
        {
			return hand == Hand.Hand_Left ? _leftHandTrigger : _rightHandTrigger;
        }

		public FloatInputAction GetHandGrip(Hand hand)
        {
			return hand == Hand.Hand_Left ? _leftHandGrip : _rightHandGrip;
        }

		public float ThrowForceMultiplication = 1.0f;

		new private void Awake()
		{
			base.Awake();
			GetHands();
		}

		private void OnDestroy()
		{
			foreach (Delegate d in ObjectGrabbed.GetInvocationList())
				ObjectGrabbed -= (ObjectGrabbedHandler)d;
			foreach (Delegate d in ObjectReleased.GetInvocationList())
				ObjectReleased -= (ObjectReleasedHandler)d;
			foreach (Delegate d in GripReleased.GetInvocationList())
				GripReleased -= (GripReleasedHandler)d;
			foreach (Delegate d in TriggerReleased.GetInvocationList())
				TriggerReleased -= (TriggerReleasedHandler)d;
			foreach (Delegate d in TriggerPressed.GetInvocationList())
				TriggerPressed -= (TriggerPressedHandler)d;
		}

		private void OnDisable()
		{
			foreach (Delegate d in ObjectGrabbed.GetInvocationList())
				ObjectGrabbed -= (ObjectGrabbedHandler)d;
			foreach (Delegate d in ObjectReleased.GetInvocationList())
				ObjectReleased -= (ObjectReleasedHandler)d;
			foreach (Delegate d in GripReleased.GetInvocationList())
				GripReleased -= (GripReleasedHandler)d;
			foreach (Delegate d in TriggerReleased.GetInvocationList())
				TriggerReleased -= (TriggerReleasedHandler)d;
			foreach (Delegate d in TriggerPressed.GetInvocationList())
				TriggerPressed -= (TriggerPressedHandler)d;
		}

		public void GetHands() => _hands = GetComponentsInChildren<Grabber>().ToList();

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
				CheckPinchInput(ref grip[i]);

			FloatInputAction[] trigger =
			{
				_rightHandTrigger,
				_leftHandTrigger,
			};
			for (int i = 0; i < trigger.Length; i++)
				CheckTriggerInput(ref trigger[i]);
		}

		private void CheckPinchInput(ref FloatInputAction floatInputAction)
		{
			InputDevices.GetDevicesAtXRNode(floatInputAction.Hand, Devices);
			if (Devices.Count > 0)
			{
				InputDevice controller = Devices[Devices.Count - 1];
				floatInputAction.Value = CheckPinch(controller, floatInputAction.Value, GetHand(XRNodeToHand(floatInputAction.Hand)));
			}
		}

		private void CheckTriggerInput(ref FloatInputAction floatInputAction)
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
				float tempGripRound = Mathf.Round(tempGrip);
				if (prevValue != tempGripRound && tempGripRound == 0)
				{
					{
						GripReleasedEventArgs e = new GripReleasedEventArgs
						{
							grabber = grabber,
							value = tempGripRound,
							floatValue = tempGripRound,
							device = controller
						};
						OnGripReleased(this, e);
					}

					_ = controller.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity);
					_ = controller.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out Vector3 angularVelocity);
					grabber.StartRelease(velocity * ThrowForceMultiplication, angularVelocity * ThrowForceMultiplication);
					return tempGripRound;
				}
				if (tempGripRound == 0)
				{
					grabber.ForceRelease();
					return tempGripRound;
				}
				else if (tempGripRound > 0.01)
					grabber.StartGrab();
				return tempGripRound;
			}
			return 0.0f;
		}

		private float CheckTrigger(InputDevice controller, float prevValue, Grabber grabber)
		{
			if (Devices.Count > 0)
			{
				controller.TryGetFeatureValue(CommonUsages.trigger, out float tempTrigger);
				float tempTriggerRound = Mathf.Round(tempTrigger);
				if (prevValue != tempTriggerRound && tempTriggerRound == 0)
				{
					{
						TriggerReleasedEventArgs e = new TriggerReleasedEventArgs
						{
							grabber = grabber,
							value = tempTriggerRound,
						};
						OnTriggerReleased(this, e);
					}

					return tempTriggerRound;
				}
				{
					TriggerPressedEventArgs e = new TriggerPressedEventArgs
					{
						grabber = grabber,
						value = tempTriggerRound,
						floatValue = tempTrigger,
						device = controller
					};
					OnTriggerPressed(this, e);
				}

				return tempTriggerRound;
			}
			return 0.0f;
		}
	}
}