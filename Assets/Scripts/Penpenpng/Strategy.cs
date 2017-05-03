using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Penpenpng
{
	abstract class Strategy
	{
		abstract public Actions RunStep(States states);
	}
}
