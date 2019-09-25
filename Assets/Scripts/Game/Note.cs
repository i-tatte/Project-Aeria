using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Note : MonoBehaviour {
	NoteInfo noteInfo;
	public GameObject meshGenerator;
	public Material slideNormalMaterial, slideLostMaterial, aerialNormalMaterial, aerialLostMaterial;
	string objectName = "";
	double score;
	double addedScore = 0;
	bool judged = false;
	//(一番最初にくるノーツが)判定されたか否か
	bool judging = false;
	//判定中か否か。Lostでも判定中(継続してLost出てる)ならtrue
	float[] judgeLine;
	int judgeNum = 0;
	//判定された回数。ホールド系ノーツで使う。
	int slideJudgeNum = 0;
	//スライドで変曲点を超えた回数。
	float releaseTime = 0;
	//スライドから手が離された時間。
	//「一瞬離されても大丈夫」を実現するために、(GVContainer.ms - releaseTime)の値が一定以下なら見逃す。
	float allowedSlideReleaseTime = 250;
	//上の「一定」の値。
	bool realJudge = true;
	int slideIdealPoint = 0;
	//スライドをすべてPrefectで抜けたときの得点加算回数
	double slidePoint = 0;
	//実際に加算されるポイント。slidePoint / slideIdealpoint * point が加算点数となる。
	int slidePrime = 3187;
	//スライド中に暫定加算される素数。下1桁が1とか9とかだと得点加算のランダム感がなくなるのでNG
	const int lighting = 320;
	//lightingSignalの初期化数値(4n必須)
	string result = "";
	// Perfect / Great / Fair / Lost
	bool hidden = false;
	bool editSound = false;
	AudioSource audioSource;
	Vector3 origin;
	GameObject[] generators = new GameObject[0];
	bool auto = false;
	void Start () {
		audioSource = this.gameObject.GetComponent<AudioSource> ();
		audioSource.clip = Resources.Load<AudioClip> ("Sounds/tap");
	}

	void Update () {
		//transform.position += transform.rotation * (new Vector3 (0, -noteInfo.speed * Time.deltaTime, 0));
		move ();
		editJudge ();
		if (SceneManager.GetActiveScene ().name == "GameScene") {
			judge ();
			hide ();
		}
		if (Input.GetKeyDown (KeyCode.A)) {
			auto = !auto;
		}
	}

	public void setNoteInfo (NoteInfo ni, double sc, string name, float judgeTime1, float judgeTime2) {
		noteInfo = ni;
		score = sc;
		objectName = name;
		noteInfo.timing += GVContainer.offset;
		if (noteInfo.kind == 1 || noteInfo.kind == 2 || noteInfo.kind == 3) {
			for (int i = 0; i < noteInfo.args.Length; i++) {
				noteInfo.args[i] += GVContainer.offset;
			}
			generators = new GameObject[noteInfo.args.Length];
		}
		initialize (); //初期配置とか
		if (noteInfo.kind < 3 || noteInfo.kind > 4) {
			origin = new Vector3 (transform.position.x, -4.63f, -14.42259f);
		} else {
			origin = new Vector3 (transform.position.x, -2.75f, -14.42259f);
		}
		judgeLine = new float[] { judgeTime1, judgeTime2 };
		//Debug.Log (noteInfo.toDebugString ());
	}
	public void setNoteInfo (NoteInfo ni, double sc, string name) {
		noteInfo = ni;
		score = sc;
		objectName = name;
		noteInfo.timing += GVContainer.offset;
		if (noteInfo.kind == 1 || noteInfo.kind == 2 || noteInfo.kind == 3) {
			for (int i = 0; i < noteInfo.args.Length; i++) {
				noteInfo.args[i] += GVContainer.offset;
			}
			generators = new GameObject[noteInfo.args.Length];
		}
		initialize (); //初期配置とか
		if (noteInfo.kind < 3 || noteInfo.kind > 4) {
			origin = new Vector3 (transform.position.x, -4.63f, -14.42259f);
		} else {
			origin = new Vector3 (transform.position.x, -2.75f, -14.42259f);
		}
		//Debug.Log (noteInfo.toDebugString ());
	}

	private void initialize () {
		//配置
		transform.position += transform.rotation * (new Vector3 (0, noteInfo.timing * noteInfo.speed / 1000f, 0));
		transform.position += new Vector3 ((float) (noteInfo.lane[1] + noteInfo.lane[0]) / 8.0f, 0, 0);
		//大きさ
		transform.localScale = new Vector3 ((float) (noteInfo.lane[1] - noteInfo.lane[0]) / 4f, 0.5f, 1);
		switch (noteInfo.kind) {
			case 1: //hold
				GenerateSlideMesh (noteInfo.timing, noteInfo.lane[0], noteInfo.lane[1],
					noteInfo.args[0], noteInfo.lane[0], noteInfo.lane[1], 0);
				break;
			case 2: //slide
				for (int i = 0; i < noteInfo.args.Length; i++) {
					if (i == 0) {
						GenerateSlideMesh (noteInfo.timing, noteInfo.lane[0], noteInfo.lane[1],
							noteInfo.args[0], noteInfo.lane[2], noteInfo.lane[3], i);
					} else {
						int j = 2 * i;
						GenerateSlideMesh (noteInfo.args[i - 1], noteInfo.lane[j], noteInfo.lane[j + 1],
							noteInfo.args[i], noteInfo.lane[j + 2], noteInfo.lane[j + 3], i);
					}
				}
				break;
			case 3: //A.Hold
				//隠す
				transform.GetChild (0).gameObject.GetComponent<MeshRenderer> ().enabled = false;
				//エアリアルメッシュ生成
				for (int i = 0; i < noteInfo.args.Length; i++) {
					if (i == 0) {
						GenerateAerialMesh (noteInfo.timing, noteInfo.lane[0], noteInfo.args[0], noteInfo.lane[1], i);
					} else {
						GenerateAerialMesh (noteInfo.args[i - 1], noteInfo.lane[i], noteInfo.args[i], noteInfo.lane[i + 1], i);
					}
				}
				releaseTime = noteInfo.timing;
				break;
			case 8:
				transform.localScale = new Vector3 (1, 0.05f, 1);
				//noteInfo.timing -= 50;
				break;
		}
	}

	private void move () {
		if (!hidden) {
			transform.position = origin + transform.rotation * (new Vector3 (0, (noteInfo.speed * (noteInfo.timing - GVContainer.ms)) / 1000f, 0));
			for (int i = 0; i < generators.Length; i++) {
				generators[i].transform.position = transform.position;
			}
		}
	}

	private void judge () {
		switch (noteInfo.kind) {
			case 0: //tap
				//判定済でなく、判定ラインにいるか?
				if (!judged && isInJudgeLine ()) {
					scoreCalc ("tap");
					//ヒント : scoreCalcではjudgedの更新も行われる。
				}
				//判定ラインを何事もなく通り過ぎた場合
				if (!judged && GVContainer.ms - noteInfo.timing > (GVContainer.judgeLine[2]) &&
					GVContainer.ms - noteInfo.timing > judgeLine[1]) {
					scoreCalc ("tap");
				}
				break;
			case 1: //hold
				//判定済でなく、判定ラインにいるか?
				if (!judged && isInJudgeLine ()) {
					scoreCalc ("hold-on");
				}
				//判定ラインを何事もなく通り過ぎた場合
				if (!judged && GVContainer.ms - noteInfo.timing > (GVContainer.judgeLine[2]) &&
					GVContainer.ms > noteInfo.timing + judgeLine[1]) {
					scoreCalc ("hold-on");
				}
				//ホールド中の得点増加
				if (judged && judging) {
					//ホールド範囲内か?
					if (isInHoldLine_Timing ()) {
						//範囲内のボタンがどれか反応しているか?
						if (isInHoldLine_Lane (noteInfo.lane[0], noteInfo.lane[1])) {
							result = "Perfect";
							for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
								GVContainer.lightingSignal[i] = lighting;
							}
							if (GVContainer.ms - noteInfo.timing > (float) judgeNum * 30000.0f / GVContainer.bpm[0]) {
								//半拍 = 30/BPM[s]
								scoreCalc ("hold-in");
							}
						} else {
							for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
								GVContainer.lightingSignal[i] = lighting + 3;
							}
							result = "Lost";
							if (GVContainer.ms - noteInfo.timing > (float) judgeNum * 30000.0f / GVContainer.bpm[0]) {
								//離されてる時も半拍ごとに評価
								scoreCalc ("hold-in");
							}
						}
					}
					//ホールドの終点
					if (GVContainer.ms > noteInfo.args[0]) {
						scoreCalc ("hold-out");
					}
				}
				break;
			case 2: //slide
				//判定済でなく、判定ラインにいるか?
				if (!judged && isInJudgeLine ()) {
					scoreCalc ("slide-on");
				}
				//判定ラインを何事もなく通り過ぎた場合
				if (!judged && GVContainer.ms - noteInfo.timing > (GVContainer.judgeLine[2]) &&
					GVContainer.ms > noteInfo.timing + judgeLine[1]) {
					scoreCalc ("slide-on");
				}
				//スライド中の得点増加
				if (judged && judging) {
					if (isInHoldLine_Timing ()) {
						float[] flane = getSlideJudgeLane ();
						int[] lane = new int[] {
							(int) flane[0], (int) flane[1]
						};
						//この区間での判定可能レーン
						//範囲内のボタンがどれか反応しているか?
						if (isInHoldLine_Lane (lane[0], lane[1])) {
							result = "Perfect";
							for (int i = lane[0]; i < lane[1]; i++) {
								GVContainer.lightingSignal[i] = lighting;
							}
							if (GVContainer.ms > noteInfo.args[slideJudgeNum]) {
								//変曲点を過ぎてる
								scoreCalc ("slide-connect");
							}
							if (GVContainer.ms - noteInfo.timing > (float) judgeNum * 30000.0f / GVContainer.bpm[0]) {
								//半拍 = 30/BPM[s]
								scoreCalc ("slide-in");
							}
						} else {
							result = "Lost";
							for (int i = lane[0]; i < lane[1]; i++) {
								GVContainer.lightingSignal[i] = lighting + 3;
							}
							if (GVContainer.ms > noteInfo.args[slideJudgeNum]) {
								scoreCalc ("slide-connect");
							}
							if (GVContainer.ms - noteInfo.timing > (float) judgeNum * 30000.0f / GVContainer.bpm[0]) {
								scoreCalc ("slide-in");
							}
						}
					}
					//スライドの終点
					if (GVContainer.ms > noteInfo.args[noteInfo.args.Length - 1]) {
						scoreCalc ("slide-out");
					}
				}
				break;
			case 3: //aerial hold
				if (noteInfo.args[noteInfo.args.Length - 1] - noteInfo.timing < 2) {
					if (!judged && isInAerialClickLine_Timing ()) {
						if (isInAerialClickLine_Lane ()) {
							result = "Perfect";
							scoreCalc ("ah-click");
						}
					}
					//判定ラインを何事もなく通り過ぎた場合
					if (!judged && GVContainer.ms - noteInfo.timing > GVContainer.judgeLine[2]) {
						result = "Lost";
						scoreCalc ("ah-click");
					}
				} else {
					if (isInAerialLine_Timing ()) {
						float flane = getSlideJudgeLane () [0];
						if (isInAerialLine_Lane (flane)) {
							if (GVContainer.ms - noteInfo.timing > (float) judgeNum * 30000.0f / GVContainer.bpm[0]) {
								//半拍 = 30/BPM[s]
								result = "Perfect";
								scoreCalc ("ah-in");
							}
						} else if (GVContainer.ms - noteInfo.timing > (float) judgeNum * 30000.0f / GVContainer.bpm[0]) {
							result = "Lost";
							scoreCalc ("ah-in");
						}
					}
					if (GVContainer.ms > noteInfo.args[noteInfo.args.Length - 1] && judging) {
						scoreCalc ("ah-out");
					}
				}
				break;
			case 4: //aerial flick
				break;
			case 5: //command
				break;
		}
	}

	private void scoreCalc (string kind) {
		judged = true;
		int lag = Math.Abs ((int) (GVContainer.ms) - noteInfo.timing); //ズレ[ms]
		switch (kind) {
			case "tap":
				if (lag > GVContainer.judgeLine[2]) { //Lost
					GVContainer.combo = 0;
					result = "Lost";
					GVContainer.results[3] += 1;
					for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
						GVContainer.lightingSignal[i] = lighting + 3;
					}
				} else {
					audioSource.Play ();
					GVContainer.combo++;
					if (GVContainer.combo > GVContainer.maxCombo) GVContainer.maxCombo++;
					if (lag > GVContainer.judgeLine[1]) { //Fair
						GVContainer.score += score / 3.0f;
						result = "Fair";
						GVContainer.results[2] += 1;
						for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
							GVContainer.lightingSignal[i] = lighting + 2;
						}
					} else if (lag > GVContainer.judgeLine[0]) { //Great
						GVContainer.score += score * 0.8f;
						result = "Great";
						GVContainer.results[1] += 1;
						for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
							GVContainer.lightingSignal[i] = lighting + 1;
						}
					} else { //Perfect
						GVContainer.score += score;
						result = "Perfect";
						GVContainer.results[0] += 1;
						for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
							GVContainer.lightingSignal[i] = lighting;
						}
					}
				}
				break;

			case "hold-on": //ホールド開始
				slideIdealPoint += 4;
				judging = true;
				if (lag > GVContainer.judgeLine[2]) { //Lost
					GVContainer.combo = 0;
					result = "Lost";
					GVContainer.results[3] += 1;
					for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
						GVContainer.lightingSignal[i] = lighting + 3;
					}
				} else {
					GVContainer.combo++;
					if (GVContainer.combo > GVContainer.maxCombo) GVContainer.maxCombo++;
					judgeNum++;
					audioSource.Play ();
					if (lag > GVContainer.judgeLine[1]) { //Fair
						GVContainer.score += score / 6f;
						addedScore = score / 6f;
						slidePoint += (4f / 3f);
						result = "Fair";
						GVContainer.results[2] += 1;
						for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
							GVContainer.lightingSignal[i] = lighting + 2;
						}
					} else if (lag > GVContainer.judgeLine[0]) { //Great
						GVContainer.score += score * 0.4f;
						addedScore = score * 0.4f;
						slidePoint += 3.2f;
						result = "Great";
						GVContainer.results[1] += 1;
						for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
							GVContainer.lightingSignal[i] = lighting + 1;
						}
					} else { //Perfect
						GVContainer.score += score / 2f;
						addedScore = score / 2f;
						slidePoint += 4;
						result = "Perfect";
						GVContainer.results[0] += 1;
						for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
							GVContainer.lightingSignal[i] = lighting;
						}
					}
				}
				break;
			case "hold-in": //ホールド中
				slideIdealPoint++;
				judgeNum++;
				if (result != "Lost") {
					GVContainer.score += slidePrime;
					addedScore += slidePrime;
					slidePoint++;
					GVContainer.combo++;
					if (GVContainer.combo > GVContainer.maxCombo) GVContainer.maxCombo++;
					GVContainer.results[0] += 1;
					judgeNum++;
					result = "Perfect";
					for (int i = 0; i < generators.Length; i++) {
						generators[i].GetComponent<Renderer> ().material = slideNormalMaterial;
					}
					if (realJudge) releaseTime = GVContainer.ms;
				} else {
					GVContainer.combo = 0;
					GVContainer.results[3] += 1;
					for (int i = 0; i < generators.Length; i++) {
						generators[i].GetComponent<Renderer> ().material = slideLostMaterial;
					}
				}
				break;
			case "hold-out": //終点
				slideIdealPoint += 4;
				judging = false;
				if (result != "Lost") {
					slidePoint += 4;
					for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
						GVContainer.lightingSignal[i] = lighting;
					}
					GVContainer.combo++;
					if (GVContainer.combo > GVContainer.maxCombo) GVContainer.maxCombo++;
					GVContainer.results[0] += 1;
				} else {
					for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
						GVContainer.lightingSignal[i] = lighting + 3;
					}
					GVContainer.combo = 0;
					GVContainer.results[3] += 1;
				}
				GVContainer.score -= addedScore;
				GVContainer.score += (slidePoint / (double) slideIdealPoint) * score;
				break;

			case "slide-on":
				slideIdealPoint += 4;
				judging = true;
				if (lag > GVContainer.judgeLine[2]) { //Lost
					GVContainer.combo = 0;
					result = "Lost";
					GVContainer.results[3] += 1;
					for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
						GVContainer.lightingSignal[i] = lighting + 3;
					}
				} else {
					GVContainer.combo++;
					if (GVContainer.combo > GVContainer.maxCombo) GVContainer.maxCombo++;
					judgeNum++;
					audioSource.Play ();
					if (lag > GVContainer.judgeLine[1]) { //Fair
						GVContainer.score += score / 6f;
						addedScore = score / 6f;
						slidePoint += (4f / 3f);
						result = "Fair";
						GVContainer.results[2] += 1;
						for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
							GVContainer.lightingSignal[i] = lighting + 2;
						}
					} else if (lag > GVContainer.judgeLine[0]) { //Great
						GVContainer.score += score * 0.4f;
						addedScore = score * 0.4f;
						slidePoint += 3.2f;
						result = "Great";
						GVContainer.results[1] += 1;
						for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
							GVContainer.lightingSignal[i] = lighting + 1;
						}
					} else { //Perfect
						GVContainer.score += score / 2f;
						addedScore = score / 2f;
						slidePoint += 4;
						result = "Perfect";
						GVContainer.results[0] += 1;
						for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
							GVContainer.lightingSignal[i] = lighting;
						}
					}
				}
				break;
			case "slide-in": //スライド中
				slideIdealPoint++;
				judgeNum++;
				if (result != "Lost") {
					GVContainer.score += slidePrime;
					addedScore += slidePrime;
					slidePoint++;
					GVContainer.combo++;
					if (GVContainer.combo > GVContainer.maxCombo) GVContainer.maxCombo++;
					GVContainer.results[0] += 1;
					result = "Perfect";
					for (int i = 0; i < generators.Length; i++) {
						generators[i].GetComponent<Renderer> ().material = slideNormalMaterial;
					}
					if (realJudge) releaseTime = GVContainer.ms;
				} else {
					GVContainer.combo = 0;
					GVContainer.results[3] += 1;
					for (int i = 0; i < generators.Length; i++) {
						generators[i].GetComponent<Renderer> ().material = slideLostMaterial;
					}
				}
				break;
			case "slide-connect": //変曲点
				slideJudgeNum++;
				break;
			case "slide-out": //終点
				slideIdealPoint += 4;
				judging = false;
				if (result != "Lost") {
					slidePoint += 4;
					for (int i = noteInfo.lane[noteInfo.lane.Length - 2]; i < noteInfo.lane[noteInfo.lane.Length - 1]; i++) {
						GVContainer.lightingSignal[i] = lighting;
					}
					GVContainer.combo++;
					if (GVContainer.combo > GVContainer.maxCombo) GVContainer.maxCombo++;
					GVContainer.results[0] += 1;
				} else {
					for (int i = noteInfo.lane[noteInfo.lane.Length - 2]; i < noteInfo.lane[noteInfo.lane.Length - 1]; i++) {
						GVContainer.lightingSignal[i] = lighting + 3;
					}
					GVContainer.combo = 0;
					GVContainer.results[3] += 1;
				}
				GVContainer.score -= addedScore;
				GVContainer.score += (slidePoint / (double) slideIdealPoint) * score;
				break;

			case "ah-click":
				if (result != "Lost") {
					GVContainer.score += score;
					GVContainer.combo++;
					if (GVContainer.combo > GVContainer.maxCombo) GVContainer.maxCombo++;
					GVContainer.results[0]++;
					audioSource.Play ();
				} else {
					GVContainer.combo = 0;
					GVContainer.results[3]++;
				}
				break;
			case "ah-in": //スライド中
				slideIdealPoint++;
				judgeNum++;
				judging = true;
				if (result != "Lost") {
					slidePoint++;
					GVContainer.score += slidePrime;
					addedScore += slidePrime;
					GVContainer.combo++;
					if (GVContainer.combo > GVContainer.maxCombo) GVContainer.maxCombo++;
					GVContainer.results[0] += 1;
					result = "Perfect";
					for (int i = 0; i < generators.Length; i++) {
						generators[i].GetComponent<Renderer> ().material = aerialNormalMaterial;
					}
					if (realJudge) releaseTime = GVContainer.ms;
				} else {
					GVContainer.combo = 0;
					GVContainer.results[3] += 1;
					//judging = false;
					for (int i = 0; i < generators.Length; i++) {
						generators[i].GetComponent<Renderer> ().material = aerialLostMaterial;
					}
				}
				break;
			case "ah-out":
				slideIdealPoint++;
				judging = false;
				if (result != "Lost") {
					slidePoint++;
					GVContainer.combo++;
					if (GVContainer.combo > GVContainer.maxCombo) GVContainer.maxCombo++;
					GVContainer.results[0] += 1;
				} else {
					GVContainer.combo = 0;
					GVContainer.results[3] += 1;
				}
				GVContainer.score -= addedScore;
				GVContainer.score += (slidePoint / (double) slideIdealPoint) * score;
				break;
		}
		Debug.Log (result + " " + kind + "\r\n" +
			"Combo : " + GVContainer.combo + "  Score : " + GVContainer.score + "\r\n" +
			"time : " + GVContainer.ms);
	}

	//ホールド、スライドなどでも始点だけの判定に使う。
	private bool isInJudgeLine () {
		if (auto && GVContainer.ms > noteInfo.timing) {
			return true;
		}
		//判定範囲内にいるか?
		if (!(Math.Abs (GVContainer.ms - noteInfo.timing) < GVContainer.judgeLine[2] && (noteInfo.timing - judgeLine[0] < GVContainer.ms && GVContainer.ms < noteInfo.timing + judgeLine[1]))) {
			return false;
		}
		bool inSlide = false;
		for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
			if (GVContainer.lightingSignal[i] > 0) {
				inSlide = true;
			}
		}
		if (inSlide && (!(Math.Abs (GVContainer.ms - noteInfo.timing) < GVContainer.judgeLine[0] && (noteInfo.timing - judgeLine[0] < GVContainer.ms && GVContainer.ms < noteInfo.timing + judgeLine[1])))) {
			return false;
		}
		//範囲内のボタンがどれか反応しているか?
		for (int i = noteInfo.lane[0]; i < noteInfo.lane[1]; i++) {
			if (GVContainer.signal[i] == 1) {
				return true;
			}
		}
		return false;
	}

	//ホールドはもちろん、スライドでも共用可能。タイミングだけでタップ場所の判定は行われない。
	private bool isInHoldLine_Timing () {
		return (noteInfo.timing < GVContainer.ms &&
			GVContainer.ms < noteInfo.args[noteInfo.args.Length - 1]);
	}

	private bool isInHoldLine_Lane (int left, int right) {
		if (auto) {
			return true;
		}
		for (int i = left; i < right; i++) {
			if (GVContainer.signal[i] != 0) {
				realJudge = true;
				return true;
			}
		}
		if (GVContainer.ms - releaseTime < allowedSlideReleaseTime) {
			realJudge = false;
			return true;
		}
		return false;
	}

	private bool isInAerialLine_Timing () {
		return (noteInfo.timing < GVContainer.ms &&
			GVContainer.ms < noteInfo.args[noteInfo.args.Length - 1]);
	}
	private bool isInAerialClickLine_Timing () {
		if (auto) {
			if (GVContainer.ms > noteInfo.timing) {
				return true;
			} else return false;
		} else {
			return (noteInfo.timing - GVContainer.judgeLine[0] < GVContainer.ms &&
				GVContainer.ms < noteInfo.timing + GVContainer.judgeLine[2]);
		}
	}

	private bool isInAerialLine_Lane (float lane) {
		if (auto) {
			return true;
		}
		for (int i = 0; i < 6; i++) {
			if (Math.Abs (GVContainer.airSignal[i] - lane) < 3.0) {
				realJudge = true;
				return true;
			}
		}
		if (GVContainer.ms - releaseTime < allowedSlideReleaseTime) {
			realJudge = false;
			return true;
		}
		return false;
	}
	private bool isInAerialClickLine_Lane () {
		if (auto) {
			return true;
		}
		for (int i = 0; i < 6; i++) {
			if (Math.Min (noteInfo.lane[0], noteInfo.lane[1]) - 2 < GVContainer.airSignal[i] &&
				GVContainer.airSignal[i] < Math.Max (noteInfo.lane[0], noteInfo.lane[1]) + 2) {
				return true;
			}
		}
		return false;
	}

	private float[] getSlideJudgeLane () {
		float[] ret;
		float progress = 0;
		int S = slideJudgeNum;
		if (S == 0) {
			progress = (GVContainer.ms - noteInfo.timing) / (noteInfo.args[0] - noteInfo.timing);
		} else {
			progress = (GVContainer.ms - noteInfo.args[S - 1]) / (noteInfo.args[S] - noteInfo.args[S - 1]);
		}
		switch (noteInfo.kind) {
			case 2:
				float left = (noteInfo.lane[2 * (S + 1)] - noteInfo.lane[2 * S]) * progress + noteInfo.lane[2 * S];
				float right = (noteInfo.lane[2 * (S + 1) + 1] - noteInfo.lane[2 * S + 1]) * progress + noteInfo.lane[2 * S + 1];
				ret = new float[] {
					Math.Max ((int) Math.Floor (left), 0), Math.Min ((int) Math.Ceiling (right), 16)
				};
				break;
			case 3:
				float center = (noteInfo.lane[S + 1] - noteInfo.lane[S]) * progress + noteInfo.lane[S];
				ret = new float[] {
					center
				};
				break;
			default:
				ret = new float[0];
				break;
		}
		return ret;
	}

	private void GenerateSlideMesh (float timing, float startL, float startR, float endTiming, float endL, float endR, int num) {
		GameObject meshGeneratorObject = Instantiate (meshGenerator, transform, true) as GameObject;
		meshGeneratorObject.transform.position = transform.position;
		MeshGenerator generator = meshGeneratorObject.gameObject.GetComponent<MeshGenerator> ();
		Vector3 v0, v1, v2, v3;
		float start = (timing - noteInfo.timing) * noteInfo.speed / 1000f;
		float end = (endTiming - noteInfo.timing) * noteInfo.speed / 1000f;
		float center = ((float) (noteInfo.lane[1] + noteInfo.lane[0])) / 2f;
		v0 = new Vector3 ((startL - center) / 4f, start, 0.0001f); //    end  
		v1 = new Vector3 ((startR - center) / 4f, start, 0.0001f); //  4-----3
		v2 = new Vector3 ((endR - center) / 4f, end, 0.0001f); //      |     |
		v3 = new Vector3 ((endL - center) / 4f, end, 0.0001f); //      |     |
		generator.generateQuadMesh (v0, v1, v2, v3); //          1-----2
		generators[num] = meshGeneratorObject; //                 start 
		generator.name = objectName + "-slideMesh" + String.Format ("{0:D4}", num);
	}

	private void GenerateAerialMesh (float startTiming, float start, float endTiming, float end, int num) {
		transform.position = new Vector3 (0, transform.position.y, transform.position.z);
		transform.localScale = new Vector3 (1, 1, 1);
		GameObject meshGeneratorObject = Instantiate (meshGenerator, transform, true) as GameObject;
		meshGeneratorObject.transform.localScale = new Vector3 (1, 1, 1);
		meshGeneratorObject.transform.position = new Vector3 (0, 0, 0);
		MeshGenerator generator = meshGeneratorObject.gameObject.GetComponent<MeshGenerator> ();
		Vector3 v0, v1;
		float startPos = (startTiming - noteInfo.timing) * noteInfo.speed / 1000f;
		float endPos = (endTiming - noteInfo.timing) * noteInfo.speed / 1000f;
		v0 = new Vector3 ((start - 8f) / 4f, startPos, 0);
		v1 = new Vector3 ((end - 8f) / 4f, endPos, 0);
		generator.generateAerialMesh (v0, v1);
		generators[num] = meshGeneratorObject;
		generator.name = objectName + "-aerialMesh" + String.Format ("{0:D4}", num);
	}

	private void hide () {
		switch (noteInfo.kind) {
			case 0:
				if (judged) {
					transform.GetChild (0).gameObject.GetComponent<MeshRenderer> ().enabled = false;
					hidden = true;
				}
				break;
			case 1:
			case 2:
			case 3:
				if (judged && !judging && GVContainer.ms > noteInfo.args[noteInfo.args.Length - 1]) {
					transform.GetChild (0).gameObject.GetComponent<MeshRenderer> ().enabled = false;
					hidden = true;
					for (int i = 0; i < generators.Length; i++) {
						generators[i].GetComponent<MeshRenderer> ().enabled = false;
						hidden = true;
					}
				}
				break;
		}
	}

	void editJudge () {
		if (noteInfo.kind <= 5) {
			if (noteInfo.timing > GVContainer.ms) {
				editSound = false;
			} else if (!editSound) {
				editSound = true;
				if (GVContainer.ms - noteInfo.timing < 50) {
					audioSource.Play ();
				}
			}
		}
	}

	public string toString () {
		string ret = "";
		ret += "timing : " + noteInfo.timing;
		ret += ", lane : [" + string.Join (", ", noteInfo.lane) + "]";
		ret += ", args : [" + string.Join (", ", noteInfo.args) + "]";
		return ret;
	}
}