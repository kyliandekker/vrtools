using System;
using System.Linq;
using UnityEngine;

namespace VRTools.Interaction
{
	public class GrabbedEventArgs : EventArgs
	{
		public Grabber grabber;
		public GrabbableObject grabbed;
	}

	public class ReleasedEventArgs : EventArgs
	{
		public Grabber grabber;
		public GrabbableObject grabbed;
	}

	[System.Serializable]
	public class HandGrabSettings
	{
		public Vector3 Position = Vector3.zero;
		public Quaternion Rotation = Quaternion.Euler(0, 0, 0);
	}

	public enum SnapSettings
    {
		SnapSettings_None,
		SnapSettings_Position,
		SnapSettings_Rotation,
		SnapSettings_Both,
    }

	[System.Serializable]
	public class SnapTransform
    {
		public Vector3 Position = Vector3.zero;
		public Vector3 Rotation = Vector3.zero;
    }

	[System.Serializable]
	public class GrabSettings
	{
		public HandGrabSettings LeftHandSetting = new HandGrabSettings();
		public HandGrabSettings RightHandSetting = new HandGrabSettings();
		public bool KinematicOnGrab = false;
		public bool DestroyOnRelease = false;
		public bool ParentObject = false;

		public SnapSettings SnapSettings = SnapSettings.SnapSettings_None;
		public SnapTransform SnapTransform = new SnapTransform();
	}

	public class GrabbableObject : MonoBehaviour
	{
		/* 
		* Name: OnGrabbed.
		* Description: Called when the object is grabbed.
		*/
		public delegate void GrabbedHandler(object sender, GrabbedEventArgs e);
		public event GrabbedHandler Grabbed = delegate { };

		protected void OnGrabbed(object sender, GrabbedEventArgs e)
		{
			Grabbed.Invoke(sender, e);
		}

		/* 
		* Name: OnReleased.
		* Description: Called when the object is released.
		*/
		public delegate void ReleasedHandler(object sender, ReleasedEventArgs e);
		public event ReleasedHandler Released = delegate { };

		protected void OnReleased(object sender, ReleasedEventArgs e)
		{
			Released.Invoke(sender, e);
		}

		protected Grabber _grabbedBy = null;
		public bool IsGrabbed => _grabbedBy != null;

		protected bool _isInAir = false;
		public Grabber GrabbedBy => _grabbedBy;

		public bool CanBeGrabbed = true;

		protected Transform _initialTransform = null;

		[SerializeField]
		private GrabSettings _grabSettings = null;

		protected void Awake() => _initialTransform = transform.parent;

        void OnDestroy()
		{
			/// Releases the object when it gets destroyed.
			if (_grabbedBy != null)
				_grabbedBy.ForceRelease();
		}

		/// <summary>
		/// Called when the object gets picked up.
		/// </summary>
		/// <param name="grabber">The hand that picked up the object.</param>
		/// <param name="grabPoint">The grabpoint of the object.</param>
		public virtual bool Grab(Grabber grabber, bool parentGrabbedObject = false)
		{
			if (!CanBeGrabbed)
				return false;

			if (_grabbedBy)
				return false;

			if (!grabber)
				return false;

			GetComponentsInChildren<Collider>().ToList().FindAll(x => !x.isTrigger).ForEach(x => x.enabled = false);

			if (parentGrabbedObject || _grabSettings.ParentObject)
				transform.SetParent(grabber.transform);

			_grabbedBy = grabber;

			Rigidbody _rigidBody = gameObject.GetComponent<Rigidbody>();
			_rigidBody.velocity = Vector3.zero;
			_rigidBody.angularVelocity = Vector3.zero;

			GrabbedEventArgs e = new GrabbedEventArgs
			{
				grabber = grabber,
				grabbed = this
			};
			OnGrabbed(this, e);

			_isInAir = true;

			if (_grabSettings.SnapSettings == SnapSettings.SnapSettings_Position || _grabSettings.SnapSettings == SnapSettings.SnapSettings_Both)
            {
				if (grabber.HandType == Hand.Hand_Right)
					transform.localPosition = _grabSettings.SnapTransform.Position;
				else
					transform.localPosition = new Vector3(-_grabSettings.SnapTransform.Position.x, _grabSettings.SnapTransform.Position.y, _grabSettings.SnapTransform.Position.z);
			}
			if (_grabSettings.SnapSettings == SnapSettings.SnapSettings_Rotation || _grabSettings.SnapSettings == SnapSettings.SnapSettings_Both)
				transform.localRotation = Quaternion.Euler(_grabSettings.SnapTransform.Rotation.x, _grabSettings.SnapTransform.Rotation.y, _grabSettings.SnapTransform.Rotation.z);

			FixedJoint joint = AddFixedJoint();

			joint.connectedBody = grabber.GetComponent<Rigidbody>();

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="grabber">The hand that released up the object.</param>
		/// <param name="linearVelocity">The linear velocity of the object that got released.</param>
		/// <param name="angularVelocity">The angular velocity of the object that got released.</param>
		public virtual bool Release(Grabber grabber, Vector3 linearVelocity, Vector3 angularVelocity)
		{
			if (!_grabbedBy)
				return false;

			GetComponentsInChildren<Collider>().ToList().FindAll(x => !x.isTrigger).ForEach(x => x.enabled = true);

			Rigidbody _rigidBody = gameObject.GetComponent<Rigidbody>();
			_rigidBody.isKinematic = false;
			_rigidBody.velocity = linearVelocity;
			_rigidBody.angularVelocity = angularVelocity;
			_grabbedBy = null;

			transform.SetParent(_initialTransform);

			ReleasedEventArgs e = new ReleasedEventArgs
			{
				grabber = grabber,
				grabbed = this
			};
			OnReleased(this, e);

			RemoveFixedJoint();

			if (_grabSettings.DestroyOnRelease)
				Destroy(this.gameObject);
			return true;
		}

		/// <summary>
		/// Adds a fixed joint.
		/// </summary>
		/// <returns></returns>
		protected FixedJoint AddFixedJoint()
		{
			FixedJoint fixedJ = GetComponent<FixedJoint>() == null ? gameObject.AddComponent<FixedJoint>() : GetComponent<FixedJoint>();
			fixedJ.breakForce = 20000;
			fixedJ.breakTorque = 20000;
			return fixedJ;
		}

		/// <summary>
		/// Removes a fixed joint if present.
		/// </summary>
		protected void RemoveFixedJoint()
		{
			FixedJoint joint = null;
			if (joint = GetComponent<FixedJoint>())
			{
				GetComponent<FixedJoint>().connectedBody = null;
				Destroy(joint);
			}
		}

		/// <summary>
		/// Simple method that changes variables when the chicken hits the ground.
		/// </summary>
		public virtual void OnTouchGround() => _isInAir = false;
    }
}