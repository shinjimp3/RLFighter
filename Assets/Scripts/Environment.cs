﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment{
	public Environment(){
		dt = 0.1f;
		min_speed = 150f * dt;
		max_speed = 250f * dt;
		yaw_speed = 135f * dt;
		bullet_speed = 300f * dt;
		hit_radius = 20f;
		bullet_life = 50;
		shoot_width = 2;
		reload_width = 4;
		shoot_time_passed_red = 0;
		shoot_time_passed_green = 0;
		reload_time_passed_red = 0;
		reload_time_passed_green = 0;
		step_i = 0;
		episode_i = 0;

		step_i_text = GameObject.Find ("Step").GetComponent<UnityEngine.UI.Text>();
		episode_i_text = GameObject.Find ("Episode").GetComponent<UnityEngine.UI.Text>();

		train_toggle = GameObject.Find ("TrainToggle").GetComponent<UnityEngine.UI.Toggle> ();
		endless_toggle = GameObject.Find ("EndlessToggle").GetComponent<UnityEngine.UI.Toggle> ();

		if (endless_toggle.isOn)
			initial_bullet_num = 10;
		else
			initial_bullet_num = 50;
		
		_initial_states = new States (new Vector2(0f,50f), new Vector2(500f, 150f), 0f, 180f, 5, initial_bullet_num);
		_states = initial_states;//set initial pos and theta


		red_actions = new Actions ();
		green_actions = new Actions ();

		_states.train = train_toggle.isOn;
		_states.endless = endless_toggle.isOn;
	}

	//states, bullets and actions
	private States _states;
	public States states{
		get {return _states.ShallowCopy();}
	}
	private States _initial_states;
	public States initial_states{
		get {return _initial_states.ShallowCopy();}
	}
	private Actions red_actions;
	private Actions green_actions;

	//parameter settings
	private float dt;
	private float min_speed;
	private float max_speed;
	private float yaw_speed;
	private float bullet_speed;
	private float hit_radius;
	private int bullet_life;
	private int shoot_width;
	private int reload_width;
	private int initial_bullet_num;

	private int shoot_time_passed_red;
	private int shoot_time_passed_green;
	private int reload_time_passed_red;
	private int reload_time_passed_green;
	private bool pre_red_shooting;
	private bool pre_green_shooting;

	//time parameter and viewer
	private int step_i;
	private int episode_i;

	UnityEngine.UI.Text step_i_text;
	UnityEngine.UI.Text episode_i_text;


	UnityEngine.UI.Toggle train_toggle;
	UnityEngine.UI.Toggle endless_toggle;

	// Run environment dynamics P(s'|s,a), and view step, episode...
	public States Run(Actions red_actions, Actions green_actions){
		this.red_actions = red_actions;
		this.green_actions = green_actions;
		_states.train = train_toggle.isOn;
		_states.endless = endless_toggle.isOn;
		_states.step_i = step_i;
		step_i_text.text = "step:" + step_i;
		episode_i_text.text = "episode:" + episode_i;
		ControlPlayer ();
		ControlBullets ();
		step_i++;
		return states;

	}

	public void Reset(){
		step_i = 0;
		episode_i++;
		_states = initial_states;
		_states.step_i = step_i;
		_states.episode_i = episode_i;
		_states.bullets_info.Clear ();
	}

	// Move players' position and rotation
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
		length = hit_radius + 0.1f;

		if (red_actions.shoot && _states.bullet_num1 > 0) {
			shoot_time_passed_red++;
		}
		if (green_actions.shoot && _states.bullet_num2 > 0) {
			shoot_time_passed_green++;
		}
		if (_states.endless) {
			if (!red_actions.shoot && _states.bullet_num1 < 10) {
				reload_time_passed_red++;
			}
			if (!green_actions.shoot && _states.bullet_num2 < 10) {
				reload_time_passed_green++;
			}
		}

		if (red_actions.shoot != pre_red_shooting) {
			shoot_time_passed_red = 0;
			reload_time_passed_red = 0;
		}
		if (green_actions.shoot != pre_green_shooting) {
			shoot_time_passed_green = 0;
			reload_time_passed_green = 0;
		}


		pre_red_shooting = red_actions.shoot;
		pre_green_shooting = green_actions.shoot;


		//Add bullets
		//if (red_actions.shoot && _states.bullet_num1 > 0) {
		if(shoot_time_passed_red >= shoot_width){
			theta = _states.theta1;
			Bullet bullet_info = new Bullet (
				                    _states.pos1 + length * new Vector2 (Mathf.Cos (theta * Mathf.Deg2Rad), Mathf.Sin (theta * Mathf.Deg2Rad)),
				                    theta,
				                    bullet_life,
									true
			                    );
			_states.bullets_info.Add (bullet_info);
			_states.bullet_num1--;
			shoot_time_passed_red = 0;
		}
		//if (green_actions.shoot && _states.bullet_num2 > 0) {
		if(shoot_time_passed_green >= shoot_width){
			theta = _states.theta2;
			Bullet bullet_info = new Bullet (
				                    _states.pos2 + length * new Vector2 (Mathf.Cos (theta * Mathf.Deg2Rad), Mathf.Sin (theta * Mathf.Deg2Rad)),
				                    theta,
				                    bullet_life,
									false
			                    );
			_states.bullets_info.Add (bullet_info);
			_states.bullet_num2--;
			shoot_time_passed_green = 0;
		}

		//reload bullets
		if (reload_time_passed_red >= reload_width) {
			_states.bullet_num1++;
			reload_time_passed_red = 0;
		}
		if (reload_time_passed_green >= reload_width) {
			_states.bullet_num2++;
			reload_time_passed_green = 0;
		}


		//  judge hit -> reduce bullet's life -> move bullets ->  remove bullets ->  -> 
		_states.isDamaged1 = false;
		_states.isDamaged2 = false;
		for(int i = 0; i < _states.bullets_info.Count; i++){
			bool removed = false;
			if (Vector2.Dot (_states.bullets_info [i].pos - _states.pos1, _states.bullets_info [i].pos - _states.pos1) < hit_radius * hit_radius) {
				_states.isDamaged1 = true;
				_states.isDameged1Before = bullet_life - _states.bullets_info [i].life;
				_states.HP1--;
				removed = true;
			}
			if (Vector2.Dot (_states.bullets_info [i].pos - _states.pos2, _states.bullets_info [i].pos - _states.pos2) < hit_radius * hit_radius) {
				_states.isDamaged2 = true;
				_states.isDameged2Before = bullet_life - _states.bullets_info [i].life;
				_states.HP2--;
				removed = true;
			}

			_states.bullets_info [i].life--;
			if (_states.bullets_info [i].life <= 0) {
				removed = true;
			}

			theta = _states.bullets_info[i].theta;
			_states.bullets_info [i].pos += bullet_speed * new Vector2 (Mathf.Cos (theta * Mathf.Deg2Rad), Mathf.Sin (theta * Mathf.Deg2Rad));

			if (removed) {
				_states.bullets_info.RemoveAt (i);
				i--;
			}
		}
	}

	public List<Bullet> GetBulletsPosAng(){
		return states.bullets_info;
	}

	public float GetDt(){
		return dt;
	}

}
