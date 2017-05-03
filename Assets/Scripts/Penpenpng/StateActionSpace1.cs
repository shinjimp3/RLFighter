using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Penpenpng
{
	/// <summary>
	/// 量子化された状態空間の一例
	/// </summary>
	class State1 : State
	{
		//赤1から見て正面を0度としたときの緑2の方向thetaの絶対値を0～2で表現したもの
		public int AbsTheta { get; private set; }
		const int AbsThetaSize = 3;
		//thetaの符号。0が非正で1が正
		public int ThetaSign { get; private set; }
		const int ThetaSignSize = 2;
		//緑2から見て正面を0度としたときの赤1の方向phiの絶対値を0～2で表現したもの
		public int AbsPhi { get; private set; }
		const int AbsPhiSize = 3;
		//phiの符号。0が非正で1が正
		public int PhiSign { get; private set; }
		const int PhiSignSize = 2;
		//赤1と緑2の距離を0～1で表現したもの
		public int Distance { get; private set; }
		const int DistanceSize = 2;

		public override string Identifier()
		{
			return string.Format("s1:{0}:{1}:{2}:{3}:{4}", AbsTheta, ThetaSign, AbsPhi, PhiSign, Distance);
		}

		public override IEnumerable<State> All()
		{
			for (int at = 0; at < AbsThetaSize; at++)
				for (int ts = 0; ts < ThetaSignSize; ts++)
					for (int ap = 0; ap < AbsPhiSize; ap++)
						for (int ps = 0; ps < PhiSignSize; ps++)
							for (int d = 0; d < DistanceSize; d++)
								yield return new State1()
								{
									AbsTheta = at,
									ThetaSign = ts,
									AbsPhi = ap,
									PhiSign = ps,
									Distance = d
								};
		}

		//区間の区切り点
		static readonly int[] ThetaKnots = new int[AbsThetaSize - 1] { 30, 150 };
		static readonly int[] PhiKnots = new int[AbsPhiSize - 1] { 30, 150 };
		static readonly int[] DistanceKnots = new int[DistanceSize - 1] { 100 };

		public override State FromRawState(States states)
		{
			//各種値の計算および離散化
			int rawAbsTheta = (int)Math.Abs(90 - states.target_theta21);
			if (rawAbsTheta > 180) rawAbsTheta = 360 - rawAbsTheta;
			int absTheta = ThetaKnots.TakeWhile(k => k <= rawAbsTheta).Count();

			int thetaSign = 90 < states.target_theta21 && states.target_theta21 <= 270 ? 1 : 0;

			int phi = (int)Math.Abs(90 - states.target_theta12);
			if (phi > 180) phi = 360 - phi;
			int absPhi = PhiKnots.TakeWhile(k => k <= rawAbsTheta).Count();

			int phiSign = 90 < states.target_theta12 && states.target_theta12 <= 270 ? 1 : 0;

			int rawDistance = (int)states.rel_pos12.magnitude;
			int distance = DistanceKnots.TakeWhile(k => k <= rawDistance).Count();

			return new State1()
			{
				RawState = states,
				AbsTheta = absTheta,
				ThetaSign = thetaSign,
				AbsPhi = absPhi,
				PhiSign = phiSign,
				Distance = distance,
			};
		}
	}

	/// <summary>
	/// 量子化された行動空間の一例
	/// </summary>
	class Action1 : Action
	{
		protected static readonly float[] YawOptions = new float[] { 0f, 0.5f, 1f };
		public float Yaw { get; private set; }
		static readonly float[] SpeedOptions = new float[] { 0f, 1f };
		public float Speed { get; private set; }
		static readonly bool[] ShootOptions = new bool[] { true, false };
		public bool Shoot { get; private set; }

		public override IEnumerable<Action> All()
		{
			foreach (var yaw in YawOptions)
				foreach (var speed in SpeedOptions)
					foreach (var shoot in ShootOptions)
						yield return new Action1()
						{
							Yaw = yaw,
							Speed = speed,
							Shoot = shoot,
						};
		}

		public override string Identifier()
		{
			return string.Format("a1:{0}:{1}:{2}", Yaw, Speed, Shoot);
		}

		public override Actions ToRawAction()
		{
			return new Actions()
			{
				yaw = Yaw,
				speed = Speed,
				shoot = Shoot
			};
		}
	}
}
