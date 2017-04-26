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
	public Transform camera;
	Vector2 camera_move_dist;
	Vector2 camera_pos_target;

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

		camera_move_dist = new Vector2 (200f, 150f);
		camera_pos_target = new Vector2(0,0);
	}

	public void Draw(States states, Actions red_acitons, Actions green_actions, List<Bullet> bullets_info){
		// draw player
		player_red.position = new Vector3 (states.pos1.x, 0f, states.pos1.y);
		player_red.rotation = Quaternion.Euler (Vector3.up * (-states.theta1)) * Quaternion.Euler(Vector3.right*-90f);
		player_red.Rotate (Vector3.right * (red_acitons.yaw - 0.5f) * 50f);

		player_green.position = new Vector3 (states.pos2.x, 0f, states.pos2.y);
		player_green.rotation = Quaternion.Euler (Vector3.up * (-states.theta2)) * Quaternion.Euler(Vector3.right*-90f);
		player_green.Rotate (Vector3.right * (green_actions.yaw - 0.5f) * 50f);

		// draw bullet
		for (int i = 0; i < bullets.Count; i++) {
			if (i < bullets_info.Count) {
				bullets [i].GetComponent<Renderer> ().enabled = true;
				bullets [i].transform.position = new Vector3 (bullets_info [i].pos.x, 0f, bullets_info [i].pos.y);
				bullets [i].transform.rotation = Quaternion.Euler (Vector3.up * (-bullets_info [i].theta -90f));
			} else {
				bullets [i].GetComponent<Renderer> ().enabled = false;
			}
		}


		//move camera
		Vector2 mid_pos = (states.pos1 + states.pos2)/2f;
		float dist = Vector2.Distance (states.pos1, states.pos2);
		if (mid_pos.x - camera_pos_target.x > camera_move_dist.x)
			camera_pos_target.x += camera_move_dist.x;
		if (mid_pos.x - camera_pos_target.x < -camera_move_dist.x)
			camera_pos_target.x -= camera_move_dist.x;
		if (mid_pos.y - camera_pos_target.y > camera_move_dist.y)
			camera_pos_target.y += camera_move_dist.y;
		if (mid_pos.y - camera_pos_target.y < -camera_move_dist.y)
			camera_pos_target.y -= camera_move_dist.y;

		//show HP
		hp_red.text = "HP:"+states.HP1;
		hp_green.text = "HP:"+states.HP2;

		//show bullet num
		bullet_num_red.text = "bullet:"+states.bullet_num1;
		bullet_num_green.text = "bullet:"+states.bullet_num2;

		dist = Mathf.Max (200f, dist);
		camera.position = (new Vector3 (camera_pos_target.x, dist*1.5f, camera_pos_target.y) + 2f*camera.position) / 3f;
		//Debug.Log (bullets_info.Count);


	}

}
