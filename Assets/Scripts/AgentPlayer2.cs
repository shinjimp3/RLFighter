using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts.Penpenpng;

public class AgentPlayer2 : Player {
	Strategy strategy;

	public AgentPlayer2(bool isRed):base(isRed){
		actions = new Actions ();
		this.isRed = isRed;

		strategy = new BufferingQTableAlt<State1, Action1>(
			//価値関数
			(s1, a, s2) => {
				float reward = -5;
				//弾の溜めすぎや使いすぎはよくないこと
				if (s2.RawState.bullet_num2 < 1 || 9 < s2.RawState.bullet_num2) reward += -3;
				//離れ過ぎはよくないこと
				if (s1.RawState.rel_pos12.magnitude < s2.RawState.rel_pos12.magnitude) reward += 3;
				//自分が相手を向いているのはよいこと
				if (s2.AbsPhi == 0) reward += 1;
				return reward;
			},
			//QTableの初期値
			(s, a) => {
				return 15;
			}
			, 10, 0);
	}
	
	new public Actions RunStep(States state)
	{
		return strategy.RunStep(state);
	}
}
