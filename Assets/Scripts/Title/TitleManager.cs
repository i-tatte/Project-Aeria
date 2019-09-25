using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour {
	GameManager gm = new GameManager ();
	void Start () { }

	// Update is called once per frame
	void Update () {
		gm.getSignal ();
		Debug.Log (string.Join (" ", GVContainer.signal) + "\r\n" + string.Join (" ", GVContainer.airSignal));
	}
}