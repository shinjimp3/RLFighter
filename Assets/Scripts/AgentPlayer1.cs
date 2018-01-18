using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Windows;

//using MathNet.Numerics.LinearAlgebra.Double;

public class AgentPlayer1 : Player {
	private float[,] Qtable = new float[450,12];
	private string serializeDataPath;
	UnityEngine.UI.Text debug_text;
	private float reward_sum = 0f;
	private int action_index;
	private bool episode_started;
	private List<Experience> exbuff;
	private int buff_num = 50;

	public AgentPlayer1(bool isRed):base(isRed){		
		actions = new Actions ();
		this.isRed = isRed;
		Initialize ();
	}

	void Initialize(){
		step_width = 2;
		serializeDataPath = Application.dataPath + "Player1Qtable.xml";
		for (int i = 0; i < Qtable.GetLength (0); i++) {
			for (int j = 0; j < Qtable.GetLength (1); j++) {
				Qtable [i, j] = 1f;
				//if(i - i%3 - (i-i%3)/3 - (i - i%3 - (i-i%3)/3)/6 )
//					if (i % 5 == 1)
//						Qtable [i, j] += 5f;
//					if (i % 5 == 3 || i % 5 == 4)
//						Qtable [i, j] -= 2f;
			}
		}
		if(isRed)
			debug_text = GameObject.Find ("FreeText1").GetComponent<UnityEngine.UI.Text> ();
		else
			debug_text = GameObject.Find ("FreeText2").GetComponent<UnityEngine.UI.Text> ();
		episode_started = false;
		exbuff = new List<Experience> ();
	}

	public struct Experience{
		public Experience(int actions, States states, States next_states, float reward){
			this.actions = actions;
			this.states = states;
			this.next_states = next_states;
			this.reward = reward;
		}
		public int actions;//Actions actions;
		public States states;
		public States next_states;
		public float reward;
	}

	public Actions RunStep(States states){
		if (states.step_i%step_width!=0 && !states.isDamaged1 && !states.isDamaged2) {
			passed_time++;
			return actions;
		}

		next_obs_states = states;

		if (episode_started) {
			//if (State2Index (next_obs_states) == State2Index (obs_states) && Random.value < 0.9f) {
			//	reward_sum += CalcReward (obs_states, actions, next_obs_states);
			//	obs_states = next_obs_states;
			//	return actions;
			//}
			if (states.step_i % step_width == 0) {
				exbuff.Add (new Experience (action_index, obs_states, next_obs_states, reward));
				if (exbuff.Count > buff_num)
					exbuff.RemoveAt (0);
			}

			if (isRed) {
				int delay = (int)(next_obs_states.isDameged2Before / step_width);
				if (next_obs_states.isDamaged2 && delay < exbuff.Count) {
					debug_text.text = "isDB:" + states.isDameged2Before + "delay:" + delay + ", exbuff.Count:" + exbuff.Count;
					float r = CalcReward (obs_states, actions, next_obs_states);
					Learn (exbuff [exbuff.Count - delay - 1].states, exbuff [exbuff.Count - delay - 1].actions, exbuff [exbuff.Count - delay - 1].next_states, r);
				}
			} else {
				int delay = (int)(next_obs_states.isDameged1Before / step_width);
				if (next_obs_states.isDamaged1 && delay < exbuff.Count) {
					float r = CalcReward (obs_states, actions, next_obs_states);
					Learn (exbuff [exbuff.Count - delay - 1].states, exbuff [exbuff.Count - delay - 1].actions, exbuff [exbuff.Count - delay - 1].next_states, r);
				}
			}

			reward_sum += CalcReward (obs_states, actions, next_obs_states);
			Learn (obs_states, action_index, next_obs_states, reward_sum);
			reward_sum = 0f;
		}
	 	episode_started = true;

		action_index = Policy (next_obs_states);
		actions = Index2Action (action_index);
		obs_states = next_obs_states;
		//actions.shoot = (isRed && Mathf.Abs (states.target_theta21 - 90f) < 20f) || (!isRed && Mathf.Abs (states.target_theta12 - 90f) < 20f);
		return actions;
	}

	public void EndEpisode(States states){
		int state_i;
		int action_i;
		float alpha = 0.1f;
		float gamma = 0.9f;
		float reward = 0f;
		if ((isRed && states.HP2 <= 0 )|| (!isRed && states.HP1 <= 0))
			reward = 1f;
		if ((!isRed && states.HP2 <= 0)|| (isRed && states.HP1 <= 0))
			reward = -1f;

		if (isRed) {
			int delay = (int)(next_obs_states.isDameged2Before / step_width);
			if (next_obs_states.isDamaged2 && delay < exbuff.Count) {
				debug_text.text = "isDB:" + states.isDameged2Before + "delay:" + delay + ", exbuff.Count:" + exbuff.Count;
				float r = CalcReward (obs_states, actions, next_obs_states);
				state_i = State2Index (exbuff [exbuff.Count - delay - 1].states);
				action_i = exbuff [exbuff.Count - delay - 1].actions;
				Qtable [state_i, action_index] += alpha * (r - Qtable [state_i, action_index]);
			}
		} else {
			int delay = (int)(next_obs_states.isDameged1Before / step_width);
			if (next_obs_states.isDamaged1 && delay < exbuff.Count) {
				float r = CalcReward (obs_states, actions, next_obs_states);
				state_i = State2Index (exbuff [exbuff.Count - delay - 1].states);
				action_i = exbuff [exbuff.Count - delay - 1].actions;
				Qtable [state_i, action_index] += alpha * (r - Qtable [state_i, action_index]);
			}
		}
			
		episode_started = false;
	}

