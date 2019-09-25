using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameAnimationController : MonoBehaviour {
	// Start is called before the first frame update
	public GameObject managerObject;
	AudioSource audioSource;
	public Animator animator;
	void Start () {
		audioSource = managerObject.GetComponent<AudioSource> ();
	}

	// Update is called once per frame
	void Update () {
		if (audioSource.clip != null && GVContainer.ms == 0 && !audioSource.isPlaying) {
			animator.SetBool ("FadeOut", true);
		}
	}
	void onFadeInEnd () {
		AudioSource source = managerObject.GetComponent<AudioSource> ();
		source.pitch = 1;
	}
	void onFadeOutEnd () {
		SceneManager.LoadScene ("ResultScene");
	}
}