using UnityEngine;

namespace Kirurobo {
	/// <summary>
	/// アタッチしたオブジェクトを一定速度でヨー回転させる
	/// </summary>
	public class AutoRotator : MonoBehaviour {
		/// <summary>
		/// 回転速度 [deg/s]
		/// </summary>
		public float angularVelocity = 90f;

		/// <summary>
		/// 回転軸（ヨー回転のため上向き）
		/// </summary>
		Vector3 rotationAxis = Vector3.up;

		/// <summary>
		/// 初期姿勢
		/// </summary>
		Quaternion initialLocalRotation;

		// Use this for initialization
		void Start () {
			// 初期姿勢を記憶
			initialLocalRotation = transform.localRotation;
		}
	
		// Update is called once per frame
		void Update () {
			var rotation = Quaternion.Euler(0f, Time.time * angularVelocity, 0f);
			transform.localRotation = initialLocalRotation * rotation;
		}
	}
}
