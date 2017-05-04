using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts.Penpenpng;

public class AgentPlayer2 : Player {
	Strategy strategy;
	const int PeriodWidth = 2;
	int CurrentPhase = -1;
	Actions CurrentAction;

	public AgentPlayer2(bool isRed):base(isRed){
		actions = new Actions ();
		this.isRed = isRed;

		strategy = new GreenHybrid();
	}
	
	new public Actions RunStep(States state)
	{
		CurrentPhase = (CurrentPhase + 1) % PeriodWidth;
		if (CurrentPhase == 0) CurrentAction = strategy.RunStep(state);
		return CurrentAction;
	}
}
