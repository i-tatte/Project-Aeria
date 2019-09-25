using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour {
	public GameObject DifficultyTextObject, SongNameTextObject, ComposerTextObject, ScoreTextObject, RankTextObject,
	JudgeResultTextObject, MaxComboTextObject, TapToNextTextObject, JacketImageObject, serialControllerObject;
	public Material SSSRank, SSRank, SRank, AARank, ARank, BRank, CRank, DRank;
	Text DifficultyText, SongNameText, ComposerText, ScoreText, RankText, JudgeResultText, MaxComboText, TapToNextText;
	RawImage JacketImage;
	SerialController serial;
	bool SerialInput = true;
	string[] inputKeys = new string[] { "a", "z", "s", "x", "d", "c", "f", "v", "j", "m", "k", ",", "l", ".", "=", "/" };
	// Start is called before the first frame update
	void Start () {
		if (true) {
			DifficultyText = DifficultyTextObject.GetComponent<Text> ();
			SongNameText = SongNameTextObject.GetComponent<Text> ();
			ComposerText = ComposerTextObject.GetComponent<Text> ();
			ScoreText = ScoreTextObject.GetComponent<Text> ();
			RankText = RankTextObject.GetComponent<Text> ();
			JudgeResultText = JudgeResultTextObject.GetComponent<Text> ();
			MaxComboText = MaxComboTextObject.GetComponent<Text> ();
			TapToNextText = TapToNextTextObject.GetComponent<Text> ();
			JacketImage = JacketImageObject.GetComponent<RawImage> ();
		}
		DifficultyText.text = GVContainer.difficultiesName[GVContainer.stage] + " " + GVContainer.difficulty[GVContainer.stage];
		SongNameText.text = GVContainer.displayName;
		ComposerText.text = GVContainer.composer;
		int score = (int) Math.Round (GVContainer.score);
		string formattedScore = String.Format ("{0:00000000}", score);
		ScoreText.text = formattedScore.Substring (0, 2) + "'" + formattedScore.Substring (2, 3) + "'" + formattedScore.Substring (5, 3);
		RankText.text = clearRankCalculate (score);
		JudgeResultText.text = GVContainer.results[0] + Environment.NewLine + GVContainer.results[1] +
			Environment.NewLine + GVContainer.results[2] + Environment.NewLine + GVContainer.results[3];
		MaxComboText.text = GVContainer.maxCombo.ToString ();
		string path = Application.dataPath.Replace ("/", "\\") + "\\Musics\\" + GVContainer.songName + ".jpg";
		StartCoroutine (downloadImage (path));
		serial = serialControllerObject.GetComponent<SerialController> ();
	}

	// Update is called once per frame
	void Update () {
		getSignal ();
	}

	private string clearRankCalculate (int score) {
		if (score >= 9_800_000) {
			RankText.material = SSSRank;
			return "SSS";
		} else if (score >= 9_500_000) {
			RankText.material = SSRank;
			return "SS";
		} else if (score >= 9_000_000) {
			RankText.material = SRank;
			return "S";
		} else if (score >= 8_500_000) {
			RankText.material = AARank;
			return "AA";
		} else if (score >= 8_000_000) {
			RankText.material = ARank;
			return "A";
		} else if (score >= 7_500_000) {
			RankText.material = BRank;
			return "B";
		} else
		if (score >= 6_000_000) {
			RankText.material = CRank;
			return "C";
		}
		RankText.material = DRank;
		return "D";
	}

	public void getSignal () {
		if (SerialInput) {
			bool[] tmp = serial.getSignal ();
			for (int i = 0; i < 16; i++) {
				if (GVContainer.signal[i] < 4) {
					if (tmp[i] == true) {
						if (GVContainer.signal[i] == 0) { //前のフレームで押されてなかった
							allOn (1);
							break;
						} else if (GVContainer.signal[i] == 1) {
							allOn (2);
							break;
						}
					}
				}
				if (i == 15) {
					allOff ();
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
	void allOff () {
		for (int i = 0; i < 16; i++) {
			GVContainer.signal[i] = 0;
		}
	}

	IEnumerator downloadImage (string path) {
		RawImage rawImage = JacketImageObject.GetComponent<RawImage> ();

		Debug.Log (path);

		byte[] bytes = File.ReadAllBytes (path);
		Texture2D texture = new Texture2D (200, 200);
		texture.filterMode = FilterMode.Trilinear;
		texture.LoadImage (bytes);

		rawImage.texture = texture;
		//rawImage.SetNativeSize ();

		yield return null;
	}
}