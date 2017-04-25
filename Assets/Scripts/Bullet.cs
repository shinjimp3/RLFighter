using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet{

	public Bullet(Vector2 pos, float theta, int life){
		this.pos = pos;
		this.theta = theta;
		this.life = life;
	}
	public Vector2 pos;
	public float theta;
	public int life;
}
