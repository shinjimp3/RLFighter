using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using MathNet.Numerics.LinearAlgebra.Double;

public class AgentPlayer1 : Player {
	float[,] q_table = new float[Stat.max_idx, ActIdx.max_idx];

	float alpha = 0.01f, gamma = 0.9f;

	bool is_started = false;
	Stat stat, next_stat;
	ActIdx act_idx;

	public AgentPlayer1(bool isRed):base(isRed){		
		this.isRed = isRed;
		InitQTable();
	}

	void InitQTable() {
		for (int i = 0; i < Stat.max_idx; i++) {
			for (int j = 0; j < ActIdx.max_idx; j++) {
				q_table[i, j] = Random.value;
			}
		}
	}

	public Actions RunStep(States states){
		next_stat = new Stat(states, isRed);
		
		if (is_started) {
			float reward = Reward(stat, next_stat);
			Learn(stat, act_idx, next_stat, reward);
		}
		is_started = true;
		act_idx = Policy(next_stat);
		stat = next_stat;
		return act_idx.Actions(next_stat);
	}

	float Reward(Stat s, Stat next_s) {
		// if (s.DistIdx() == 2) {
		// 	return -10f;
		// }
		if (s.DirecIdx() == 1) {
			if (s.FaceIdx() == 3) {
				if (s.DistIdx() == 0) {
					return 30f;
				}
				return 10f;
			}
			return 5f;
		} else if (s.DirecIdx() == 4 && s.FaceIdx() == 1) {
			return -10f;
		}
		return 0f;
	}

	void Learn(Stat s, ActIdx a, Stat next_s, float reward) {
		QActIdx qa = MaxQActIdx(next_s);
		q_table[s.Idx(), a.Idx()] +=
			alpha * (reward + gamma * qa.q - q_table[s.Idx(), a.Idx()]);
	}

	ActIdx Policy(Stat s) {

		// FIXME
		if (Random.value < 0.2) {
			return new ActIdx(Random.Range(0, ActIdx.max_idx));
		}

		QActIdx qa = MaxQActIdx(s);
		return qa.a_idx;
	}

	QActIdx MaxQActIdx(Stat s) {
		int s_idx = s.Idx();

		float max_q = float.NegativeInfinity;
		ActIdx max_a = new ActIdx(0);
		for (int i = 0; i < ActIdx.max_idx; i++) {
			if (q_table[s_idx, i] > max_q) {
				max_q = q_table[s_idx, i];
				max_a = new ActIdx(i);
			}
		}
		return new QActIdx(max_q, max_a);
	}

	class QActIdx {
		public float q;
		public ActIdx a_idx;

		public QActIdx(float q, ActIdx a_idx) {
			this.q = q;
			this.a_idx = a_idx;
		}
	}

	class Stat {
		public static int dist_num = 3, direc_num = 6, face_num = 4;
		public static int max_idx = dist_num * direc_num * face_num - 1;

		public States s;
		public bool isRed;

		public Stat(States s, bool idRed) {
			this.s = s;
			this.isRed = isRed;
		}

		public int DistIdx() {
			float dist = Vector2.Distance(s.pos1, s.pos2);
			if (dist < 100f) {
				return 0;
			} else if (dist < 200f) {
				return 1;
			}
			return 2;
		}

		public int DirecIdx() {
			float theta;
			if (isRed) {
				theta = s.target_theta21;
			} else {
				theta = s.target_theta12;
			}

			if (theta < 70f) {
				return 0;
			} else if (theta < 110f) {
				return 1;
			} else if (theta < 180f) {
				return 2;
			} else if (theta < 250f) {
				return 3;
			} else if (theta < 290f) {
				return 4;
			}
			return 5;
		}

		public int FaceIdx() {
			float theta;
			if (isRed) {
				theta = s.target_theta12;
			} else {
				theta = s.target_theta21;
			}

			if (theta < 45f || 225f < theta) {
				return 0;
			} else if (theta < 135f) {
				return 1;
			} else if (theta < 225f) {
				return 2;
			}
			return 3;
		}

		public int Idx() {
			return dist_num * direc_num * FaceIdx() +
				dist_num * DirecIdx() +
				DistIdx();
		}
	}

	class ActIdx {
		public static int yaw_num = 3, speed_num = 2;
		public static int max_idx = yaw_num * speed_num - 1;

		int yaw_idx, speed_idx;

		public ActIdx(int yaw_idx, int speed_idx) {
			this.yaw_idx = yaw_idx;
			this.speed_idx = speed_idx;
		}

		public ActIdx(int idx) {
			this.yaw_idx = idx / speed_num;
			this.speed_idx = idx % speed_num;
		}

		public int Idx() {
			return speed_num * yaw_idx + speed_idx;
		}

		public Actions Actions(Stat s) {
			Actions a = new Actions();
			a.yaw = (float)yaw_idx / (float)(yaw_num - 1);
			a.speed = (float)speed_idx / (float)(speed_num - 1);

			int bullet_num;
			if (s.isRed) {
				bullet_num = s.s.bullet_num1;
			} else {
				bullet_num = s.s.bullet_num2;
			}

			if (bullet_num > 0 && s.DirecIdx() == 1) {
				a.shoot = true;
			} else {
				a.shoot = false;
			}
			return a;
		}
	}

}
