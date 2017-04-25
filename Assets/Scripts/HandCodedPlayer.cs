using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCodedPlayer : Player {

	public HandCodedPlayer(bool isRed):base(isRed){
		actions = new Actions ();
		this.isRed = isRed;
	}

	private float target_theta = 0f;



	public Actions RunStep(States states){
		if (passed_time < step_width) {
			passed_time++;
			return actions;
		}
		passed_time = 0;


		Vector2 rel_enemy_pos;
		if (isRed) {
			rel_enemy_pos = states.rel_pos21;
			target_theta = states.target_theta21;
		} else {
			rel_enemy_pos = states.rel_pos12;
			target_theta = states.target_theta12;
		}
			
		if ( target_theta < 180f ) {
			actions.yaw = 0.5f + Mathf.Min (0.5f, Mathf.Max (-0.5f, (target_theta - 90f) / 180f));
			actions.speed = 0f;
		}
		else {
			if (Random.value < 0.5f)
				actions.yaw = 0f;
			else
				actions.yaw = 1f;
			if(Vector2.Distance(states.pos1,states.pos2) < 250f)
				actions.speed = 1f;
		}

		actions.shoot = (Mathf.Abs (target_theta - 90f) < 20f);


		return actions;
	}
}
