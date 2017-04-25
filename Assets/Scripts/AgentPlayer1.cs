using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using MathNet.Numerics.LinearAlgebra.Double;

public class AgentPlayer1 : Player {
	private float[,] Qtable = new float[50,6];
	private string serializeDataPath;
	UnityEngine.UI.Text qvalue_text;

	public AgentPlayer1(bool isRed):base(isRed){		
		actions = new Actions ();
		this.isRed = isRed;

		Initialize ();
	}

	void Initialize(){
		serializeDataPath = Application.dataPath + "/Player1Qtable.xml";
		if (System.IO.File.Exists (serializeDataPath)) {
			//load Qtable
			Qtable = XmlUtil.Deserialize<float[,]> (serializeDataPath);
		} else {
			for (int i = 0; i < Qtable.GetLength (0); i++) {
				for (int j = 0; j < Qtable.GetLength (1); j++) {
					Qtable [i, j] = 1f;
					if (i % 5 == 1)
						Qtable [i, j] += 5f;
					if (i % 5 == 3 || i % 5 == 4)
						Qtable [i, j] -= 2f;
				}
			}
		}
		qvalue_text = GameObject.Find ("Text").GetComponent<UnityEngine.UI.Text> ();
		episode_started = false;
	}

	private int action_index;
	private bool episode_started;

	public Actions RunStep(States states){
		if (passed_time < step_width) {
			passed_time++;
			return actions;
		}

		next_obs_states = states;
		if (episode_started) {
			float reward = CalcReward (obs_states, actions, next_obs_states);
			Learn (obs_states, action_index, next_obs_states, reward);
		}
		episode_started = true;
		action_index = Policy (next_obs_states);
		actions = Index2Action (action_index);
		obs_states = next_obs_states;
		return actions;
	}

	private void Learn(States states, int action_i, States next_states, float reward){
		int state_i = State2Index (states);
		int next_state_i = State2Index (next_states);
		float alpha = 0.1f;
		float gamma = 0.99f;

		int action_num = Qtable.GetLength (1);
		float[] qvector = new float[action_num];
		for (int i = 0; i < qvector.Length; i++) {
			qvector [i] = Qtable [next_state_i, i];
		}
		Qtable [state_i, action_i] += alpha * (reward + gamma * Max (qvector) - Qtable [state_i, action_i]); 
	}

	private int Policy(States states){
		int state_i = State2Index (states);
		int action_i;
		int action_num = Qtable.GetLength (1);
		if (Random.value < 0.2f) {
			action_i = (int)((Random.value) * action_num);
		} else {
			float[] qvector = new float[action_num];
			qvalue_text.text = "";
			for (int i = 0; i < qvector.Length; i++) {
				qvector [i] = Qtable [state_i, i];
				qvalue_text.text += qvector[i] + ", ";
			}
			action_i = ArgMax (qvector);
		}
		return action_i;
	}

	private float CalcReward(States states, Actions actions, States next_states){
		float reward;
		reward = 10f*((states.HP2 - next_states.HP2) - (states.HP1 - next_states.HP1));
		if (!isRed)
			reward = -reward;
		return reward;
	}

	Actions Index2Action(int index){
		Actions ret_actions = new Actions ();
		ret_actions.yaw = (index % 3) * 0.5f;
		ret_actions.speed = (index - index % 3) / 3f;
		ret_actions.shoot = true;
		return ret_actions;
	}

	int State2Index(States states){
		float target_theta;
		float enemy_target_theta;
		int target_theta_i = 0;
		int enemy_target_theta_i = 0;
		int dist_i = 0;

		float[] tarm_theta = new float[]{ 80f, 100f, 180f, 270f };

		if (isRed) {
			target_theta = states.target_theta21;
			enemy_target_theta = states.target_theta12;
		}
		else{
			target_theta = states.target_theta12;
			enemy_target_theta = states.target_theta21;
		}

		for (int i = 0; i < tarm_theta.Length; i++) {
			if (target_theta > tarm_theta [i])
				target_theta_i++;
			if (enemy_target_theta > tarm_theta [i])
				enemy_target_theta_i++;
		}
		if (Vector2.Distance (states.pos1, states.pos2) > 100)
			dist_i = 1;

		return dist_i * (tarm_theta.Length + 1) * (tarm_theta.Length + 1) + enemy_target_theta_i * (tarm_theta.Length + 1) + target_theta_i;
	}

	int ArgMax(float [] vector){
		float max = -Mathf.Infinity;
		int max_i = 0;
		for (int i = 0; i < vector.Length; i++) {
			if (vector [i] > max) {
				max = vector [i];
				max_i = i;
			}
		}
		return max_i;
	}

	float Max(float [] vector){
		float max = -Mathf.Infinity;
		int max_i = 0;
		for (int i = 0; i < vector.Length; i++) {
			if (vector [i] > max) {
				max = vector [i];
				max_i = i;
			}
		}
		return max;
	}

	void OnApplicationQuit(){
		//save Qtable
		XmlUtil.Seialize<float[,]>(serializeDataPath, Qtable);
	}

}
