using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectAnimationController : MonoBehaviour {
	public RawImage[] ChangeMusicFrames = new RawImage[3];
	public Material Unselected, Selected;
	public Material[] LevelBack = new Material[4];
	int selectPosition = 0;
	public GameObject[] DisplayJacketObjects = new GameObject[7];
	public GameObject[] DifficultyLabel = new GameObject[4];
	public GameObject[] DifficultyLabelTextObject = new GameObject[4];
	public GameObject SongNameTextObject, ComposerTextObject, LevelTextObject, ConfirmTextObject, DifficultyTextObject, SpeedTextObject, LevelSelectBack, SpeedSelectBack, MapperTextObject;
	Text SongNameText, ComposerText, LevelText, DifficultyText, SpeedText, ConfirmText, MapperText;
	Text[] DifficultyLabelText = new Text[4];
	RawImage[] DisplayJackets = new RawImage[7];
	AudioSource audioSource;
	public Animator animator;
	// Start is called before the first frame update
	void Start () {
		for (int i = 0; i < 7; i++) {
			DisplayJackets[i] = DisplayJacketObjects[i].GetComponent<RawImage> ();
		}
		for (int i = 0; i < 4; i++) {
			DifficultyLabelText[i] = DifficultyLabelTextObject[i].GetComponent<Text> ();
		}
		SongNameText = SongNameTextObject.GetComponent<Text> ();
		ComposerText = ComposerTextObject.GetComponent<Text> ();
		LevelText = LevelTextObject.GetComponent<Text> ();
		DifficultyText = DifficultyTextObject.GetComponent<Text> ();
		SpeedText = SpeedTextObject.GetComponent<Text> ();
		ConfirmText = ConfirmTextObject.GetComponent<Text> ();
		MapperText = MapperTextObject.GetComponent<Text> ();
		setJacketImage (selectPosition);
		DisplayMusicInfomation ();
		StartDemoMusic ();
		audioSource = GetComponent<AudioSource> ();
	}

	// Update is called once per frame
	void Update () {
		keyEventCheck ();
	}

	void onLeftAnimationStart () {
		ChangeMusicFrames[1].material = Unselected;
		ChangeMusicFrames[2].material = Selected;
	}
	void onLeftAnimationEnd () {
		ChangeMusicFrames[1].material = Selected;
		ChangeMusicFrames[2].material = Unselected;
		animator.SetBool ("Left", false);
		selectPosition++;
		setJacketImage (selectPosition);
		DisplayMusicInfomation ();
		StartDemoMusic ();
	}
	void onRightAnimationStart () {
		ChangeMusicFrames[1].material = Unselected;
		ChangeMusicFrames[0].material = Selected;
	}
	void onRightAnimationEnd () {
		ChangeMusicFrames[0].material = Unselected;
		ChangeMusicFrames[1].material = Selected;
		animator.SetBool ("Right", false);
		selectPosition--;
		setJacketImage (selectPosition);
		DisplayMusicInfomation ();
		StartDemoMusic ();
	}
	void onEnterAnimationStart () {
		GVContainer.stage = 3;
		levelClick ();
		MusicInfo currentMusic = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length];
		LevelText.text = GVContainer.difficultiesName[GVContainer.stage];
		DifficultyText.text = currentMusic.difficulties[GVContainer.stage];
		SpeedText.text = "×" + string.Format ("{0:0.0}", GVContainer.globalSpeed);
		LevelSelectBack.GetComponent<RawImage> ().material = LevelBack[GVContainer.stage];
	}
	void onConfirmAnimationStart () {
		MusicInfo currentMusic = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length];
		ConfirmText.text = String.Format ("この譜面は、『{0}』" + Environment.NewLine +
			"{1}「{2}」難易度です。" + Environment.NewLine +
			"プレイしますか？", currentMusic.displayName, GVContainer.difficultiesName[GVContainer.stage], currentMusic.difficulties[GVContainer.stage]);
	}
	void onFadeOutStart () {
		StartCoroutine (FadeOutMusic (0.8f));
	}
	void onFadeOutEnd () {
		MusicInfo currentMusic = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length];
		GVContainer.songName = currentMusic.fileName;
		SceneManager.LoadScene ("GameScene");
	}
	void keyEventCheck () {
		if (!animator.GetBool ("Start") && animator.GetBool ("Confirm")) {
			if (Input.GetKeyDown (KeyCode.Return)) {
				animator.SetBool ("Start", true);
			}
			if (Input.GetKeyDown (KeyCode.Escape)) {
				animator.SetBool ("Confirm", false);
			}
		} else if (animator.GetBool ("Enter")) {
			if (Input.GetKeyDown (KeyCode.Escape)) {
				animator.SetBool ("Enter", false);
			}
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				GVContainer.globalSpeed += 0.1f;
				MusicInfo currentMusic = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length];
				LevelText.text = GVContainer.difficultiesName[GVContainer.stage];
				DifficultyText.text = currentMusic.difficulties[GVContainer.stage];
				SpeedText.text = "×" + string.Format ("{0:0.0}", GVContainer.globalSpeed);
				LevelSelectBack.GetComponent<RawImage> ().material = LevelBack[GVContainer.stage];
			}
			if (Input.GetKeyDown (KeyCode.DownArrow)) {
				GVContainer.globalSpeed -= 0.1f;
				MusicInfo currentMusic = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length];
				LevelText.text = GVContainer.difficultiesName[GVContainer.stage];
				DifficultyText.text = currentMusic.difficulties[GVContainer.stage];
				SpeedText.text = "×" + string.Format ("{0:0.0}", GVContainer.globalSpeed);
				LevelSelectBack.GetComponent<RawImage> ().material = LevelBack[GVContainer.stage];
			}
			if (Input.GetKeyDown (KeyCode.Space)) {
				levelClick ();
				MusicInfo currentMusic = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length];
				LevelText.text = GVContainer.difficultiesName[GVContainer.stage];
				DifficultyText.text = currentMusic.difficulties[GVContainer.stage];
				SpeedText.text = "×" + string.Format ("{0:0.0}", GVContainer.globalSpeed);
				LevelSelectBack.GetComponent<RawImage> ().material = LevelBack[GVContainer.stage];
			}
			if (Input.GetKeyDown (KeyCode.Return)) {
				animator.SetBool ("Confirm", true);
			}
		} else {
			if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				animator.SetBool ("Right", true);
			}
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				animator.SetBool ("Left", true);
			}
			if (Input.GetKeyDown (KeyCode.Space)) {
				levelClick ();
			}
			if (Input.GetKeyDown (KeyCode.Return)) {
				animator.SetBool ("Enter", true);
			}
		}
		if (!animator.GetBool ("Start") && animator.GetBool ("Confirm")) {
			for (int i = 0; i < 16; i++) {
				if (GVContainer.signal[i] == 1) {
					if (i < 10) {
						animator.SetBool ("Confirm", false);
						allOn (0, 10, 2);
					} else {
						animator.SetBool ("Start", true);
						allOn (10, 16, 2);
					}
					break;
				}
			}
		} else if (animator.GetBool ("Enter")) {
			for (int i = 0; i < 16; i++) {
				if (GVContainer.signal[i] == 1) {
					if (i < 2) {
						animator.SetBool ("Enter", false);
						allOn (0, 2, 2);
					} else if (i < 4) {
						if (GVContainer.globalSpeed > 0.25) {
							GVContainer.globalSpeed -= 0.1f;
							MusicInfo currentMusic = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length];
							LevelText.text = GVContainer.difficultiesName[GVContainer.stage];
							DifficultyText.text = currentMusic.difficulties[GVContainer.stage];
							SpeedText.text = "×" + string.Format ("{0:0.0}", GVContainer.globalSpeed);
							LevelSelectBack.GetComponent<RawImage> ().material = LevelBack[GVContainer.stage];
							allOn (2, 4, 2);
						}
					} else if (i < 6) {
						if (GVContainer.globalSpeed < 2.55) {
							GVContainer.globalSpeed += 0.1f;
							MusicInfo currentMusic = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length];
							LevelText.text = GVContainer.difficultiesName[GVContainer.stage];
							DifficultyText.text = currentMusic.difficulties[GVContainer.stage];
							SpeedText.text = "×" + string.Format ("{0:0.0}", GVContainer.globalSpeed);
							LevelSelectBack.GetComponent<RawImage> ().material = LevelBack[GVContainer.stage];
							allOn (4, 6, 2);
						}
					} else if (i < 10) {
						levelClick ();
						MusicInfo currentMusic = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length];
						LevelText.text = GVContainer.difficultiesName[GVContainer.stage];
						DifficultyText.text = currentMusic.difficulties[GVContainer.stage];
						SpeedText.text = "×" + string.Format ("{0:0.0}", GVContainer.globalSpeed);
						LevelSelectBack.GetComponent<RawImage> ().material = LevelBack[GVContainer.stage];
						allOn (6, 10, 2);
					} else if (i < 11) {
						break;
					} else {
						animator.SetBool ("Confirm", true);
						allOn (11, 16, 2);
					}
					break;
				}
			}
		} else {
			for (int i = 0; i < 16; i++) {
				if (GVContainer.signal[i] == 1) {
					if (i < 4) {
						animator.SetBool ("Right", true);
						allOn (0, 4, 2);
					} else if (i < 12) {
						animator.SetBool ("Enter", true);
						allOn (4, 12, 2);
					} else {
						animator.SetBool ("Left", true);
						allOn (12, 16, 2);
					}
					break;
				}
			}
		}
	}
	void allOn (int start, int end, int num) {
		for (int i = start; i < end; i++) {
			GVContainer.signal[i] = num;
		}
	}
	//[start, end)の範囲
	void levelClick () {
		MusicInfo currentMusic = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length];
		GVContainer.stage++;
		GVContainer.stage %= 4;
		while (currentMusic.difficulties[GVContainer.stage] == "0") {
			GVContainer.stage++;
			GVContainer.stage %= 4;
		}
	}
	void StartDemoMusic () {
		StartCoroutine (inputMusicFile (SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length].fileName));
	}
	private void setJacketImage (int position) {
		while (position < 0) {
			position += SelectGV.musicInfo.Length;
			selectPosition += SelectGV.musicInfo.Length;
		}
		for (int i = 0; i < 7; i++) {
			string path = Application.dataPath.Replace ("/", "\\") + "\\Musics\\" + SelectGV.musicInfo[(position + i) % SelectGV.musicInfo.Length].fileName + ".jpg";
			StartCoroutine (downloadImage (path, i));
		}
	}
	IEnumerator downloadImage (string path, int i) {
		byte[] bytes = File.ReadAllBytes (path);
		Texture2D texture = new Texture2D (200, 200);
		texture.filterMode = FilterMode.Trilinear;
		texture.LoadImage (bytes);
		DisplayJackets[i].texture = texture;

		yield return null;
	}
	private void DisplayMusicInfomation () {
		SongNameText.text = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length].displayName;
		ComposerText.text = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length].composer;
		MusicInfo currentMusic = SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length];
		for (int i = 0; i < 4; i++) {
			string tmp = currentMusic.difficulties[i];
			if (tmp != "0") {
				DifficultyLabel[i].SetActive (true);
				DifficultyLabelText[i].text = tmp;
			} else {
				DifficultyLabel[i].SetActive (false);
				DifficultyLabelText[i].text = "---";
			}
		}
		MapperText.text = "Map Design : " + currentMusic.mapper;
	}
	IEnumerator inputMusicFile (string name) {
		string path = Application.dataPath.Replace ("/", "\\") + "\\Musics\\" + name + ".ogg";
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip (path, AudioType.OGGVORBIS)) {
			yield return www.SendWebRequest ();

			try {
				audioSource.clip = DownloadHandlerAudioClip.GetContent (www);
				audioSource.time = (float) SelectGV.musicInfo[(selectPosition + 3) % SelectGV.musicInfo.Length].demoStart / 1000f;
				StartCoroutine (PlayWithFadeIn (0.3f));
			} catch (InvalidOperationException m) {
				Debug.Log (m.Message);
			}
		}
	}

	IEnumerator PlayWithFadeIn (float fadeTime) {
		audioSource.Play ();
		audioSource.volume = 0f;

		while (audioSource.volume < 1f) {
			float tempVolume = audioSource.volume + (Time.deltaTime / fadeTime);
			audioSource.volume = tempVolume > 1f ? 1f : tempVolume;
			yield return null;
		}
	}
	IEnumerator FadeOutMusic (float fadeTime) {
		while (audioSource.volume > 0f) {
			float tempVolume = audioSource.volume - (Time.deltaTime / fadeTime);
			audioSource.volume = tempVolume < 0f ? 0f : tempVolume;
			yield return null;
		}
	}
}