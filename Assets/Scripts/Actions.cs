using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions{

	public Actions ShallowCopy(){
		return (Actions)this.MemberwiseClone ();
	}

	private float _yaw;
	private float _speed;
	private bool _shoot;

	public float yaw {
		get{
			return _yaw;
		}
		set{
			_yaw = Constrain (0, 1, value);
		}
	}

	public float speed {
		get{
			return _speed;
		}
		set{
			_speed = Constrain (0, 1, value);
		}
	}

	public bool shoot{
		get{
			return _shoot;
		}
		set{
			_shoot = value;
		}
	}

	private float Constrain(float a, float b, float x){
		if (x > b)
			x = b;
		if (x < a)
			x = a;
		return x;
	}
}
