using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EpisodeController : MonoBehaviour {

	public StateViewer state_viewer;

	private bool iterating = false;
	private bool drawing = false;
	private bool training = false;

	private float time_passed = 0f;
	private float step_time_passed = 0f;
	private float step_width = 0.2f;
	private bool episode_end = false;
	private float loss_time = 3f;
	private float loss_time_passed;

	private Environment env;

	private int player_type_red;
	private int player_type_green;

	private RandomPlayer random_player_red;
	private HumanPlayer  human_player_red;
	private HandCodedPlayer hand_coded_player_red;
	private AgentPlayer1 agent_player1_red;
	private AgentPlayer2 agent_player2_red;

	private RandomPlayer random_player_green;
	private HumanPlayer  human_player_green;
	private HandCodedPlayer hand_coded_player_green;
	private AgentPlayer1 agent_player1_green;
	private AgentPlayer2 agent_player2_green;

	private States states;
	private Actions red_actions, green_actions;


	void Start(){
		
	}

	// Update is called once per frame
	void Update () {
		if (!iterating)
			return;

		if (Input.GetKeyUp(KeyCode.V)) {
			drawing = !drawing;
		}
			
		if (!drawing || step_time_passed > env.GetDt()) {
			if (!episode_end) {
				red_actions = PlayerRedRunStep (states);
				green_actions = PlayerGreenRunStep (states);
			}
			states = env.Run (red_actions, green_actions);
			step_time_passed = 0f;
			if ((states.HP1 <= 0 || states.HP2 <= 0 || 
				(!states.endless && states.bullet_num1 <= 0 && states.bullet_num2 <= 0))
				&&!episode_end) {
				episode_end = true;
				PlayerRedEndEpisode (states);
				PlayerGreenEndEpisode (states);
				state_viewer.ShowResult (states);
			}
		}
		if(drawing){
			state_viewer.Draw (states, red_actions, green_actions, env.GetBulletsPosAng (), step_time_passed/env.GetDt());

		}

		if (loss_time_passed > loss_time || (!drawing&&episode_end)) {
			env.Reset ();
			episode_end = false;
			loss_time_passed = 0f;
			state_viewer.ShowResult (env.initial_states);
		}

		if (episode_end)
			loss_time_passed += Time.deltaTime;
		step_time_passed += Time.deltaTime;
	}

	public void StartIteration(){
		iterating = true;
		drawing = true;
	}
		
	public void SetEnvironmentAndPlayer(int player_type_red, int player_type_green){
		env = new Environment ();
		states = env.states;
		red_actions = new Actions ();
		green_actions = new Actions ();

		this.player_type_red = player_type_red;
		this.player_type_green = player_type_green;

		random_player_red = new RandomPlayer (true);
		human_player_red = new HumanPlayer (true);
		hand_coded_player_red = new HandCodedPlayer (true);
		agent_player1_red = new AgentPlayer1 (true);
		agent_player2_red = new AgentPlayer2 (true);

		random_player_green = new RandomPlayer (false);
		human_player_green = new HumanPlayer (false);
		hand_coded_player_green = new HandCodedPlayer (false);
		agent_player1_green = new AgentPlayer1 (false);
		agent_player2_green = new AgentPlayer2 (false);
	}

	public Actions PlayerRedRunStep(States states){
		switch (player_type_red) {
		case 0:
			return random_player_red.RunStep (states);
		case 1:
			return human_player_red.RunStep (states);
		case 2:
			return hand_coded_player_red.RunStep (states);
		case 3:
			return agent_player1_red.RunStep (states);
		case 4:
			return agent_player2_red.RunStep (states);
		default :
			Debug.LogError ("Player red type settings are illegal.", transform);
			return new Actions();
		}
	}

	public Actions PlayerGreenRunStep(States states){
		switch (player_type_green) {
		case 0:
			return random_player_green.RunStep (states);
		case 1:
			return human_player_green.RunStep (states);
		case 2:
			return hand_coded_player_green.RunStep (states);
		case 3:
			return agent_player1_green.RunStep (states);
		case 4:
			return agent_player2_green.RunStep (states);
		default :
			Debug.LogError ("Player green type settings are illegal.", transform);
			return new Actions ();
		}
	}


	public void PlayerRedEndEpisode(States states){
		switch (player_type_red) {
		case 0:
			random_player_red.EndEpisode (states);
			break;
		case 1:
			human_player_red.EndEpisode (states);
			break;
		case 2:
			hand_coded_player_red.EndEpisode (states);
			break;
		case 3:
			agent_player1_red.EndEpisode (states);
			break;
		case 4:
			agent_player2_red.EndEpisode (states);
			break;
		default :
			Debug.LogError ("Player red type settings are illegal.", transform);
			break;
		}
	}

	public void PlayerGreenEndEpisode(States states){
		switch (player_type_green) {
		case 0:
			random_player_green.EndEpisode (states);
			break;
		case 1:
			human_player_green.EndEpisode (states);
			break;
		case 2:
			hand_coded_player_green.EndEpisode (states);
			break;
		case 3:
			agent_player1_green.EndEpisode (states);
			break;
		case 4:
			agent_player2_green.EndEpisode (states);
			break;
		default :
			Debug.LogError ("Player green type settings are illegal.", transform);
			break;
		}
	}
}

