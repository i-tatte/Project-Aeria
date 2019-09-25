using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultAnimationController : MonoBehaviour {
	public Animator animator;
	// Start is called before the first frame update
	int phase = 0;
	void Start () {
		if (GVContainer.results[3] == 0) {
			animator.SetBool ("FC", true);
		} else {
			animator.SetBool ("FC", false);
		}
	}

	// Update is called once per frame
	void Update () {
		if (GVContainer.signal[1] == 1 || Input.GetKeyDown (KeyCode.Space)) {
			if (phase == 1) {
				animator.SetTrigger ("Skip");
				phase++;
			} else if (phase == 2) {
				animator.SetTrigger ("Exit");
			}
		}
	}
	void onRankDisplayed () {
		phase++;
	}
	public void onFadeOutEnd () {
		GVContainer.score = 0;
		GVContainer.combo = 0;
		GVContainer.maxCombo = 0;
		GVContainer.globalSpeed = 1;
		SceneManager.LoadScene ("SelectScene");
	}
}