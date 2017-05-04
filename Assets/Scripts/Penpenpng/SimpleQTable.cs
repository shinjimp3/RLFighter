using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Penpenpng
{
	/// <summary>
	/// QTableの単純な実装
	/// </summary>
	/// <typeparam name="S">Stateを継承する状態空間</typeparam>
	/// <typeparam name="A">Actionを継承する行動空間</typeparam>
	class SimpleQTable<S, A> : Strategy
		where S : State, new()
		where A : Action, new()
	{
		protected Dictionary<S, Dictionary<A, float>> QTable;
		protected readonly Func<S, A, S, float> RewardFunc;
		protected readonly State StateFactory;
		protected readonly Action ActionFactory;
		protected S PrevState;
		protected A PrevAction;
		protected int CurrentEpisode = -1;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="rewardFunc">報酬関数</param>
		public SimpleQTable(Func<S, A, S, float> rewardFunc)
		{
			StateFactory = new S();
			ActionFactory = new A();
			RewardFunc = rewardFunc;

			InitializeQTable((s, a) => 0);
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="rewardFunc">報酬関数</param>
		/// <param name="defaultQ">テーブルの初期値を決定する関数</param>
		public SimpleQTable(Func<S, A, S, float> rewardFunc, Func<S, A, float> defaultQ)
		{
			StateFactory = new S();
			ActionFactory = new A();
			RewardFunc = rewardFunc;

			InitializeQTable(defaultQ);
		}

		void InitializeQTable(Func<S, A, float> defaultQ)
		{
			QTable = StateFactory.All().ToDictionary(
				s => (S)s,
				s => ActionFactory.All().ToDictionary(
					a => (A)a,
					a => defaultQ((S)s, (A)a)));
		}

		public override Actions RunStep(States states)
		{
			S nowState = (S)StateFactory.FromRawState(states);
			A decision = (A)ActionFactory.Random();

			if (states.episode_i == CurrentEpisode)
			{
				Learn(PrevState, PrevAction, nowState);
				decision = Policy(nowState);
			}
			else
			{
				CurrentEpisode = states.episode_i;
				OnStartedNewEpisode();
			}

			PrevState = nowState;
			PrevAction = decision;
			return decision.ToRawAction();
		}

		protected virtual void OnStartedNewEpisode() { }

		protected virtual void Learn(S s1, A a, S s2)
		{
			const float alpha = 0.1f;
			const float gamma = 0.99f;
			QTable[s1][a] += alpha * (RewardFunc(s1, a, s2) + gamma * QTable[s2].Values.Max() - QTable[s1][a]);
		}

		protected virtual A Policy(S nowState)
		{
			bool greedy = new Random().NextDouble() < 0.8;

			if (greedy)
			{
				var maxQ = QTable[nowState].Values.Max();
				return QTable[nowState].First(p => p.Value == maxQ).Key;
			}
			else
			{
				return (A)ActionFactory.Random();
			}
		}

	}


	abstract class State : IEquatable<State>
	{
		//離散化する前のナマのStates
		public States RawState { get; protected set; }

		public abstract string Identifier();
		public abstract IEnumerable<State> All();
		public abstract State FromRawState(States states);
		public State Random()
		{
			var choices = All().ToArray();
			var i = new Random().Next(0, choices.Length);
			return choices[i];
		}

		public bool Equals(State other) { return Identifier() == other.Identifier(); }
		public override bool Equals(object obj) { return Equals(obj as State); }
		public override int GetHashCode() { return Identifier().GetHashCode(); }
	}


	abstract class Action : IEquatable<Action>
	{
		public abstract string Identifier();
		public abstract IEnumerable<Action> All();
		public abstract Actions ToRawAction();
		public Action Random()
		{
			var choices = All().ToArray();
			var i = new Random().Next(0, choices.Length);
			return choices[i];
		}

		public bool Equals(Action other) { return Identifier() == other.Identifier(); }
		public override bool Equals(object obj) { return Equals(obj as Action); }
		public override int GetHashCode() { return Identifier().GetHashCode(); }
	}
}