	private void Learn(States states, int action_i, States next_states, float reward){
		int state_i = State2Index (states);
		int next_state_i = State2Index (next_states);
		float alpha = 0.1f;
		float gamma = 0.9f;

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
		float epsilon = Mathf.Min( 0.5f, Mathf.Max(0.1f, (30f-states.episode_i)/30f));
		if (Random.value < epsilon) {
			action_i = (int)((Random.value) * action_num);
		} else {
			float[] qvector = new float[action_num];
			debug_text.text = "";
			for (int i = 0; i < qvector.Length; i++) {
				qvector [i] = Qtable [state_i, i];
				debug_text.text += qvector[i].ToString("f4") + ", ";
				if( i%4 == 3)
					debug_text.text += "\n";
			}
			action_i = ArgMax (qvector);
		}
		return action_i;
	}

	private float CalcReward(States states, Actions actions, States next_states){
		float reward;
		reward = 0.5f*((states.HP2 - next_states.HP2) - (states.HP1 - next_states.HP1));
		reward += (Mathf.Max(90f,Mathf.Abs (states.target_theta12 - 90f)) - Mathf.Max(Mathf.Abs (states.target_theta21 - 90f)))/360f;
		if (!isRed)
			reward = -reward;
		if (Vector2.Distance (states.pos1, states.pos2) > 400f)
			reward -= 0.1f;
		if ((isRed && states.bullet_num1 < 1) || (!isRed && states.bullet_num2 < 1))
			reward -= 0.1f;

		foreach (Bullet bi in states.bullets_info)
			reward += 0f * bi.theta;

		return reward;
	}
		

	Actions Index2Action(int index){
		Actions ret_actions = new Actions ();
		float shoot;
		float speed;
		float yaw;
		shoot = (float)index % 2f;
		speed = (((float)index - shoot) / 2f) % 2f;
		yaw = ((float)index - shoot - 2f * speed) / 4f;
		//speed = index%2f;
		//yaw = (index - speed) / 2f;
		ret_actions.shoot = shoot > 0.5f;
		ret_actions.speed = speed;
		ret_actions.yaw = yaw * 0.5f;
		return ret_actions;
	}

	int State2Index(States states){
		float target_theta;
		float enemy_target_theta;
		float dist;
		int b_num;
		int target_theta_i = 0;
		int enemy_target_theta_i = 0;
		int dist_i = 0;
		int HP_i = 0;
		int bnum_i = 0;

		float[] tarm_theta = new float[]{ 80f, 100f, 180f, 270f };
		float[] tarm_dist = new float[]{ 100f, 400f };
		int[] tarm_bnum = new int[]{ 0, 5 };

		if (isRed) {
			target_theta = states.target_theta21;
			enemy_target_theta = states.target_theta12;
		}
		else{
			target_theta = states.target_theta12;
			enemy_target_theta = states.target_theta21;
		}
		dist = Vector2.Distance (states.pos1, states.pos2);
		if (isRed)
			b_num = states.bullet_num1;
		else
			b_num = states.bullet_num2;

		for (int i = 0; i < tarm_theta.Length; i++) {
			if (target_theta > tarm_theta [i])
				target_theta_i++;
			if (enemy_target_theta > tarm_theta [i])
				enemy_target_theta_i++;
		}
		for (int i = 0; i < tarm_dist.Length; i++) {
			if (dist > tarm_dist [i])
				dist_i++;
		}
		if ((isRed && (states.HP1 > states.HP2)) || (!isRed && (states.HP1 < states.HP2)))
			HP_i = 1;
		for (int i = 0; i < tarm_bnum.Length; i++) {
			if (b_num > tarm_bnum [i])
				bnum_i++;
		}

		//return dist_i * (tarm_theta.Length + 1) * (tarm_theta.Length + 1) + enemy_target_theta_i * (tarm_theta.Length + 1) + target_theta_i;
		return 3*2*3*5*target_theta_i + 3*2*3*enemy_target_theta_i + 3*2*dist_i + 3*HP_i + bnum_i;
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

	public void Save(){
		//save Qtable

		Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

		List<float> q = new List<float> ();
		foreach (float v in Qtable)
			q.Add (v);
		//PlayerPrefsUtils.SetObject ("hoge", obs_states);
		//var result = PlayerPrefsUtils.GetObject<States> ("hoge");
		//Debug.Log (result);
		XmlUtil.Seialize<List<float>>(serializeDataPath, q);
	}

	public void Load(){
		//load Qtable
		List<float> q = XmlUtil.Deserialize<List<float>> (serializeDataPath);;
		for (int i = 0; i < Qtable.GetLength (0); i++) {
			for (int j = 0; j < Qtable.GetLength (1); j++) {
				Qtable [i, j] = q[Qtable.GetLength (0)*j + i];
			}
		}
	}

}
