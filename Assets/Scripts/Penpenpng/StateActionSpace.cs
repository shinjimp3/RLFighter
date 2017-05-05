using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Penpenpng
{
	/// <summary>
	/// 量子化された状態空間
	/// </summary>
	class PolarCoord : State
	{
		//赤1から見て正面を0度としたときの緑2の方向thetaの絶対値を表現したもの
		public int RawAbsTheta { get; private set; }
		public int AbsTheta { get; private set; }
		const int AbsThetaSize = 8;
		//thetaの符号。0が非正で1が正
		public int ThetaSign { get; private set; }
		const int ThetaSignSize = 2;
		//緑2から見て正面を0度としたときの赤1の方向phiの絶対値を表現したもの
		public int RawAbsPhi { get; private set; }
		public int AbsPhi { get; private set; }
		const int AbsPhiSize = 8;
		//phiの符号。0が非正で1が正
		public int PhiSign { get; private set; }
		const int PhiSignSize = 2;
		//赤1と緑2の距離を表現したもの
		public int RawDistance { get; private set; }
		public int Distance { get; private set; }
		const int DistanceSize = 11;

		public string DebugStr()
		{
			return string.Format("dis:{0}({3}), th:{1}({4}), ph:{2}({5})", RawDistance, RawAbsTheta, RawAbsPhi, Distance, AbsTheta, AbsPhi);
		}

		public override string Identifier()
		{
			return string.Format("pc:{0}:{1}:{2}:{3}:{4}", AbsTheta, ThetaSign, AbsPhi, PhiSign, Distance);
		}

		public override IEnumerable<State> All()
		{
			for (int at = 0; at < AbsThetaSize; at++)
				for (int ts = 0; ts < ThetaSignSize; ts++)
					for (int ap = 0; ap < AbsPhiSize; ap++)
						for (int ps = 0; ps < PhiSignSize; ps++)
							for (int d = 0; d < DistanceSize; d++)
								yield return new PolarCoord()
								{
									AbsTheta = at,
									ThetaSign = ts,
									AbsPhi = ap,
									PhiSign = ps,
									Distance = d
								};
		}

		//区間の区切り点
		static readonly int[] ThetaKnots = new int[AbsThetaSize - 1] { 10, 20, 30, 40, 60, 90, 120 };
		static readonly int[] PhiKnots = new int[AbsPhiSize - 1] { 10, 20, 30, 40, 60, 90, 120 };
		static readonly int[] DistanceKnots = new int[DistanceSize - 1] { 60, 120, 180, 240, 300, 400, 500, 600, 800, 1000 };

		public override State FromRawState(States states)
		{
			//各種値の計算および離散化
			int rawAbsTheta = (int)Math.Abs(90 - states.target_theta21);
			if (rawAbsTheta > 180) rawAbsTheta = 360 - rawAbsTheta;
			int absTheta = ThetaKnots.TakeWhile(k => k <= rawAbsTheta).Count();

			int thetaSign = 90 < states.target_theta21 && states.target_theta21 <= 270 ? 1 : 0;

			int rawAbsPhi = (int)Math.Abs(90 - states.target_theta12);
			if (rawAbsPhi > 180) rawAbsPhi = 360 - rawAbsPhi;
			int absPhi = PhiKnots.TakeWhile(k => k <= rawAbsPhi).Count();

			int phiSign = 90 < states.target_theta12 && states.target_theta12 <= 270 ? 1 : 0;

			int rawDistance = (int)states.rel_pos12.magnitude;
			int distance = DistanceKnots.TakeWhile(k => k <= rawDistance).Count();

			return new PolarCoord()
			{
				RawState = states,
				RawAbsTheta = rawAbsTheta,
				AbsTheta = absTheta,
				ThetaSign = thetaSign,
				RawAbsPhi = rawAbsPhi,
				AbsPhi = absPhi,
				PhiSign = phiSign,
				RawDistance = rawDistance,
				Distance = distance,
			};
		}

		public PolarCoord Reverse()
		{
			return new PolarCoord()
			{
				RawState = RawState,
				AbsTheta = AbsTheta,
				ThetaSign = ThetaSign == 1 ? 0 : 1,
				AbsPhi = AbsPhi,
				PhiSign = PhiSign == 1 ? 0 : 1,
				Distance = Distance,
			};
		}
	}

	/// <summary>
	/// 量子化された行動空間
	/// </summary>
	class AllAction : Action
	{
		protected static readonly float[] YawOptions = new float[] { 0f, 0.25f, 0.5f, 0.75f, 1f };
		public float Yaw { get; protected set; }
		static readonly float[] SpeedOptions = new float[] { 0f, 0.3f, 0.6f, 1f };
		public float Speed { get; protected set; }
		static readonly bool[] ShootOptions = new bool[] { true, false };
		public bool Shoot { get; protected set; }

		public override IEnumerable<Action> All()
		{
			foreach (var yaw in YawOptions)
				foreach (var speed in SpeedOptions)
					foreach (var shoot in ShootOptions)
						yield return new AllAction()
						{
							Yaw = yaw,
							Speed = speed,
							Shoot = shoot,
						};
		}

		public override string Identifier()
		{
			return string.Format("aa:{0}:{1}:{2}", Yaw, Speed, Shoot);
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


	class HandleAction : Action
	{
		protected static readonly float[] YawOptions = new float[] { 0f, 0.25f, 0.5f, 0.75f, 1f };
		public float Yaw { get; protected set; }
		static readonly float[] SpeedOptions = new float[] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f };
		public float Speed { get; protected set; }

		public override IEnumerable<Action> All()
		{
			foreach (var yaw in YawOptions)
				foreach (var speed in SpeedOptions)
					yield return new HandleAction()
					{
						Yaw = yaw,
						Speed = speed,
					};
		}

		public override string Identifier()
		{
			return string.Format("ha:{0}:{1}", Yaw, Speed);
		}

		public override Actions ToRawAction()
		{
			return new Actions()
			{
				yaw = Yaw,
				speed = Speed,
			};
		}

		public Actions ToRawAction(bool shoot)
		{
			return new Actions()
			{
				yaw = Yaw,
				speed = Speed,
				shoot = shoot,
			};
		}

		public HandleAction Reverse()
		{
			int yaw_i = YawOptions.ToList().IndexOf(Yaw);
			return new HandleAction()
			{
				Yaw = YawOptions.Reverse().ToArray()[yaw_i],
				Speed = Speed,
			};
		}
	}

}
