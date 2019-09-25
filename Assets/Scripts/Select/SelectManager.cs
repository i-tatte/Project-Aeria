using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour {
	public GameObject SerialControllerObject;
	SerialController serialController;
	bool SerialInput = true;
	string[] inputKeys = new string[] { "a", "z", "s", "x", "d", "c", "f", "v", "j", "m", "k", ",", "l", ".", "=", "/" };
	void Start () {
		StreamReader sr = new StreamReader (Application.dataPath + "/Musics/musics.list", Encoding.UTF8);
		Scanner sc = new Scanner (sr);
		int N = sc.nextInt ();
		SelectGV.musicInfo = new MusicInfo[N];
		for (int i = 0; i < N; i++) {
			string fileName = sc.next ();
			SelectGV.musicInfo[i] = new MusicInfo (fileName);
			Debug.Log (SelectGV.musicInfo[i].ToString ());
		}
		serialController = SerialControllerObject.GetComponent<SerialController> ();
	}

	void Update () {
		getSignal ();
		Debug.Log (string.Join (" ", GVContainer.signal) + "\r\n" + string.Join (" ", GVContainer.airSignal));
		//Debug.Log (GVContainer.globalSpeed);
	}

	public void getSignal () {
		if (SerialInput) {
			bool[] tmp = serialController.getSignal ();
			for (int i = 0; i < 16; i++) {
				if (GVContainer.signal[i] == 2 && tmp[i]) break;
				if (i == 15) allOn (0);
			}
			for (int i = 0; i < 16; i++) {
				if (GVContainer.signal[i] < 4) {
					if (tmp[i] == true) {
						if (GVContainer.signal[i] == 0) { //前のフレームで押されてなかった
							GVContainer.signal[i] = 1;
						}
					} else if (GVContainer.signal[i] == 1) {
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
			if (Input.GetKeyDown ("w")) { }
		}
	}
	void allOn (int num) {
		for (int i = 0; i < 16; i++) {
			GVContainer.signal[i] = num;
		}
	}
}