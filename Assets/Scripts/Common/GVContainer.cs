public static class GVContainer {
	/*
	グローバル変数(Global Variable : GV)を格納する。ここ以外でGVを宣言するのは禁止。
	出来るだけわかりやすい名前を付けること。
	それぞれの変数を宣言するときにその下にコメントで説明を付けることを義務づける。
	*/

	public static string songName = "yur";
	//曲名(システムネーム)。表示名じゃなくてファイル名の方ね。
	public static int stage = 3;
	//楽曲のステージ(4段階)。Easy:0,Normal:1,Hard:2,Lunatic:3.
	public static string displayName = "";
	//曲名の表示名。
	public static string composer = "";
	//作曲者名。
	public static string designer = "";
	//マップ作者名。
	public static int offset = 0;
	//開始までのオフセット。
	public static float demoStart = 0;
	//デモプレイの開始時間
	public static float speed = 0;
	//ノーツの流れるスピード。実際にここをゲーム中に参照するわけではなく、生成時に使う。
	public static float globalSpeed = 1;
	public static string[] difficulty = new string[4] { "", "", "", "" };
	//難易度。
	public static float ms = 0;
	//経過時間[ms](Editorでは現在の編集点のms)
	public static double score = 0;
	//言わずもがな、楽曲の現時点でのスコア。表示は整数だけど内部は浮動小数。
	public static int[] results = new int[4] { 0, 0, 0, 0 };
	public static int combo = 0;
	//現時点での楽曲のコンボ数。
	public static int maxCombo = 0;
	//現時点での最高コンボ数
	public static string displayBPM = "";
	public static float[] bpm = new float[2] { 60, 4 };
	//現在のBPMと拍子。bpm[0]=bpm,bpm[1]=1/N拍子を表す。
	public static int[] judgeLine = new int[] { 80, 120, 150, 150 };
	//判定基準[ms]。前から順にP,G,F,Lの閾値。
	public static string[] difficultiesName = new string[] { "Easy", "Normal", "Hard", "Lunatic" };
	public static int[] signal = new int[16];
	//グラウンドセンサーの入力信号
	public static int[] lightingSignal = new int[16];
	//地面を光らせるための信号。mod 4をとって経過時間を計る。
	//これが0以外であればsignal[]の更新が拘束される。←いいえ。巻き込み防止は別に実装しました。
	public static double[] airSignal = new double[6];
	//エアリアルセンサーの入力信号
}