using UnityEditor;
using UnityEngine;

namespace VRTools.Interaction.Editor
{
	[CustomEditor(typeof(GrabbableObject), true)]
	public class GrabbableObjectEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GrabbableObject grabbable = (GrabbableObject)target;

			GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.miniButton);
			myFoldoutStyle.fontStyle = FontStyle.Bold;
			myFoldoutStyle.fontSize = 12;
			Color myStyleColor = Color.white;
			myFoldoutStyle.normal.textColor = myStyleColor;
			myFoldoutStyle.onNormal.textColor = myStyleColor;
			myFoldoutStyle.hover.textColor = myStyleColor;
			myFoldoutStyle.onHover.textColor = myStyleColor;
			myFoldoutStyle.focused.textColor = myStyleColor;
			myFoldoutStyle.onFocused.textColor = myStyleColor;
			myFoldoutStyle.active.textColor = myStyleColor;
			myFoldoutStyle.onActive.textColor = myStyleColor;

			if (Application.isPlaying)
			{
				if (!VRControls.Instance)
				{
					Debug.LogWarning("There is no VR Rig in the scene.");
					return;
				}
				Grabber hand = VRControls.Instance.GetHand(Hand.Hand_Left);
				if (GUILayout.Button(grabbable.GrabbedBy == hand ? "Release (Left Hand)" : "Grab (Left Hand)", myFoldoutStyle))
				{
					if (!Application.isPlaying)
						return;

					_ = grabbable.GrabbedBy != hand
						? grabbable.Grab(hand)
						: grabbable.Release(hand, new Vector3(), new Vector3());
				}

				hand = VRControls.Instance.GetHand(Hand.Hand_Right);
				if (GUILayout.Button(grabbable.GrabbedBy == hand ? "Release (Right Hand)" : "Grab (Right Hand)", myFoldoutStyle))
				{
					if (!Application.isPlaying)
						return;

					_ = grabbable.GrabbedBy != hand
						? grabbable.Grab(hand)
						: grabbable.Release(hand, new Vector3(), new Vector3());
				}

				GUILayout.Label("Status: " + (grabbable.GrabbedBy ? ("Grabbed by " + (VRControls.Instance.GetHand(Hand.Hand_Left) == grabbable.GrabbedBy ? "left hand" : "right hand")) : "Not grabbed"));
			}
			else
			{
				bool initial = GUI.enabled;
				GUI.enabled = false;
				if (GUILayout.Button("Grab (Left Hand)"))
				{

				}
				if (GUILayout.Button("Grab (Right Hand)"))
				{

				}
				GUI.enabled = initial;
			}

			_ = serializedObject.ApplyModifiedProperties();
		}
	}
}