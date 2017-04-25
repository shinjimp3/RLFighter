using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButtonCallback : MonoBehaviour {
	public UnityEngine.UI.Dropdown dp1;
	public UnityEngine.UI.Dropdown dp2;
	public EpisodeController episode_controller;

	public void OnButtonReleased(int result){
		int player1_num = dp1.value;
		int player2_num = dp2.value;
		episode_controller.SetEnvironmentAndPlayer (player1_num, player2_num);
		episode_controller.StartIteration ();
	}
}
