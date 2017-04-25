using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player {

	public HumanPlayer(bool isRed):base(isRed){
		actions = new Actions ();
		this.isRed = isRed;
	}

	public Actions RunStep(States states){
		if (passed_time < step_width) {
			passed_time++;
			return actions;
		}

		if (Input.GetKey (KeyCode.A)) {
			actions.yaw = 1f;
		} else if (Input.GetKey (KeyCode.D)) {
			actions.yaw = 0f;
		} else {
			actions.yaw = 0.5f;
		}
		if (Input.GetKey (KeyCode.W)) {
			actions.speed = 1f;
		} else if (Input.GetKey (KeyCode.S)) {
			actions.speed = 0f;
		}
		actions.shoot = Input.GetKey (KeyCode.F);
		return actions;
	}

}
