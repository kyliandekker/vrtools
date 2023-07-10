using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using VRTools.Utils;

namespace VRTools.Interaction
{
	public class FloatInputAction
	{
		public XRNode Hand;
		public float Value;
		public FloatInputAction(XRNode hand) => Hand = hand;
	}

	public class VRControls : UnitySingleton<VRControls>
	{
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

        public virtual void Grab(Grabber grabber, GrabbableObject grabbed)
        {

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

		public virtual void Release(Grabber grabber, GrabbableObject grabbed)
        {
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

		public virtual void OnGripReleased(Grabber grabber, float value, float floatValue, InputDevice controller)
        {

		}

		public virtual void OnTriggerReleased(Grabber grabber, float tempTriggerRound, InputDevice controller)
		{

		}

		public virtual void OnTriggerPressed(Grabber grabber, float value, float floatValue, InputDevice controller)
		{

		}

		private float CheckPinch(InputDevice controller, float prevValue, Grabber grabber)
		{
			if (Devices.Count > 0)
			{
				_ = controller.TryGetFeatureValue(CommonUsages.grip, out float tempGrip);
				float tempGripRound = Mathf.Round(tempGrip);
				if (prevValue != tempGripRound && tempGripRound == 0)
				{
					OnGripReleased(grabber, tempGripRound, tempGripRound, controller);

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
					OnTriggerReleased(grabber, tempTriggerRound, controller);

					return tempTriggerRound;
				}
				OnTriggerPressed(grabber, tempTriggerRound, tempTrigger, controller);

				return tempTriggerRound;
			}
			return 0.0f;
		}
    }
}