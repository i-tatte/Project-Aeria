using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickSensor : MonoBehaviour {
	public GameObject canvas;
	RectTransform rect;
	void Start () {
		rect = canvas.GetComponent<RectTransform> ();
	}
	void Update () {
		for (int i = 0; i < 16; i++) {
			if (GVContainer.signal[i] == 1) {
				OnClick ();
			}
		}
	}
	public void OnClick () {
		Debug.Log ("clicked!!");
		//rect.localPosition += new Vector3 (10, 0, 0);
		//camera.transform.Rotate (new Vector3 (0, 1, 0));
		SceneManager.LoadScene ("SelectScene");
	}
}