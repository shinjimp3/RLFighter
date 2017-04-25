using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPlayer : Player {
	public RandomPlayer(bool isRed):base(isRed){
		actions = new Actions ();
		this.isRed = isRed;
	}

}
