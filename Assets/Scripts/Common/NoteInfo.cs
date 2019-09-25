public class NoteInfo {
	public int timing = 0;
	public int kind = 0;
	public float speed = 0;
	public int[] lane;
	public float[] args;
	public NoteInfo (Scanner sc) {
		timing = sc.nextInt ();
		kind = sc.nextInt ();
		if (kind < 5) {
			speed = GVContainer.speed * GVContainer.globalSpeed;
			int lane_num = sc.nextInt ();
			lane = sc.arrayInt (lane_num);
			int arg_num = sc.nextInt ();
			args = sc.arrayFloat (arg_num);
		} else {
			switch (kind) {
				case 5:
					GVContainer.speed = sc.nextFloat ();
					break;
				case 6:
					GVContainer.bpm = sc.arrayFloat (2);
					break;
				case 7:
					//Colour Change!!
					break;
			}
		}
	}

	public NoteInfo (int t, float s) {
		//小節線
		timing = t;
		kind = 8;
		speed = s;
		lane = new int[] {-2, 2 };
		args = new float[] { };
	}

	public NoteInfo (int t, int k, int[] l, float[] a) {
		timing = t;
		kind = k;
		speed = GVContainer.speed;
		lane = l;
		args = a;
	}

	public string toDebugString () {
		string ret = "";
		ret += "timing:" + timing;
		ret += ",kind:" + kind;
		ret += ",speed:" + speed;
		ret += ",lane:[" + string.Join (", ", lane) + "]";
		ret += ",args:[" + string.Join (", ", args) + "]";
		return ret;
	}

	public string toString (float offset) {
		string ret = "";
		ret += (timing - offset) + " ";
		ret += kind + " ";
		ret += lane.Length + " ";
		for (int i = 0; i < lane.Length; i++) {
			ret += lane[i] + " ";
		}
		ret += args.Length + " ";
		for (int i = 0; i < args.Length; i++) {
			if (kind < 5) {
				ret += (args[i] - offset);
			} else {
				ret += args[i];
			}
			if (i != args.Length - 1) {
				ret += " ";
			}
		}
		return ret;
	}
}