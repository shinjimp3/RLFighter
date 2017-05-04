using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveButtonCallBack : MonoBehaviour {
	public EpisodeController episode_controller;
	private AgentPlayer1 agent_player1_red;
	private AgentPlayer2 agent_player2_red;
	private AgentPlayer1 agent_player1_green;
	private AgentPlayer2 agent_player2_green;
	private int player_type_red;
	private int player_type_green;

	void Start(){
		episode_controller = GameObject.Find ("EpisodeController").GetComponent<EpisodeController> ();
		agent_player1_red = episode_controller.GetPlayer1Ref (true);
		agent_player2_red = episode_controller.GetPlayer2Ref (true);
		agent_player1_green = episode_controller.GetPlayer1Ref (false);
		agent_player2_green = episode_controller.GetPlayer2Ref (false);
		Debug.Log (agent_player1_red == null);
	}

	public void OnButtonReleased(int result){
		if (transform.name == "SaveButton1")
			episode_controller.SaveRed();
		if (transform.name == "SaveButton2")
			episode_controller.SaveGreen ();
	}

}
