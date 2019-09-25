using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour {
	AudioSource audioSource;
	public GameObject BPMInputFieldObject, measureInputFieldObject, offsetInputFieldObject, quantizeInputFieldObject, displaySongNameInputFieldObject, mapperInputFieldObject, composerInputFieldObject, displayBPMInputFieldObject, demoStartInputFieldObject, difficultyInputFieldObject, speedInputFieldObject,
	songNameTextBoxObject, songInfoTextObject, BPMTextObject, measureTextObject, quantizeTextObject, offsetTextObject,
	scrollBarObject, kindSelectorObject, difficultySelectorObject, saveTextObject;
	public GameObject currentPositionMarker, startLaneMarker, bar, smallBar, normalNote;
	Text songNameTextBox, songInfoText, saveText;
	Scrollbar scrollBar;
	Dropdown kindSelector, difficultySelector;
	InputField BPMInputField, measureInputField, offsetInputField, quantizeInputField, displaySongNameInputField, mapperInputField, composerInputField, displayBPMInputField, demoStartInputField, difficultyInputField, speedInputField;
	string songName, BPMString, measureString, quantizeString, offsetString;
	int measure, quantize, kind;
	float BPM, offset;
	int currentPos = 0;
	int inputTimer = 0;
	int difficulty = 0;

	NoteInfo currentEditing;
	int editMode = 0;
	List<int> lane = new List<int> ();
	int startLane = -1;
	List<float> args = new List<float> ();
	List<NoteInfo> noteMap = new List<NoteInfo> ();
	List<float> barList = new List<float> ();
	List<float> qtList = new List<float> ();
	int[][] bpmChange = new int[50][];
	void Start () {
		Screen.SetResolution (1280, 600, false, 60);
		GVContainer.speed = 5;
		//GetComponent系の折りたたみ
		if (true) {
			audioSource = this.gameObject.GetComponent<AudioSource> ();
			songNameTextBox = songNameTextBoxObject.GetComponent<Text> ();
			songInfoText = songInfoTextObject.GetComponent<Text> ();
			saveText = saveTextObject.GetComponent<Text> ();
			scrollBar = scrollBarObject.GetComponent<Scrollbar> ();
			kindSelector = kindSelectorObject.GetComponent<Dropdown> ();
			BPMInputField = BPMInputFieldObject.GetComponent<InputField> ();
			measureInputField = measureInputFieldObject.GetComponent<InputField> ();
			offsetInputField = offsetInputFieldObject.GetComponent<InputField> ();
			quantizeInputField = quantizeInputFieldObject.GetComponent<InputField> ();
			displaySongNameInputField = displaySongNameInputFieldObject.GetComponent<InputField> ();
			mapperInputField = mapperInputFieldObject.GetComponent<InputField> ();
			composerInputField = composerInputFieldObject.GetComponent<InputField> ();
			displayBPMInputField = displayBPMInputFieldObject.GetComponent<InputField> ();
			demoStartInputField = demoStartInputFieldObject.GetComponent<InputField> ();
			difficultyInputField = difficultyInputFieldObject.GetComponent<InputField> ();
			speedInputField = speedInputFieldObject.GetComponent<InputField> ();
			difficultySelector = difficultySelectorObject.GetComponent<Dropdown> ();
		}
	}

	// Update is called once per frame
	void Update () {
		GVContainer.ms = audioSource.time * 1000f;
		applySeconds ();
		keyEventCheck ();
	}

	void showGUI () {
		string path = Application.dataPath;
		GUI.TextArea (new Rect (5, 5, Screen.width, 50), path);
	}

	public void applyValues () {
		songName = songNameTextBox.text;
		BPMString = BPMInputField.text;
		measureString = measureInputField.text;
		quantizeString = quantizeInputField.text;
		offsetString = offsetInputField.text;
		kind = kindSelector.value;
		if (BPMString != "" && measureString != "" && quantizeString != "" && offsetString != "" && audioSource.clip != null) {
			BPM = float.Parse (BPMString);
			measure = int.Parse (measureString);
			quantize = int.Parse (quantizeString);
			offset = int.Parse (offsetString);
			generateBar ();
		}
	}

	void applySeconds () {
		if (audioSource.clip != null) {
			songInfoText.text = "(" + string.Format ("{0:f4}", GVContainer.ms) + " / " +
				audioSource.clip.length * 1000f + ") [ms] " + songName;
			scroll (GVContainer.ms / (audioSource.clip.length * 1000f));
		}
	}

	public void onScrolled () {
		if (audioSource != null && audioSource.clip != null) {
			GVContainer.ms = audioSource.clip.length * scrollBar.value * 1000f;
			audioSource.time = GVContainer.ms / 1000f;
		}
	}

	void scroll (float val) {
		scrollBar.value = val;
		if (audioSource.time == 0f && !audioSource.isPlaying) {
			audioSource.Play ();
			audioSource.pitch = 0f;
		}
	}

	public void songInput () {
		songName = songNameTextBox.text;
		StartCoroutine (inputMusicFile (songName));
		string path = Application.dataPath.Replace ("/", "\\") + "\\Musics\\" + songName + ".amp";
		if (File.Exists (path)) {
			StreamReader sr = new StreamReader (path, Encoding.UTF8);
			Scanner sc = new Scanner (sr);
			GVContainer.displayName = sc.nextIncludeSpace ();
			GVContainer.composer = sc.nextIncludeSpace ();
			GVContainer.designer = sc.nextIncludeSpace ();
			GVContainer.offset = sc.nextInt ();
			GVContainer.demoStart = sc.nextFloat (); //デモプレイ
			GVContainer.speed = sc.nextFloat ();
			GVContainer.displayBPM = sc.next (); //表示BPM
			GVContainer.bpm[0] = sc.nextFloat ();
			GVContainer.bpm[1] = sc.nextInt ();
			GVContainer.difficulty[0] = sc.next ();
			GVContainer.difficulty[1] = sc.next ();
			GVContainer.difficulty[2] = sc.next ();
			GVContainer.difficulty[3] = sc.next ();

			BPMInputField.text = GVContainer.bpm[0].ToString ();
			measureInputField.text = GVContainer.bpm[1].ToString ();
			offsetInputField.text = GVContainer.offset.ToString ();
			quantizeInputField.text = "4";
			speedInputField.text = GVContainer.speed.ToString ();
			demoStartInputField.text = GVContainer.demoStart.ToString ();
			difficultyInputField.text = GVContainer.difficulty[difficulty];
			displaySongNameInputField.text = GVContainer.displayName;
			displayBPMInputField.text = GVContainer.displayBPM;
			mapperInputField.text = GVContainer.designer;
			composerInputField.text = GVContainer.composer;

			GVContainer.offset = 0;
			GVContainer.speed = 5;

			applyValues ();
		}
	}
	IEnumerator inputMusicFile (string name) {
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip ("file:///" + Application.dataPath + "/Musics/" + name + ".ogg", AudioType.OGGVORBIS)) {
			yield return www.SendWebRequest ();

			try {
				audioSource.clip = DownloadHandlerAudioClip.GetContent (www);
				audioSource.Play ();
				audioSource.pitch = 0f;
				applyValues ();
			} catch (InvalidOperationException) { }
		}
	}

	public void onSaveAction () {
		try {
			string path = Application.dataPath.Replace ("/", "\\") + "\\Musics\\" + songName + difficulty + ".amc";
			File.WriteAllText (path, noteMap.Count.ToString () + Environment.NewLine);
			for (int i = 0; i < noteMap.Count; i++) {
				string output = noteMap[i].toString (offset);
				output += Environment.NewLine;
				File.AppendAllText (path, output);
			}

			if (true) {
				path = Application.dataPath.Replace ("/", "\\") + "\\Musics\\" + songName + ".amp";
				if (displaySongNameInputField.text != "") {
					File.WriteAllText (path, displaySongNameInputField.text + Environment.NewLine);
				} else {
					File.WriteAllText (path, "-" + Environment.NewLine);
				}
				if (composerInputField.text != "") {
					File.AppendAllText (path, composerInputField.text + Environment.NewLine);
				} else {
					File.AppendAllText (path, "-" + Environment.NewLine);
				}
				if (mapperInputField.text != "") {
					File.AppendAllText (path, mapperInputField.text + Environment.NewLine);
				} else {
					File.AppendAllText (path, "-" + Environment.NewLine);
				}
				File.AppendAllText (path, offset.ToString () + Environment.NewLine);
				if (demoStartInputField.text != "") {
					File.AppendAllText (path, demoStartInputField.text + Environment.NewLine);
				} else {
					File.AppendAllText (path, offset.ToString () + Environment.NewLine);
				}
				if (speedInputField.text != "") {
					File.AppendAllText (path, speedInputField.text + Environment.NewLine);
				} else {
					File.AppendAllText (path, 10 + Environment.NewLine);
				}
				if (displayBPMInputField.text != "") {
					File.AppendAllText (path, displayBPMInputField.text + Environment.NewLine);
				} else {
					File.AppendAllText (path, BPM + Environment.NewLine);
				}
				File.AppendAllText (path, BPM.ToString () + " " + measure.ToString () + Environment.NewLine);
				for (int i = 0; i < 4; i++) {
					if (i == difficulty) {
						if (difficultyInputField.text != "") {
							File.AppendAllText (path, difficultyInputField.text + " ");
						} else if (GVContainer.difficulty[i] != "") {
							File.AppendAllText (path, GVContainer.difficulty[i] + " ");
						} else {
							File.AppendAllText (path, "0 ");
						}
					} else if (GVContainer.difficulty[i] != "") {
						File.AppendAllText (path, GVContainer.difficulty[i] + " ");
					} else {
						File.AppendAllText (path, "0 ");
					}
				}
			}

			saveText.text = "Save succeed";
		} catch (IOException) {
			saveText.text = "Save failed";
		}
	}

	public void onDifficultySelected () {
		difficulty = difficultySelector.value;
	}

	public void import () {
		noteMap = getMap (songName, difficulty);
		reGenerateAllNote (noteMap);
	}

	private List<NoteInfo> getMap (string fileName, int dif) {
		string path = Application.dataPath.Replace ("/", "\\") + "\\Musics\\" + songName + dif + ".amc";
		StreamReader sr = new StreamReader (path, Encoding.UTF8);
		Scanner sc = new Scanner (sr);
		int N = sc.nextInt ();
		List<NoteInfo> map = new List<NoteInfo> ();
		for (int i = 0; i < N; i++) {
			map.Add (new NoteInfo (sc));
			map[i].timing += (int) offset;
			for (int j = 0; j < map[i].args.Length; j++) {
				map[i].args[j] += offset;
			}
		}
		return map;
	}

	void generateBar () {
		// GameObject型の配列cubesに、"Bar"タグのついたオブジェクトをすべて格納
		GameObject[] cubes = GameObject.FindGameObjectsWithTag ("Bar");

		// GameObject型の変数cubeに、cubesの中身を順番に取り出す。
		// foreachは配列の要素の数だけループ。
		foreach (GameObject cube in cubes) {
			// 消す！
			Destroy (cube);
		}

		if (bpmChange[0] == null) {
			int beat = 0;
			barList.Clear ();
			for (float i = offset; i < audioSource.clip.length * 1000f; i += (60000f / BPM)) {
				//ノーツ召喚してデータ持たせる
				GameObject barObject;
				if (beat % measure != 0) {
					barObject = MonoBehaviour.Instantiate (smallBar) as GameObject;
				} else {
					barObject = MonoBehaviour.Instantiate (bar) as GameObject;
				}
				barObject.tag = "Bar";
				Note b = barObject.GetComponent<Note> (); //変数名b(一時変数だから…)でNoteスクリプトのインスタンス順次生成
				b.name = "Bar " + i;
				b.setNoteInfo (new NoteInfo ((int) i, GVContainer.speed), 0, b.name);
				barList.Add (i);
				beat++;
			}
			qtList.Clear ();
			for (float i = offset; i < audioSource.clip.length * 1000f; i += (60000f / BPM) / (float) quantize) {
				qtList.Add (i);
			}
		}
	}

	void keyEventCheck () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			if (audioSource != null && audioSource.clip != null) {
				if (audioSource.pitch == 0f) {
					audioSource.pitch = 1f;
				} else {
					audioSource.pitch = 0f;
					GVContainer.ms = barList.Nearest (GVContainer.ms);
					audioSource.time = GVContainer.ms / 1000f;
				}
			}
		}

		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			if (currentPos > 0) {
				if (currentPos != startLane + 1) {
					currentPos--;
				} else if (currentPos > 1) {
					currentPos -= 2;
				}
			}
			currentPositionMarker.transform.position = new Vector3 ((float) (currentPos - 8) / 4f, 0, -14.43f);
		}
		if (Input.GetKey (KeyCode.LeftArrow)) {
			inputTimer++;
			if (inputTimer > 30 && inputTimer % 2 == 0) {
				if (currentPos > 0) {
					if (currentPos != startLane + 1) {
						currentPos--;
					} else if (currentPos > 1) {
						currentPos -= 2;
					}
				}
				currentPositionMarker.transform.position = new Vector3 ((float) (currentPos - 8) / 4f, 0, -14.43f);
			}
		}
		if (Input.GetKeyUp (KeyCode.LeftArrow)) {
			inputTimer = 0;
		}

		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			if (currentPos < 16) {
				if (currentPos != startLane - 1) {
					currentPos++;
				} else if (currentPos < 15) {
					currentPos += 2;
				}
				currentPositionMarker.transform.position = new Vector3 ((float) (currentPos - 8) / 4f, 0, -14.43f);
			}
		}
		if (Input.GetKey (KeyCode.RightArrow)) {
			inputTimer++;
			if (inputTimer > 30 && inputTimer % 2 == 0) {
				if (currentPos < 16) {
					if (currentPos != startLane - 1) {
						currentPos++;
					} else if (currentPos < 15) {
						currentPos += 2;
					}
					currentPositionMarker.transform.position = new Vector3 ((float) (currentPos - 8) / 4f, 0, -14.43f);
				}
			}
		}
		if (Input.GetKeyUp (KeyCode.RightArrow)) {
			inputTimer = 0;
		}

		if (editMode != 1) {
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				if (audioSource.pitch == 0) {
					if (GVContainer.ms >= offset - 1) {
						if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
							GVContainer.ms += (60000f / BPM);
							GVContainer.ms = qtList.Nearest (GVContainer.ms);
							audioSource.time = GVContainer.ms / 1000f;
						} else if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl) || Input.GetKey (KeyCode.LeftCommand) || Input.GetKey (KeyCode.RightCommand)) {
							GVContainer.ms++;
							audioSource.time = GVContainer.ms / 1000f;
						} else {
							GVContainer.ms += (60000f / BPM) / quantize;
							GVContainer.ms = qtList.Nearest (GVContainer.ms);
							audioSource.time = GVContainer.ms / 1000f;
						}
					}
				}
			}
			if (Input.GetKey (KeyCode.UpArrow)) {
				inputTimer++;
				if (inputTimer > 30) {
					if (audioSource.pitch == 0) {
						if (GVContainer.ms >= offset - 1) {
							if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
								GVContainer.ms += (60000f / BPM);
								GVContainer.ms = qtList.Nearest (GVContainer.ms);
								audioSource.time = GVContainer.ms / 1000f;
							} else if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl) || Input.GetKey (KeyCode.LeftCommand) || Input.GetKey (KeyCode.RightCommand)) {
								GVContainer.ms++;
								audioSource.time = GVContainer.ms / 1000f;
							} else {
								GVContainer.ms += (60000f / BPM) / quantize;
								GVContainer.ms = qtList.Nearest (GVContainer.ms);
								audioSource.time = GVContainer.ms / 1000f;
							}
						}
					}
				}
			}
			if (Input.GetKeyUp (KeyCode.UpArrow)) {
				inputTimer = 0;
			}
			if (Input.GetKeyDown (KeyCode.DownArrow)) {
				if (audioSource.pitch == 0) {
					if (GVContainer.ms < audioSource.clip.length * 1000f) {
						if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
							GVContainer.ms -= (60000f / BPM);
							GVContainer.ms = qtList.Nearest (GVContainer.ms);
							audioSource.time = GVContainer.ms / 1000f;
						} else if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl) || Input.GetKey (KeyCode.LeftCommand) || Input.GetKey (KeyCode.RightCommand)) {
							GVContainer.ms--;
							audioSource.time = GVContainer.ms / 1000f;
						} else {
							GVContainer.ms -= (60000f / BPM) / quantize;
							GVContainer.ms = qtList.Nearest (GVContainer.ms);
							audioSource.time = GVContainer.ms / 1000f;
						}
					}
				}
			}
			if (Input.GetKey (KeyCode.DownArrow)) {
				inputTimer++;
				if (inputTimer > 30) {
					if (audioSource.pitch == 0) {
						if (GVContainer.ms < audioSource.clip.length * 1000f) {
							if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
								GVContainer.ms -= (60000f / BPM);
								GVContainer.ms = qtList.Nearest (GVContainer.ms);
								audioSource.time = GVContainer.ms / 1000f;
							} else if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl) || Input.GetKey (KeyCode.LeftCommand) || Input.GetKey (KeyCode.RightCommand)) {
								GVContainer.ms--;
								audioSource.time = GVContainer.ms / 1000f;
							} else {
								GVContainer.ms -= (60000f / BPM) / quantize;
								GVContainer.ms = qtList.Nearest (GVContainer.ms);
								audioSource.time = GVContainer.ms / 1000f;
							}
						}
					}
				}
			}
			if (Input.GetKeyUp (KeyCode.DownArrow)) {
				inputTimer = 0;
			}
			float scroll = Input.GetAxis ("Mouse ScrollWheel");
			if (scroll != 0) {
				GVContainer.ms += ((60000f / BPM) / quantize) * (scroll * 10f);
				GVContainer.ms = qtList.Nearest (GVContainer.ms);
				audioSource.time = GVContainer.ms / 1000f;
			}
		}

		if (Input.GetKeyDown (KeyCode.X)) {
			switch (kind) {
				case 0: //tap
					if (editMode == 0) {
						editMode = 1;
						kindSelector.interactable = false;
						lane.Add (currentPos);
						startLane = currentPos;
						startLaneMarker.transform.position = new Vector3 ((float) (startLane - 8) / 4f, 0, -14.43f);
						if (currentPos != 16) {
							currentPos++;
						} else {
							currentPos--;
						}
						currentPositionMarker.transform.position = new Vector3 ((float) (currentPos - 8) / 4f, 0, -14.43f);
					} else if (editMode == 1) {
						editMode = 0;
						kindSelector.interactable = true;
						lane.Add (currentPos);
						lane.Sort (new Comparator ());
						startLane = -1;
						startLaneMarker.transform.position = new Vector3 (0, 0, 0);
						currentEditing = new NoteInfo ((int) GVContainer.ms, kind, lane.ToArray (), new float[0]);
						noteMap.Add (currentEditing);
						generateNote (currentEditing);
						lane.Clear ();
					}
					break;
				case 1: //hold
					if (editMode == 0) {
						editMode = 1;
						kindSelector.interactable = false;
						lane.Add (currentPos);
						startLane = currentPos;
						startLaneMarker.transform.position = new Vector3 ((float) (startLane - 8) / 4f, 0, -14.43f);
						if (currentPos != 16) {
							currentPos++;
						} else {
							currentPos--;
						}
						currentPositionMarker.transform.position = new Vector3 ((float) (currentPos - 8) / 4f, 0, -14.43f);
					} else if (editMode == 1) {
						editMode = 2;
						lane.Add (currentPos);
						lane.Sort (new Comparator ());
						startLane = -1;
						startLaneMarker.transform.position = new Vector3 (0, 0, 0);
						currentEditing = new NoteInfo ((int) GVContainer.ms, 0, lane.ToArray (), new float[0]);
						generatePreviewNote (currentEditing);
					} else if (editMode == 2) {
						if (GVContainer.ms > currentEditing.timing) {
							editMode = 0;
							kindSelector.interactable = true;
							currentEditing = new NoteInfo (currentEditing.timing, kind, lane.ToArray (), new float[1] { GVContainer.ms });
							noteMap.Add (currentEditing);
							generateNote (currentEditing);
							lane.Clear ();
							args.Clear ();
						}
					}
					break;
				case 2: //slide
					if (editMode == 0) {
						editMode = 1;
						kindSelector.interactable = false;
						lane.Add (currentPos);
						startLane = currentPos;
						startLaneMarker.transform.position = new Vector3 ((float) (startLane - 8) / 4f, 0, -14.43f);
						currentEditing = new NoteInfo ((int) GVContainer.ms, 0, lane.ToArray (), new float[0]);
						if (currentPos != 16) {
							currentPos++;
						} else {
							currentPos--;
						}
						currentPositionMarker.transform.position = new Vector3 ((float) (currentPos - 8) / 4f, 0, -14.43f);
					} else if (editMode == 1) {
						editMode = 2;
						lane.Add (currentPos);
						lane.Sort (lane.Count - 2, 2, new Comparator ());
						startLane = -1;
						startLaneMarker.transform.position = new Vector3 (0, 0, 0);
						if (lane.Count > 2) {
							args.Add (GVContainer.ms);
						}
						currentEditing = new NoteInfo (currentEditing.timing, kind, lane.ToArray (), args.ToArray ());
						generatePreviewNote (currentEditing);
					} else if (editMode == 2) {
						if (GVContainer.ms > currentEditing.timing) {
							editMode = 1;
							lane.Add (currentPos);
							startLane = currentPos;
							startLaneMarker.transform.position = new Vector3 ((float) (startLane - 8) / 4f, 0, -14.43f);
							if (currentPos != 16) {
								currentPos++;
							} else {
								currentPos--;
							}
							currentPositionMarker.transform.position = new Vector3 ((float) (currentPos - 8) / 4f, 0, -14.43f);
						}
					}
					break;
				case 3: //A-Hold
					if (editMode == 0) {
						editMode = 2;
						kindSelector.interactable = false;
						lane.Add (currentPos);
						currentEditing = new NoteInfo ((int) GVContainer.ms, 0, lane.ToArray (), new float[0]);
						currentPositionMarker.transform.position = new Vector3 ((float) (currentPos - 8) / 4f, 0, -14.43f);
					} else if (editMode == 2) {
						lane.Add (currentPos);
						if (lane.Count > 1) {
							args.Add (GVContainer.ms);
						}
						currentEditing = new NoteInfo (currentEditing.timing, kind, lane.ToArray (), args.ToArray ());
						generatePreviewNote (currentEditing);
					}
					break;
			}
		}

		if (Input.GetKeyDown (KeyCode.Return)) {
			if (editMode == 2 && kind == 2) { //slide
				editMode = 0;
				kindSelector.interactable = true;
				startLane = -1;
				startLaneMarker.transform.position = new Vector3 (0, 0, 0);
				currentEditing = new NoteInfo (currentEditing.timing, kind, lane.ToArray (), args.ToArray ());
				noteMap.Add (currentEditing);
				Debug.Log (string.Join (", ", lane.ToArray ()));
				Debug.Log (string.Join (", ", args.ToArray ()));
				generateNote (currentEditing);
				lane.Clear ();
				args.Clear ();
			}
			if (editMode == 2 && kind == 3) { //A-hold
				editMode = 0;
				kindSelector.interactable = true;
				startLane = -1;
				startLaneMarker.transform.position = new Vector3 (0, 0, 0);
				currentEditing = new NoteInfo (currentEditing.timing, kind, lane.ToArray (), args.ToArray ());
				noteMap.Add (currentEditing);
				generateNote (currentEditing);
				lane.Clear ();
				args.Clear ();
			}
		}

		if (Input.GetKeyDown (KeyCode.Z)) {
			editMode = 0;
			kindSelector.interactable = true;
			lane.Clear ();
			args.Clear ();
			startLane = -1;
			startLaneMarker.transform.position = new Vector3 (0, 0, 0);
			currentEditing = null;
		}

		if (Input.GetKeyDown (KeyCode.Backspace)) {
			noteMap.Sort ((a, b) => a.timing - b.timing);
			if (noteMap.Count > 0) {
				int destroyIndex = BackKey (noteMap, GVContainer.ms - 5);
				if (destroyIndex >= 0) {
					Destroy (GameObject.Find ("Note " + String.Format ("{0:D4}", destroyIndex)));
					noteMap.RemoveAt (destroyIndex);
					reGenerateAllNote (noteMap);
				}
			}
		}
		if (Input.GetKeyDown (KeyCode.Delete)) {
			noteMap.Sort ((a, b) => a.timing - b.timing);
			if (noteMap.Count > 0) {
				int destroyIndex = DeleteKey (noteMap, GVContainer.ms - 5);
				if (destroyIndex >= 0) {
					Destroy (GameObject.Find ("Note " + String.Format ("{0:D4}", destroyIndex)));
					noteMap.RemoveAt (destroyIndex);
					reGenerateAllNote (noteMap);
				}
			}
		}

		if ((Input.GetKey (KeyCode.S)) && (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl) || Input.GetKey (KeyCode.LeftCommand) || Input.GetKey (KeyCode.RightCommand))) {
			onSaveAction ();
		}

	}

	private void generateNote (NoteInfo NI) {
		//ノーツ召喚してデータ持たせる
		GameObject note = MonoBehaviour.Instantiate (normalNote) as GameObject;
		Note n = note.GetComponent<Note> (); //変数名n(一時変数だから…)でNoteスクリプトのコンポーネント生成
		n.name = "Note " + String.Format ("{0:D4}", noteMap.Count);
		n.setNoteInfo (NI, 0, n.name);
		reGenerateAllNote (noteMap);
	}
	private void generateNote (NoteInfo NI, int i) {
		GameObject note = MonoBehaviour.Instantiate (normalNote) as GameObject;
		Note n = note.GetComponent<Note> ();
		n.name = "Note " + String.Format ("{0:D4}", i);
		n.setNoteInfo (NI, 0, n.name);
	}
	private void generatePreviewNote (NoteInfo NI) {
		GameObject previewNote = MonoBehaviour.Instantiate (normalNote) as GameObject;
		previewNote.tag = "PreviewNote";
		Note n = previewNote.GetComponent<Note> ();
		n.name = "PreviewNote";
		n.setNoteInfo (NI, 0, n.name);
	}

	private void reGenerateAllNote (List<NoteInfo> list) {
		GameObject[] notes = GameObject.FindGameObjectsWithTag ("Note");
		foreach (GameObject note in notes) {
			Destroy (note);
		}
		GameObject[] preview = GameObject.FindGameObjectsWithTag ("PreviewNote");
		foreach (GameObject pv in preview) {
			Destroy (pv);
		}
		noteMap.Sort ((a, b) => a.timing - b.timing);

		for (int i = 0; i < list.Count; i++) {
			generateNote (list[i], i);
			//Debug.Log (list[i].toDebugString ());
		}
	}

	private int BackKey (List<NoteInfo> map, float timing) {
		for (int i = 0; i < map.Count; i++) {
			if (map[i].timing < timing) {
				continue;
			} else {
				return i - 1;
			}
		}
		return map.Count - 1;
	}
	private int DeleteKey (List<NoteInfo> map, float timing) {
		for (int i = 0; i < map.Count; i++) {
			if (map[i].timing < timing) {
				continue;
			} else {
				return i;
			}
		}
		return -1;
	}
}

public class Comparator : IComparer<int> {
	// 二つのintを比較するためのメソッド
	public int Compare (int x, int y) {
		return x.CompareTo (y);
	}
}