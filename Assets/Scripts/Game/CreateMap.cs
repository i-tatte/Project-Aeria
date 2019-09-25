using System.IO;
using UnityEngine; //System.IO.FileInfo, System.IO.StreamReader, System.IO.StreamWriter
using System; //Exception
using System.Text; //Encoding
using System.Collections;
using UnityEngine.Networking;

// ファイル読み込み
public class CreateMap : MonoBehaviour {
	private string guitxt = "";
	public GameObject noteObject;
	AudioSource audioSource;
	void Start () {
		audioSource = this.gameObject.GetComponent<AudioSource> ();
		createMap (GVContainer.songName);
	}

	// Update is called once per frame
	void Update () { }

	public void createMap (string name) {
		NoteInfo[] map = getMap (name);
		generateNote (map);
	}

	// 読み込んだ情報をGUIとして表示
	void showGUI () {
		GUI.TextArea (new Rect (5, 5, Screen.width, 50), guitxt);
	}

	private NoteInfo[] getMap (string fileName) {
		StreamReader sr = new StreamReader (Application.dataPath + "/Musics/" + fileName + ".amp", Encoding.UTF8);
		Scanner sc = new Scanner (sr);
		readAMP (fileName, sc);
		StreamReader sr2 = new StreamReader (Application.dataPath + "/Musics/" + fileName + GVContainer.stage + ".amc", Encoding.UTF8);
		Scanner sc2 = new Scanner (sr2);
		int N = sc2.nextInt ();
		NoteInfo[] map = new NoteInfo[N];
		for (int i = 0; i < N; i++) {
			map[i] = new NoteInfo (sc2);
		}
		return map;
	}

	private void generateNote (NoteInfo[] map) {
		double fullScore = fullScoreCalcurate (map);
		double score = 10000000f / fullScore;
		float[][] judgeTimes = new float[map.Length][];
		for (int i = 0; i < judgeTimes.Length; i++) {
			judgeTimes[i] = new float[] { GVContainer.judgeLine[3], GVContainer.judgeLine[3] };
		}
		//ジャグ配列初期化
		for (int i = 0; i < map.Length; i++) {
			int j = i;
			if (map[i].kind > 2) {
				continue;
			}
			while (j < map.Length && map[i].timing + (GVContainer.judgeLine[3] * 2) > map[j].timing) {
				if (map[j].kind > 2) {
					j++;
					continue;
				}
				if (map[j].timing - map[i].timing > 2) {
					if (isLaneOverlap (map[i], map[j])) {
						float tmp = map[j].timing - map[i].timing; //2ノーツ間のタイミングの差
						if (judgeTimes[i][1] == GVContainer.judgeLine[3]) {
							judgeTimes[i][1] = tmp / 2f;
							judgeTimes[j][0] = tmp / 2f;
						} else {
							judgeTimes[j][0] = map[j].timing - map[i].timing - judgeTimes[i][1];
						}
					}
				}
				j++;
			}
		}
		for (int i = 0; i < map.Length; i++) {
			//ノーツ召喚してデータ持たせる
			GameObject note = MonoBehaviour.Instantiate (noteObject) as GameObject;
			Note n = note.GetComponent<Note> (); //変数名n(一時変数だから…)でNoteスクリプトのインスタンス順次生成
			n.name = "Note " + String.Format ("{0:D4}", i);
			n.setNoteInfo (map[i], score, n.name, judgeTimes[i][0], judgeTimes[i][1]);
		}
		//オーディオ準備
		StartCoroutine (inputMusicFile (GVContainer.songName));
	}

	private void readAMP (string fileName, Scanner sc) {
		GVContainer.displayName = sc.nextIncludeSpace ();
		GVContainer.composer = sc.nextIncludeSpace ();
		GVContainer.designer = sc.nextIncludeSpace ();
		GVContainer.offset = sc.nextInt ();
		string _ = sc.next (); //デモプレイ
		GVContainer.speed = sc.nextFloat ();
		_ = sc.next (); //表示BPM
		GVContainer.bpm[0] = sc.nextFloat ();
		GVContainer.bpm[1] = sc.nextInt ();
		GVContainer.difficulty[0] = sc.next ();
		GVContainer.difficulty[1] = sc.next ();
		GVContainer.difficulty[2] = sc.next ();
		GVContainer.difficulty[3] = sc.next ();
	}

	private int fullScoreCalcurate (NoteInfo[] map) {
		int ret = 0;
		float[] bpm = GVContainer.bpm;
		for (int i = 0; i < map.Length; i++) {
			switch (map[i].kind) {
				case 0: //tap
				case 1: //hold
				case 2: //slide
				case 3: //A-hold
					ret++;
					break;
			}
		}
		return ret;
	}

	private bool isLaneOverlap (NoteInfo a, NoteInfo b) {
		if (a.lane[1] > b.lane[0]) {
			if (a.lane[0] < b.lane[1]) {
				return true;
			}
		}
		return false;
	}

	IEnumerator inputMusicFile (string name) {
		string path = Application.dataPath.Replace ("/", "\\") + "\\Musics\\" + name + ".ogg";
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip (path, AudioType.OGGVORBIS)) {
			yield return www.SendWebRequest ();

			try {
				audioSource.clip = DownloadHandlerAudioClip.GetContent (www);
				audioSource.Play ();
				audioSource.pitch = 0;
			} catch (InvalidOperationException m) {
				Debug.Log (m.Message);
			}
		}
	}
}