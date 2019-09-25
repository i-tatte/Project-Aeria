using UnityEngine;

public class RotateSkybox : MonoBehaviour {
	public float _anglePerSecond = 0.1f;
	float _rot = 0.0f;
	public Material sky;

	// Use this for initialization
	void Start () {
		RenderSettings.skybox = sky;
	}

	// Update is called once per frame
	void Update () {
		_rot += _anglePerSecond * Time.deltaTime;
		sky.SetFloat ("_Rotation", _rot % 360f); // 回す
	}
}