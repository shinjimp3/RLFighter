using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Penpenpng
{
	class GreenHybrid : Strategy
	{
		List<Step<PolarCoord, HandleAction>> History = new List<Step<PolarCoord, HandleAction>>();
		Dictionary<PolarCoord, Dictionary<HandleAction, float>> QTable;
		const float HitReward = 20;
		static readonly State StateFactory = new PolarCoord();
		static readonly Action ActionFactory = new HandleAction();

		PolarCoord PrevState;
		HandleAction PrevAction;
		int CurrentEpisode = -1;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public GreenHybrid()
		{
			InitializeQTable((s, a) => 20);
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="defaultQ">テーブルの初期値を決定する関数</param>
		public GreenHybrid(Func<PolarCoord, HandleAction, float> defaultQ)
		{
			InitializeQTable(defaultQ);
		}

		void InitializeQTable(Func<PolarCoord, HandleAction, float> defaultQ)
		{
			QTable = StateFactory.All().ToDictionary(
				s => (PolarCoord)s,
				s => ActionFactory.All().ToDictionary(
					a => (HandleAction)a,
					a => defaultQ((PolarCoord)s, (HandleAction)a)));
		}

		public override Actions RunStep(States states)
		{
			PolarCoord nowState = (PolarCoord)StateFactory.FromRawState(states);
			HandleAction decision = (HandleAction)ActionFactory.Random();

			Debug(nowState.DebugStr());

			if (states.episode_i == CurrentEpisode)
			{
				Store(PrevState, PrevAction, nowState);
				Learn();
				Learn(PrevState, PrevAction, nowState);
				Learn(PrevState.Reverse(), PrevAction.Reverse(), nowState.Reverse());
				decision = Policy(nowState);
			}
			else
			{
				CurrentEpisode = states.episode_i;
			}

			PrevState = nowState;
			PrevAction = decision;

			bool shoot = states.bullet_num2 != 0 && nowState.RawAbsPhi < 60;
			return decision.ToRawAction(shoot);
		}

		void Learn()
		{
			foreach (var step in History.OrderBy(i => new Random().Next()).Take(100))
			{
				var s1 = step.Prev;
				var a = step.Action;
				var s2 = step.Next;
				Learn(s1, a, s2);
				Learn(s1.Reverse(), a.Reverse(), s2.Reverse());
			}
		}

		void Learn(PolarCoord s1, HandleAction a, PolarCoord s2)
		{
			const float alpha = 0.1f;
			const float gamma = 0.99f;
			QTable[s1][a] += alpha * (Reward(s1, a, s2) + gamma * QTable[s2].Values.Max() - QTable[s1][a]);
		}

		void Learn(PolarCoord s1, HandleAction a, PolarCoord s2, float reward)
		{
			const float alpha = 0.1f;
			const float gamma = 0.99f;
			QTable[s1][a] += alpha * (reward + gamma * QTable[s2].Values.Max() - QTable[s1][a]);
		}

		float Reward(PolarCoord s1, HandleAction a, PolarCoord s2)
		{
			return - s2.AbsPhi + s2.AbsTheta + s2.Distance * (s2.RawAbsTheta < 90 ? -1 : 1);
		}

		void HitCheck(PolarCoord nowState)
		{
			var nowRaw = nowState.RawState;

			//機体にダメージを与えた**可能性がある**行動に報酬を与える
			if (nowRaw.isDamaged1)
			{
				var valiants = History.Where(p => p.Action.ToRawAction().shoot).Reverse().Take(5);
				foreach (var valiant in valiants)
				{
					var s1 = valiant.Prev;
					var a = valiant.Action;
					var s2 = valiant.Next;
					Learn(s1, a, s2, HitReward);
					Learn(s1.Reverse(), a.Reverse(), s2.Reverse(), HitReward);
				}

				//experience replayもどき
				foreach (var step in History.AsEnumerable().Reverse())
				{
					var s1 = step.Prev;
					var a = step.Action;
					var s2 = step.Next;
					Learn(s1, a, s2);
					Learn(s1.Reverse(), a.Reverse(), s2.Reverse());
				}
			}
		}

		HandleAction Policy(PolarCoord nowState)
		{
			bool greedy = new Random().NextDouble() < 0.8;

			if (greedy)
			{
				var maxQ = QTable[nowState].Values.Max();
				return QTable[nowState].First(p => p.Value == maxQ).Key;
			}
			else
			{
				return (HandleAction)ActionFactory.Random();
			}
		}

		void Store(PolarCoord s1, HandleAction a, PolarCoord s2)
		{
			const int bufsize = 1000;
			History.Add(new Step<PolarCoord, HandleAction>(s1, a, s2));
			if (History.Count() > bufsize) History.RemoveAt(0);
		}
	}

	class Step<S, A>
		where S : State
		where A : Action
	{
		public S Prev { get; set; }
		public A Action { get; set; }
		public S Next { get; set; }

		public Step(S s1, A a, S s2)
		{
			Prev = s1; Action = a; Next = s2;
		}
	}
}
