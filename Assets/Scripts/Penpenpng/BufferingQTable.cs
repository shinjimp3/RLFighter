using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Penpenpng
{
	/// <summary>
	/// 着弾観測用フィールドStates.isDamagedBeforeを利用するため、内部バッファを備えたQTable
	/// </summary>
	/// <typeparam name="S">Stateを継承する状態空間</typeparam>
	/// <typeparam name="A">Actionを継承する行動空間</typeparam>
	class BufferingQTable<S, A> : SimpleQTable<S, A>
		where S : State, new()
		where A : Action, new()
	{
		const int BufferSize = 300;
		List<Pair<S, A>> Buffer = new List<Pair<S, A>>();
		readonly float HitReward1;
		readonly float HitReward2;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="rewardFunc">報酬関数</param>
		/// <param name="hitReward1">機体1がダメージを受けた弾が発射されたステップにおける報酬</param>
		/// <param name="hitReward2">機体2がダメージを受けた弾が発射されたステップにおける報酬</param>
		public BufferingQTable(Func<S, A, S, float> rewardFunc, float hitReward1, float hitReward2) : base(rewardFunc)
		{
			HitReward1 = hitReward1;
			HitReward2 = hitReward2;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="rewardFunc">報酬関数</param>
		/// <param name="defaultQ">テーブルの初期値を決定する関数</param>
		/// <param name="hitReward1">機体1がダメージを受けた弾が発射されたステップにおける報酬</param>
		/// <param name="hitReward2">機体2がダメージを受けた弾が発射されたステップにおける報酬</param>
		public BufferingQTable(Func<S, A, S, float> rewardFunc, Func<S, A, float> defaultQ, float hitReward1, float hitReward2) : base(rewardFunc, defaultQ)
		{
			HitReward1 = hitReward1;
			HitReward2 = hitReward2;
		}

		protected override void OnStartedNewEpisode()
		{
			Buffer.Clear();
		}

		protected override void Learn(S s1, A a, S s2)
		{
			base.Learn(s1, a, s2);
			HitCheck(s1);
		}

		protected override A Policy(S nowState)
		{
			var decision = base.Policy(nowState);
			Store(nowState, decision);
			return decision;
		}

		void HitCheck(S nowState)
		{
			var nowRaw = nowState.RawState;
			const float alpha = 0.1f;
			const float gamma = 0.99f;

			//機体1にダメージを与えた弾が発射されたステップの行動に対して報酬hitReward1を与える
			if (nowRaw.isDamaged1)
			{
				try
				{
					var valiant = Buffer.First(p =>
					{
						var pastRaw = p.State.RawState;
						return pastRaw.step_i == nowRaw.step_i - nowRaw.isDameged1Before;
					});
					var valiantNext = Buffer[Buffer.IndexOf(valiant) + 1];

					//機体1にダメージを与えた弾が発射されたステップを正しく抽出出来ているかどうかのテスト
					//現状なぜか失敗する
					UnityEngine.Debug.Assert(valiant.State.RawState.isShooting2 || valiant.Action.ToRawAction().shoot);

					var s1 = valiant.State;
					var a = valiant.Action;
					var s2 = valiantNext.State;
					QTable[s1][a] += alpha * (HitReward1 + gamma * QTable[s2].Values.Max() - QTable[s1][a]);
				}
				catch (Exception e)
				{
					UnityEngine.Debug.Log(e.Message);
				}
			}

			//機体2にダメージを与えた弾が発射されたステップの行動に対して報酬hitReward2を与える
			if (nowRaw.isDamaged2)
			{
				try
				{
					var valiant = Buffer.First(p =>
					{
						var pastRaw = p.State.RawState;
						return pastRaw.step_i == nowRaw.step_i - nowRaw.isDameged2Before;
					});
					var valiantNext = Buffer[Buffer.IndexOf(valiant) + 1];

					//機体2にダメージを与えた弾が発射されたステップを正しく抽出出来ているかどうかのテスト
					//現状なぜか失敗する
					UnityEngine.Debug.Assert(valiant.State.RawState.isShooting1 || valiant.Action.ToRawAction().shoot);

					var s1 = valiant.State;
					var a = valiant.Action;
					var s2 = valiantNext.State;
					QTable[s1][a] += alpha * (HitReward2 + gamma * QTable[s2].Values.Max() - QTable[s1][a]);
				}
				catch (Exception e)
				{
					UnityEngine.Debug.Log(e.Message);
				}
			}
		}

		void Store(S state, A action)
		{
			Buffer.Add(new Pair<S, A>(state, action));
			if (Buffer.Count() > BufferSize) Buffer.RemoveAt(0);
		}
	}

	class Pair<S, A>
		where S : State
		where A : Action
	{
		public S State { get; set; }
		public A Action { get; set; }

		public Pair(S state, A action)
		{
			State = state; Action = action;
		}
	}
}
