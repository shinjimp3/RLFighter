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
}
