using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadButtonCallBack : MonoBehaviour {

	public EpisodeController episode_controller;
	private AgentPlayer1 agent_player1_red;
	private AgentPlayer2 agent_player2_red;
	private AgentPlayer1 agent_player1_green;
	private AgentPlayer2 agent_player2_green;
	private int player_type_red;
	private int player_type_green;

	void Start(){
		episode_controller = GameObject.Find ("EpisodeController").GetComponent<EpisodeController> ();
	}

	public void OnButtonReleased(int result){
		if (transform.name == "LoadButton1")
			episode_controller.LoadRed ();
		if (transform.name == "LoadButton2")
			episode_controller.LoadGreen ();
	}

}
