using System;
using System.IO; //Exception

public class Scanner {
	string[] s;
	int i;
	StreamReader sr;

	char[] cs = new char[] { ' ' };

	public Scanner (StreamReader stread) {
		s = new string[0];
		i = 0;
		sr = stread;
	}

	public string next () {
		while (i >= s.Length) {
			string st = sr.ReadLine ();
			while (st == "") st = sr.ReadLine ();
			s = st.Split (cs, StringSplitOptions.RemoveEmptyEntries);
			i = 0;
		}
		return s[i++];
	}
	public string nextIncludeSpace () {
		string st = sr.ReadLine ();
		return st;
	}
	public int nextInt () {
		return int.Parse (next ());
	}
	public int[] arrayInt (int N) {
		int[] ret = new int[N];
		for (int i = 0; i < N; i++) {
			ret[i] = nextInt ();
		}
		return ret;
	}
	public long nextLong () {
		return long.Parse (next ());
	}
	public float nextFloat () {
		return float.Parse (next ());
	}
	public bool nextBool () {
		return bool.Parse (next ());
	}
	public float[] arrayFloat (int N) {
		float[] ret = new float[N];
		for (int i = 0; i < N; i++) {
			ret[i] = nextFloat ();
		}
		return ret;
	}
}