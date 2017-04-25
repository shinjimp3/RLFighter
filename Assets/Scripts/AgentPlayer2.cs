using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentPlayer2 : Player {
	public AgentPlayer2(bool isRed):base(isRed){
		actions = new Actions ();
		this.isRed = isRed;
	}

}
