using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class States{

	//public States (States _states){
	//	PosAngle pos_ang1 = _states.pos_ang1;
	//	PosAngle pos_ang2 = _states.pos
	//
	//}
	public States(Vector2 pos1, Vector2 pos2, float theta1, float theta2, int HP, int bullet_num){
		this.pos1 = pos1;
		this.pos2 = pos2;
		this.theta1 = theta1;
		this.theta2 = theta2;
		this.HP1 = HP;
		this.HP2 = HP;
		this.bullet_num1 = bullet_num;
		this.bullet_num2 = bullet_num;
		bullets_info = new List<Bullet> ();
	}

	public States ShallowCopy(){
		return (States)this.MemberwiseClone ();
	}

	public bool train;
	public bool endless;
	public int step_i;
	public int episode_i;

	public Vector2 pos1;
	public Vector2 pos2;
	public float theta1;
	public float theta2;
	public int HP1;
	public int HP2;
	public int bullet_num1;
	public int bullet_num2;
	public bool isShooting1;
	public bool isShooting2;
	public bool isDamaged1;
	public int isDameged1Before;
	public bool isDamaged2;
	public int isDameged2Before;
	public List<Bullet> bullets_info;
	public Vector2 rel_pos12{
		get { return RelatedPos (pos1, pos2, theta2); }
	}
	public Vector2 rel_pos21{
		get { return RelatedPos (pos2, pos1, theta1); }
	}
	public float target_theta12{
		get{
			return (Mathf.Atan2 (rel_pos12.y, rel_pos12.x) * Mathf.Rad2Deg + 360f) % 360f;
		}
	}
	public float target_theta21{
		get{
			return (Mathf.Atan2 (rel_pos21.y, rel_pos21.x) * Mathf.Rad2Deg + 360f) % 360f;
		}
	}
	public float rel_theta12 {
		get {
			return RelatedThteta (theta1, theta2);
		}
	}
	public float rel_theta21{
		get {
			return RelatedThteta (theta2, theta1);
		}
	}
		


	private Vector2 RelatedPos(Vector2 to_pos, Vector2 from_pos, float from_theta){
		float delta_theta = Mathf.Deg2Rad * (from_theta - 90f);

		Vector2 rel_pos;

		Vector2 rotate_Mx = new Vector2(Mathf.Cos (-delta_theta), - Mathf.Sin (-delta_theta));
		Vector2 rotate_My = new Vector2(Mathf.Sin (-delta_theta),   Mathf.Cos (-delta_theta));
		Vector2 rel_pos_world = to_pos - from_pos;

		rel_pos.x = Vector2.Dot (rotate_Mx, rel_pos_world);
		rel_pos.y = Vector2.Dot (rotate_My, rel_pos_world);

		return rel_pos;
	}

	private float RelatedThteta(float to_theta, float from_theta){
		float ret;
		ret = (to_theta - from_theta) % 360f;
		if (ret < 0)
			ret += 360f;
		return ret;
	}
}