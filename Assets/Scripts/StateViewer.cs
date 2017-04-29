using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateViewer : MonoBehaviour {
	public Transform player_red;
	public Transform player_green;
	public GameObject bullet;
	private List<GameObject> bullets;
	UnityEngine.UI.Text hp_red;
	UnityEngine.UI.Text hp_green;
	UnityEngine.UI.Text bullet_num_red;
	UnityEngine.UI.Text bullet_num_green;
	UnityEngine.UI.Text result_text;

	public Transform camera;
	Vector2 camera_move_dist;
	Vector2 camera_pos_target;
	private States pre_states;
	private States tmp_states;
	private Actions pre_red_actions;
	private Actions pre_green_actions;
	private Actions tmp_red_actions;
	private Actions tmp_green_actions;
	private float pre_time_ratio;

	void Start(){
		bullets = new List<GameObject> ();
		for (int i = 0; i < 100; i++) {
			GameObject _bullet = Instantiate (bullet, transform.position, transform.rotation);
			_bullet.GetComponent<Renderer> ().enabled = false;
			bullets.Add (_bullet);
		}
		hp_red = GameObject.Find ("HPred").GetComponent<UnityEngine.UI.Text>();
		hp_green = GameObject.Find ("HPgreen").GetComponent<UnityEngine.UI.Text>();
		bullet_num_red = GameObject.Find ("BulletNumred").GetComponent<UnityEngine.UI.Text>();
		bullet_num_green = GameObject.Find ("BulletNumgreen").GetComponent<UnityEngine.UI.Text>();
		result_text = GameObject.Find ("Result").GetComponent<UnityEngine.UI.Text>();

		camera_move_dist = new Vector2 (200f, 150f);
		camera_pos_target = new Vector2(0,0);
	}

	public void Draw(States states, Actions red_acitons, Actions green_actions, List<Bullet> bullets_info, float time_ratio){

		if (time_ratio < pre_time_ratio) {
			pre_states = tmp_states.ShallowCopy ();
			pre_red_actions = tmp_red_actions;
			pre_green_actions = tmp_green_actions;
		}
		tmp_states = states.ShallowCopy();
		tmp_red_actions = red_acitons;
		tmp_green_actions = green_actions;
		pre_time_ratio = time_ratio;

		DrawPlayers (states, red_acitons, green_actions, time_ratio);

		// draw bullets
		for (int i = 0; i < bullets.Count; i++) {
			if (i < bullets_info.Count) {
				bullets [i].GetComponent<Renderer> ().enabled = true;
				bullets [i].transform.position = new Vector3 (bullets_info [i].pos.x, 0f, bullets_info [i].pos.y)
					- (1f - time_ratio) * 30f * new Vector3 (Mathf.Cos (Mathf.Deg2Rad * bullets_info [i].theta), 0f, Mathf.Sin (Mathf.Deg2Rad * bullets_info [i].theta));
				//30f will be fixed to be bullet_speed * dt.
				bullets [i].transform.rotation = Quaternion.Euler (Vector3.up * (-bullets_info [i].theta -90f));
			} else {
				bullets [i].GetComponent<Renderer> ().enabled = false;
			}
		}

		ShowText (states);
		MoveCamera (states, time_ratio);
	}

	void DrawPlayers(States states, Actions red_actions, Actions green_actions, float t){
		if (pre_states == null)
			pre_states = states;
		if (pre_red_actions == null)
			pre_red_actions = red_actions;
		if (pre_green_actions == null)
			pre_green_actions = green_actions;

		float thetat;
		Vector2 player_pos;
		// draw players
		//player_red.position = new Vector3 (states.pos1.x, 0f, states.pos1.y);
		player_pos = CalcBezierPos(pre_states.pos1, states.pos1, pre_states.theta1, states.theta1, t);
		player_red.position = new Vector3(player_pos.x, 0f, player_pos.y);
		thetat = (1f - t) * pre_states.theta1 + t * states.theta1;
		player_red.rotation = Quaternion.Euler (Vector3.up * -thetat) * Quaternion.Euler(Vector3.right*-90f);
		player_red.Rotate (Vector3.right * ((1f - t) * pre_red_actions.yaw + t * red_actions.yaw - 0.5f) * 50f);

		//player_green.position = new Vector3 (states.pos2.x, 0f, states.pos2.y);
		player_pos = CalcBezierPos (pre_states.pos2, states.pos2, pre_states.theta2, states.theta2, t);
		player_green.position = new Vector3(player_pos.x, 0f, player_pos.y);
		thetat = (1f - t) * pre_states.theta2 + t * states.theta2;
		player_green.rotation = Quaternion.Euler (Vector3.up * -thetat) * Quaternion.Euler(Vector3.right*-90f);
		player_green.Rotate (Vector3.right * ((1f - t) * pre_green_actions.yaw + t * green_actions.yaw - 0.5f) * 50f);

	}

	Vector2 CalcBezierPos(Vector2 x0, Vector2 x1, float theta0, float theta1, float t){
		theta0 *= Mathf.Deg2Rad;
		theta1 *= Mathf.Deg2Rad;
		Vector2 p;
		Vector2 r0 = new Vector2 (Mathf.Cos (theta0), Mathf.Sin (theta0));
		Vector2 r1 = new Vector2 (Mathf.Sin (theta1), -Mathf.Cos (theta1));

		//if (Mathf.Abs (theta0 - theta1) < 0.001f) {
			p = (1f - t) * x0 + t * x1;
		Debug.Log (x0 + "," + x1);
		//}else{
		//	Vector2 q = x0 + (Vector2.Dot(x1-x0,r1)) / (Mathf.Sin (theta0 - theta1)) * r0;
		//	p = (1f - t) * (1f - t) * x0 + 2f * t * (1f - t) * q + t * t * x1;
		//}
		return p;
	}

	void ShowText(States states){
		//show HP
		hp_red.text = "HP:"+states.HP1;
		hp_green.text = "HP:"+states.HP2;

		//show bullet num
		bullet_num_red.text = "bullet:"+states.bullet_num1;
		bullet_num_green.text = "bullet:"+states.bullet_num2;

	}

	void MoveCamera(States states, float t){
		//move camera
		Vector2 mid_pos = ((1f-t)*(pre_states.pos1+ pre_states.pos2) + t*(states.pos1 + states.pos2))/2f;
		float dist = Vector2.Distance ((1f-t)*pre_states.pos1 + t*states.pos1, (1f-t)*pre_states.pos2 + t*states.pos2);
		if (mid_pos.x - camera_pos_target.x > camera_move_dist.x)
			camera_pos_target.x += camera_move_dist.x;
		if (mid_pos.x - camera_pos_target.x < -camera_move_dist.x)
			camera_pos_target.x -= camera_move_dist.x;
		if (mid_pos.y - camera_pos_target.y > camera_move_dist.y)
			camera_pos_target.y += camera_move_dist.y;
		if (mid_pos.y - camera_pos_target.y < -camera_move_dist.y)
			camera_pos_target.y -= camera_move_dist.y;

		dist = Mathf.Max (200f, dist);
		camera.position = (new Vector3 (camera_pos_target.x, dist*1.5f, camera_pos_target.y) + 2f*camera.position) / 3f;
		//Debug.Log (bullets_info.Count);

	}

	public void ShowResult(States states){
		if (states.HP1 <= 0) {
			result_text.text = "Player Green Wins!";
			result_text.color = Color.green;
		} else if (states.HP2 <= 0) {
			result_text.text = "Player Red Wins!";
			result_text.color = Color.red;
		} else if (states.bullet_num1 == 0 && states.bullet_num2 == 0) {
			result_text.text = "No Bullets...";
			result_text.color = Color.blue;
		} else {
			result_text.text = "";
		}
	}

}
