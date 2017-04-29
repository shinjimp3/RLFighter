using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player{
	
	//private float step_width = 2f * 0.1f;
	internal int step_width = 2;	//times of env update rate.
	//private float passed_time = 0f;
	internal int passed_time = 0;

	internal Actions actions;
	internal States obs_states;
	internal States next_obs_states;
	internal float reward;
	internal bool isRed;
	internal bool episode_started;

//	public Actions actions{
//		get {return action.ShallowCopy();}
//	}
		

	public Player(bool isRed){
		actions = new Actions ();
		this.isRed = isRed;
		episode_started = false;
	}

	public Actions RunStep(States states){
		if (passed_time < step_width) {
			passed_time++;
			return actions;
		}
		
		passed_time = 0;
		next_obs_states = states;
		if (episode_started) {
			reward = CalcReward (obs_states, actions, next_obs_states);
			Learn (obs_states, actions, next_obs_states, reward);
		}
		episode_started = true;
		obs_states = next_obs_states;
		actions = Policy (next_obs_states);
		return actions;
	}

	public void Reset(){
		
	}

	private void Learn(States states, Actions actions, States next_states, float reward){
		//hoge hoge
	}

	private Actions Policy(States states){
		Actions retaction = new Actions ();
		retaction.yaw = Random.value;
		retaction.speed = Random.value;
		retaction.shoot = (Random.value > 0.8f);
		return retaction;
	}

	private float CalcReward(States states, Actions actions, States next_states){
		return Random.value;
	}

}
