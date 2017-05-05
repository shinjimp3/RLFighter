using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Penpenpng
{
	abstract class Strategy
	{
		abstract public Actions RunStep(States states);

		protected void Debug(string message)
		{
			GameObject.Find("FreeText2").GetComponent<Text>().text = message;
		}
	}
}
