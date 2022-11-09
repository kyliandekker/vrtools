using System;
using UnityEngine;

namespace VRTools.Interaction
{
    public class GrabEventArgs : EventArgs
	{
		public Grabber grabber;
		public GrabbableObject grabbed;
	}

	public class ReleaseEventArgs : EventArgs
	{
		public Grabber grabber;
		public GrabbableObject grabbed;
	}

	public enum Hand
	{
		Hand_Left,
		Hand_Right
	}

	public class Grabber : MonoBehaviour
	{
		[SerializeField]
		protected bool _parentHeldObject = false;

		/* 
		* Name: OnGrab.
		* Description: Called when the grabber grabs an object.
		*/
		public delegate void GrabHandler(object sender, GrabEventArgs e);
		public event GrabHandler Grab = delegate { };

		private void OnGrab(object sender, GrabEventArgs e)
		{
			Grab.Invoke(sender, e);
		}

		/* 
		* Name: OnReleased.
		* Description: Called when the grabber releases an object.
		*/
		public delegate void ReleaseHandler(object sender, ReleaseEventArgs e);
		public event ReleaseHandler Release = delegate { };

		private void OnRelease(object sender, ReleaseEventArgs e)
		{
			Release.Invoke(sender, e);
		}

		protected GrabbableObject _grabbedObj = null;
		public GrabbableObject GrabbedObject => _grabbedObj;

		protected GrabbableObject _potentiallyGrabbedObject = null;

		[SerializeField]
		protected Hand _handType;

		public Hand HandType => _handType;

		/// <summary>
		/// Called when the player must release an object without input.
		/// </summary>
		/// <param name="object">The object that got released up.</param>
		public void ForceRelease()
		{
			if (_grabbedObj)
				StartRelease(new Vector3(), new Vector3());
		}

		protected void OnTriggerEnter(Collider other)
		{
			GrabbableObject grabbable = other.GetComponent<GrabbableObject>() ?? other.GetComponentInParent<GrabbableObject>() ?? other.GetComponentInChildren<GrabbableObject>();
			if (grabbable == null)
				return;

			if (_potentiallyGrabbedObject == grabbable)
				return;

			if (_grabbedObj != null)
				return;

			if (!_potentiallyGrabbedObject || Vector3.Distance(_potentiallyGrabbedObject.transform.position, transform.position) < Vector3.Distance(grabbable.transform.position, transform.position))
				_potentiallyGrabbedObject = grabbable;
		}

		protected void OnTriggerExit(Collider other)
		{
			GrabbableObject grabbable = other.GetComponent<GrabbableObject>() ?? other.GetComponentInParent<GrabbableObject>() ?? other.GetComponentInChildren<GrabbableObject>();
			if (grabbable == null)
				return;

			if (_potentiallyGrabbedObject == grabbable)
				_potentiallyGrabbedObject = null;
		}

		/// <summary>
		/// Grabs the current potentially grabbed object.
		/// </summary>
		public void StartGrab()
		{
			if (!_potentiallyGrabbedObject)
				return;

			_grabbedObj = _potentiallyGrabbedObject;
			if (_grabbedObj.Grab(this, _parentHeldObject))
				return;
		}

		public void ResetGrabbable() => _grabbedObj = null;

		public void StartRelease(Vector3 linearVelocity, Vector3 angularVelocity)
		{
			if (!_grabbedObj)
				return;

			if (_grabbedObj.Release(this, linearVelocity, angularVelocity))
			{
				if (_grabbedObj == _potentiallyGrabbedObject)
					_potentiallyGrabbedObject = null;
				_grabbedObj = null;
			}
		}
	}
}