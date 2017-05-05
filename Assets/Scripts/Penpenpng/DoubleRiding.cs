using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Penpenpng
{
	class DoubleRiding : Strategy
	{
		Strategy Controller;
		Strategy Shooter;

		public DoubleRiding(Strategy controller, Strategy shooter)
		{
			Controller = controller;
			Shooter = shooter;
		}

		public override Actions RunStep(States states)
		{
			var control = Controller.RunStep(states);
			var shoot = Shooter.RunStep(states);
			return new Actions()
			{
				yaw = control.yaw,
				speed = control.speed,
				shoot = shoot.shoot,
			};
		}
	}

	/// <summary>
	/// 量子化された射手の行動空間の一例
	/// </summary>
	class ShooterAction1 : AllAction
	{
		static readonly float DummyYaw = 0;
		static readonly float DummySpeed = 0;

		public override IEnumerable<Action> All()
		{
			return base.All().Cast<AllAction>().Select(
				a => new ShooterAction1()
				{
					Yaw = DummySpeed,
					Speed = DummySpeed,
					Shoot = a.Shoot,
				}
			).Cast<Action>().Distinct();
		}

		public override Actions ToRawAction()
		{
			return new Actions()
			{
				yaw = DummyYaw,
				speed = DummySpeed,
				shoot = Shoot,
			};
		}
	}

	/// <summary>
	/// 量子化された操舵手の行動空間。ControllerAction2より空間が大きい
	/// </summary>
	class ControllerAction2 : AllAction
	{
		static readonly bool DummyShoot = false;

		public override IEnumerable<Action> All()
		{
			return base.All().Cast<AllAction>().Select(
				a => new ControllerAction2()
				{
					Yaw = a.Yaw,
					Speed = a.Speed,
					Shoot = DummyShoot,
				}
			).Cast<Action>().Distinct();
		}

		public override Actions ToRawAction()
		{
			return new Actions()
			{
				yaw = Yaw,
				speed = Speed,
				shoot = DummyShoot,
			};
		}
	}
}
