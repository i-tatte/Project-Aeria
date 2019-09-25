using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	// Use this for initialization 25~79.5
	public string lane_number;
	public Material tap_normal, tap_perfect, tap_great, tap_fair, tap_lost;
	public GameObject GUIScoreText, GUITitleText, GUIComboText, obj_Serial, seekPoint, DifficultyTextObject;
	public GameObject[] aerial;
	AudioSource audioSource;
	RectTransform seek;
	Text scoreText, titleText, comboText, difficultyText;
	GameObject[] tap = new GameObject[16];
	SerialController serial;
	double[] prevAerial = new double[6];
	string[] inputKeys = new string[] { "a", "z", "s", "x", "d", "c", "f", "v", "j", "m", "k", ",", "l", ".", "=", "/" };
	public static bool canUseSerialPort = true;
	bool view = false;
	void Start () { //初期化時に呼ばれる。デフォルトで呼ばれてめっさ強いのであんまりベタ書きしない方が吉。
		// プレハブを取得
		GameObject laneObject = (GameObject) Resources.Load ("lane" + lane_number);
		// プレハブからインスタンスを生成
		Instantiate (laneObject, new Vector3 (0, 0, 0), Quaternion.Euler (0, 0, 0));
		serial = obj_Serial.GetComponent<SerialController> ();
		audioSource = this.gameObject.GetComponent<AudioSource> ();
		scoreText = GUIScoreText.GetComponent<Text> ();
		titleText = GUITitleText.GetComponent<Text> ();
		comboText = GUIComboText.GetComponent<Text> ();
		difficultyText = DifficultyTextObject.GetComponent<Text> ();
		seek = seekPoint.GetComponent<RectTransform> ();
		for (int i = 0; i < 16; i++) {
			tap[i] = GameObject.Find ("tap" + i);
			tap[i].SetActive (false);
		}
		for (int i = 0; i < 6; i++) {
			Instantiate (aerial[i]);
		}
	}
	void Update () { //デフォルトのアップデートメソッド。こっちもめっさ強いのでベタ書きはやめような。
		/*
		//これは入力信号を1フレームごとに出力するデバッグ装置
		Debug.Log (string.Join (" ", GVContainer.signal) + "\r\n" + string.Join (" ", GVContainer.airSignal));
		//*/
		// Debug.Log (GVContainer.globalSpeed);

		getSignal ();
		tapActivate ();
		setScore ((int) GVContainer.score);
		GVContainer.ms = audioSource.time * 1000f;
		setSeekBar ();
		viewAerialInput ();
	}
	public void getSignal () {
		if (canUseSerialPort) {
			bool[] tmp = serial.getSignal ();
			for (int i = 0; i < 16; i++) {
				if (GVContainer.signal[i] < 4) {
					if (tmp[i] == true) {
						if (GVContainer.signal[i] == 0) { //前のフレームで押されてなかった
							GVContainer.signal[i] = 1;
						} else if (GVContainer.signal[i] == 1) {
							GVContainer.signal[i] = 2;
						} else {
							GVContainer.signal[i] = 3;
						}
					} else {
						GVContainer.signal[i] = 0;
					}
				}
			}
		} else {
			for (int i = 0; i < 16; i++) {
				if (GVContainer.signal[i] < 4) {
					if (Input.GetKey (inputKeys[i])) {
						if (GVContainer.signal[i] == 0) { //前のフレームで押されてなかった
							GVContainer.signal[i] = 1;
						} else if (GVContainer.signal[i] == 1) {
							GVContainer.signal[i] = 2;
						} else {
							GVContainer.signal[i] = 3;
						}
					} else {
						GVContainer.signal[i] = 0;
					}
				}
			}
			if (Input.anyKeyDown) { }
		}

		for (int i = 0; i < 16; i++) {
			if (GVContainer.lightingSignal[i] > 0) {
				GVContainer.lightingSignal[i] -= (int) (Time.deltaTime * 1000f) * 4;
			}
		}

		for (int i = 0; i < 6; i++) {
			GVContainer.airSignal[i] = (GVContainer.airSignal[i] + prevAerial[i]) / 2.0;
			prevAerial[i] = GVContainer.airSignal[i];
		}
		if (Input.GetKeyDown (KeyCode.Escape)) {
			SceneManager.LoadScene ("ResultScene");
		}
		if (Input.GetKeyDown (KeyCode.V)) view = !view;
	}
	void viewAerialInput () {
		if (view) {
			double[] air = GVContainer.airSignal;
			//AIL,AIRを動かす。
			for (int i = 0; i < 6; i++) {
				aerial[i].transform.position = new Vector3 ((float) air[i] / 4.0f - 2.0f, -2.65f - ((i % 3) * 0.1f), -14.42259f);
			}
		} else {
			for (int i = 0; i < 6; i++) {
				aerial[i].transform.position = new Vector3 (100, 0, 0);
			}
		}
	}
	void tapActivate () {
		for (int i = 0; i < 16; i++) {
			if (GVContainer.signal[i] != 0) {
				tap[i].SetActive (true);
			} else {
				tap[i].SetActive (false);
			}
			if (GVContainer.lightingSignal[i] > 0) {
				tap[i].SetActive (true);
				switch (GVContainer.lightingSignal[i] % 4) {
					case 0:
						tap[i].GetComponent<Renderer> ().material = tap_perfect;
						break;
					case 1:
						tap[i].GetComponent<Renderer> ().material = tap_great;
						break;
					case 2:
						tap[i].GetComponent<Renderer> ().material = tap_fair;
						break;
					case 3:
						tap[i].GetComponent<Renderer> ().material = tap_lost;
						break;
				}
			} else {
				tap[i].GetComponent<Renderer> ().material = tap_normal;
			}
		}
	}

	void setScore (int score) {
		scoreText.text = string.Format ("{0:00000000}", GVContainer.score);
		titleText.text = GVContainer.displayName;
		comboText.text = GVContainer.combo.ToString ();
		difficultyText.text = GVContainer.difficultiesName[GVContainer.stage] + " " + GVContainer.difficulty[GVContainer.stage];
	}
	void setSeekBar () {
		float x = ((470f) * (GVContainer.ms / (audioSource.clip.length * 1000f)));
		seek.localPosition = new Vector3 (x, 0, 0);
	}
	private double average (List<double> list) {
		double ret = 0;
		for (int i = 0; i < list.Count; i++) {
			ret += list[i];
		}
		ret /= 10.0;
		return ret;
	}
}