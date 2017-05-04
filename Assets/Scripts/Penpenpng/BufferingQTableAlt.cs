using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Penpenpng
{
	/// <summary>
	/// QTableAltのAssertionが失敗するので暫定的に作った
	/// </summary>
	/// <typeparam name="S">Stateを継承する状態空間</typeparam>
	/// <typeparam name="A">Actionを継承する行動空間</typeparam>
	class BufferingQTableAlt<S, A> : SimpleQTable<S, A>
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
		public BufferingQTableAlt(Func<S, A, S, float> rewardFunc, float hitReward1, float hitReward2) : base(rewardFunc)
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
		public BufferingQTableAlt(Func<S, A, S, float> rewardFunc, Func<S, A, float> defaultQ, float hitReward1, float hitReward2) : base(rewardFunc, defaultQ)
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

			//機体にダメージを与えた**可能性がある**行動に報酬を与える(Alt仕様)
			if (nowRaw.isDamaged1 || nowRaw.isDamaged2)
			{
				var valiants = Buffer.Where(p => p.Action.ToRawAction().shoot).Reverse().Take(5);
				foreach (var valiant in valiants)
				{
					int next_i = Buffer.IndexOf(valiant) + 1;
					if (next_i > Buffer.Count()) continue;
					var valiantNext = Buffer[next_i];

					var s1 = valiant.State;
					var a = valiant.Action;
					var s2 = valiantNext.State;
					if (nowRaw.isDamaged1) QTable[s1][a] += alpha * (HitReward1 + gamma * QTable[s2].Values.Max() - QTable[s1][a]);
					if (nowRaw.isDamaged2) QTable[s1][a] += alpha * (HitReward2 + gamma * QTable[s2].Values.Max() - QTable[s1][a]);
				}
			}
		}

		void Store(S state, A action)
		{
			Buffer.Add(new Pair<S, A>(state, action));
			if (Buffer.Count() > BufferSize) Buffer.RemoveAt(0);
		}
	}
}
	