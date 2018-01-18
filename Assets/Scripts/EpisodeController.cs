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

	// Generate all players (agents) here and in public void SetEnvironmentAndPlayer(int player_type_red, int player_type_green)
	// but only selected players are controlled in private Actions PlayerRedRunStep(States states),
	//                                             private Actions PlayerGreenRunStep(States states),
	//                                             private void PlayerRedEndEpisode(States states),
	//                                             private void PlayerGreenEndEpisode(States states),
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

	private int score_red;
	private int score_green;
	UnityEngine.UI.Text score_red_text;
	UnityEngine.UI.Text score_green_text;


	void Start(){
		score_red_text =  GameObject.Find ("ScoreRed").GetComponent<UnityEngine.UI.Text>();
		score_green_text =  GameObject.Find ("ScoreGreen").GetComponent<UnityEngine.UI.Text>();
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
				if(states.HP2 <= 0){
					score_red++;
					score_red_text.text = "score:" + score_red;
				}
				if(states.HP1 <= 0){
					score_green++;
					score_green_text.text = "score:" + score_green;
				}
				episode_end = true;
				PlayerRedEndEpisode (states);
				PlayerGreenEndEpisode (states);
				if (drawing) {
					state_viewer.ShowResult (states);
				}
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
		Debug.Log (agent_player1_red == null);
		agent_player2_red = new AgentPlayer2 (true);

		random_player_green = new RandomPlayer (false);
		human_player_green = new HumanPlayer (false);
		hand_coded_player_green = new HandCodedPlayer (false);
		agent_player1_green = new AgentPlayer1 (false);
		agent_player2_green = new AgentPlayer2 (false);
	}

	private Actions PlayerRedRunStep(States states){
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

	private Actions PlayerGreenRunStep(States states){
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


	private void PlayerRedEndEpisode(States states){
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

	private void PlayerGreenEndEpisode(States states){
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

	public AgentPlayer1 GetPlayer1Ref(bool isRed){
		if (Time.time != 0f) {
			return new AgentPlayer1 (true);
		}
		if (isRed) {
			Debug.Log ("sansyou watatte ru.");
			return agent_player1_red;
		}
		else
			return agent_player1_green;
	}

	public AgentPlayer2 GetPlayer2Ref(bool isRed){
		if (Time.time != 0f) {
			return new AgentPlayer2 (true);
		}
		if (isRed)
			return agent_player2_red;
		else
			return agent_player2_green;
	}

	public int GetPlayerTypeRed(){
		return player_type_red;
	}

	public int GetPlayerTypeGreen(){
		return player_type_green;
	}

	public void SaveRed(){
		switch (player_type_red) {
		case 3:
			agent_player1_red.Save ();
			break;
		case 4:
			agent_player2_red.Save ();
			break;
		default:
			Debug.LogWarning ("No saved contents",transform);
			break;
		}
	}

	public void SaveGreen(){
		switch (player_type_green) {
		case 3:
			agent_player1_green.Save();
			break;
		case 4:
			agent_player2_green.Save ();
			break;
		default:
			Debug.LogWarning ("No saved contents",transform);
			break;
		}
	}

	public void LoadRed(){
		switch (player_type_red) {
		case 3:
			agent_player1_red.Load ();
			break;
		case 4:
			agent_player2_red.Load ();
			break;
		default:
			Debug.LogWarning ("No saved contents",transform);
			break;
		}
	}

	public void LoadGreen(){
		switch (player_type_green) {
		case 3:
			agent_player1_green.Load ();
			break;
		case 4:
			agent_player2_green.Load ();
			break;
		default:
			Debug.LogWarning ("No saved contents",transform);
			break;
		}
	}

}

