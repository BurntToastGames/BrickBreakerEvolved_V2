using UnityEngine;
using System.Collections;

public class ResetPlayerWins : MonoBehaviour {

	// Use this for initialization
	void Start () {
		PlayerPrefs.SetInt ("Player1Wins", 0);
		PlayerPrefs.SetInt ("Player2Wins", 0);

		Time.timeScale = 1f;
	}
	
	// Update is called once per frame
	void Update () 
	{
			
	}
}
