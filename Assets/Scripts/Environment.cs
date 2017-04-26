using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment{
	public Environment(){
		min_speed = 100f * dt;
		max_speed = 250f * dt;
		yaw_speed = 135f * dt;
		bullet_speed = 300f * dt;
		hit_radius = 30f;
		bullet_life = 50;
		shoot_width = 4;
		shoot_time_passed = 0;
		step_i = 0;

		_states = new States (new Vector2(0f,0f), new Vector2(500f, 10f), 0f, 180f, 10, 10);//set initial pos and theta


		red_actions = new Actions ();
		green_actions = new Actions ();
		bullets_info = new List<Bullet> ();

		step_i_text = GameObject.Find ("Step").GetComponent<UnityEngine.UI.Text>();

	}

	UnityEngine.UI.Text step_i_text;

	private States _states;
	public States states{
		get {return _states.ShallowCopy();}
	}
	private States _initial_states;
	public States initial_states{
		get {return _initial_states.ShallowCopy();}
	}


	private float dt = 0.1f;

	private float min_speed;
	private float max_speed;
	private float yaw_speed;
	private float bullet_speed;
	private float hit_radius;
	private int bullet_life;
	private int shoot_width;
	private int shoot_time_passed;
	private int step_i;

	private List<Bullet> bullets_info; 

	private Actions red_actions;
	private Actions green_actions;

	public States Run(Actions red_actions, Actions green_actions){
		this.red_actions = red_actions;
		this.green_actions = green_actions;
		ControlPlayer ();
		ControlBullets ();
		step_i++;
		step_i_text.text = "step:" + step_i;
		return states;

	}

	void ControlPlayer(){
		_states.theta1 += yaw_speed * 2f * (red_actions.yaw - 0.5f);
		float speed = (min_speed + red_actions.speed * (max_speed - min_speed));
		_states.pos1.x += Mathf.Cos (_states.theta1 * Mathf.Deg2Rad) * speed;
		_states.pos1.y += Mathf.Sin (_states.theta1 * Mathf.Deg2Rad) * speed;

		_states.theta2 += yaw_speed * 2f * (green_actions.yaw - 0.5f);
		speed = (min_speed + green_actions.speed * (max_speed - min_speed));
		_states.pos2.x += Mathf.Cos (_states.theta2 * Mathf.Deg2Rad) * speed;
		_states.pos2.y += Mathf.Sin (_states.theta2 * Mathf.Deg2Rad) * speed;
	}

	void ControlBullets(){
		float theta, length;
		length = hit_radius + 1f;

		//Add bullets
		if (shoot_time_passed == shoot_width) {
			shoot_time_passed = 0;
			if (red_actions.shoot) {
				theta = _states.theta1;
				Bullet bullet_info = new Bullet (
					                    _states.pos1 + length * new Vector2 (Mathf.Cos (theta * Mathf.Deg2Rad), Mathf.Sin (theta * Mathf.Deg2Rad)),
					                    theta,
					                    bullet_life
				                    );
				bullets_info.Add (bullet_info);
				_states.bullet_num1--;
			}
			if (green_actions.shoot) {
				theta = _states.theta2;
				Bullet bullet_info = new Bullet (
					                    _states.pos2 + length * new Vector2 (Mathf.Cos (theta * Mathf.Deg2Rad), Mathf.Sin (theta * Mathf.Deg2Rad)),
					                    theta,
					                    bullet_life
				                    );
				bullets_info.Add (bullet_info);
				_states.bullet_num2--;
			}
		}
		shoot_time_passed++;

		//  judge hit -> reduce bullet's life -> move bullets ->  remove bullets ->  -> 
		_states.isDamaged1 = false;
		_states.isDamaged2 = false;
		for(int i = 0; i < bullets_info.Count; i++){
			bool removed = false;
			if (Vector2.Dot (bullets_info [i].pos - _states.pos1, bullets_info [i].pos - _states.pos1) < hit_radius * hit_radius) {
				_states.isDamaged1 = true;
				_states.HP1--;
				removed = true;
			}
			if (Vector2.Dot (bullets_info [i].pos - _states.pos2, bullets_info [i].pos - _states.pos2) < hit_radius * hit_radius) {
				_states.isDamaged2 = true;
				_states.HP2--;
				removed = true;
			}

			bullets_info [i].life--;
			if (bullets_info [i].life <= 0) {
				removed = true;
			}

			theta = bullets_info[i].theta;
			bullets_info [i].pos += bullet_speed * new Vector2 (Mathf.Cos (theta * Mathf.Deg2Rad), Mathf.Sin (theta * Mathf.Deg2Rad));

			if (removed) {
				bullets_info.RemoveAt (i);
				i--;
			}
		}
	}

	public List<Bullet> GetBulletsPosAng(){
		return bullets_info;
	}

	public float GetDt(){
		return dt;
	}
}
