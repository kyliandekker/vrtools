using UnityEngine;

namespace VRTools.Utils
{
	public class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static T Instance = null;

		[SerializeField]
		private bool _dontDestroyOnLoad = false;

		protected void Awake()
		{
			if (Instance == null)
				Instance = gameObject.GetComponent<T>();
			else if (Instance.GetInstanceID() != GetInstanceID())
				Destroy(gameObject);

			if (_dontDestroyOnLoad)
				DontDestroyOnLoad(gameObject);
		}
	}
}