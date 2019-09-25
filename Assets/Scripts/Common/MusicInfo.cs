using System.IO;
using System.Text;
using UnityEngine;

public class MusicInfo {
	public string fileName;
	public string displayName;
	public string composer;
	public string mapper;
	public int offset;
	public int demoStart;
	public string displayBPM;
	public string[] difficulties = new string[4];
	public MusicInfo (string name) {
		fileName = name;
		StreamReader sr = new StreamReader (Application.dataPath + "/Musics/" + fileName + ".amp", Encoding.UTF8);
		Scanner sc = new Scanner (sr);
		displayName = sc.nextIncludeSpace ();
		composer = sc.nextIncludeSpace ();
		mapper = sc.nextIncludeSpace ();
		offset = sc.nextInt ();
		demoStart = sc.nextInt ();
		float _ = sc.nextFloat ();
		displayBPM = sc.next ();
		_ = sc.nextFloat ();
		_ = sc.nextFloat ();
		for (int i = 0; i < 4; i++) {
			difficulties[i] = sc.next ();
		}
	}

	public override string ToString () {
		string ret = "";
		ret += "fileName : " + fileName;
		ret += ", displayName : " + displayName;
		ret += ", composer : " + composer;
		ret += ", mapper : " + mapper;
		ret += ", offset : " + offset;
		ret += ", demoStart : " + demoStart;
		ret += ", displayBPM : " + displayBPM;
		ret += ", difficluties : [" + string.Join (", ", difficulties) + "]";
		return ret;
	}
}