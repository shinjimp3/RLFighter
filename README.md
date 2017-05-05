# RLFighter

専攻公開用デモの候補です。  
2D戦闘機ゲームを題材として，強化学習ができる環境が構築されています。  

2機の戦闘機は2次元平面上で動きます。  
エージェントは出力として，速度，旋回速度，射撃の有無を決められます。  
戦闘機は円形の当たり判定を持ちます。  

以下のAssetを各自でダウンロードし，Assetフォルダ直下に置いてください。<br>
戦闘機3Dモデル：https://www.assetstore.unity3d.com/jp/#!/content/52212<br>
背景：https://www.assetstore.unity3d.com/en/#!/content/25582<br>

## ゲームの操作方法
左上の2つのトグルで，訓練モードかどうか，エンドレスルールかどうかを決めます。<br>
左上の2つのドロップダウンで，各機体（赤，緑）にPlayerを割り当てます。<br> 
Playerの内，Agent Player1, Agent Player2は後述するAgentPlayer1.cs, AgentPlayer2.csによって動作します。<br>
Random Playerはランダムに，Hand Coded Playerはハンドコーディングで動作します。<br>
Human PlayerはWASDキーで左右旋回と速度の設定ができます。Fキーで弾を撃ちます。<br>
Iteration Startボタンでゲームが始まります。<br>
Vキーを押すことで描画の有無を切り替えます。<br>
描画をするとリアルタイムにゲームが動きますが，描画を切ると高速でstepが進みます。<br>

## 各スクリプト・クラスについて
### AgentPlayer1.cs, AgentPlayer2.cs  
このクラスを編集することでエージェントを構築して下さい。<br>
環境とのインタラクションはRunStepメソッドを用いて行います。<br>
RunStepメソッドは状態（Statesクラス）を引数に取り，行動（Actionsクラス）を返します。<br>
ゲーム内時間0.1s毎に呼びだされ，その後環境が状態遷移します。<br>
ランダムな行動を取らせるならRunStepメソッドを以下のようにしましょう。<br>
```csharp
public Actions RunStep(States states){
    //Playerクラスから継承した actions に適当な行動を設定する。
    actions.yaw = Random.value;
    actions.speed = Random.value;
    actions.shoot = Random.value < 0.8f;
    actions = Policy (next_obs_states);
		
    return actions;
}
```
もう少しマシな強化学習エージェントにしたければ以下のようにしましょう。
```csharp
public Actions RunStep(States states){
    //制御周期の設定
    //環境はゲーム内時間0.1s毎とかなり細かめに状態を遷移させますが，エージェントの制御周期をこれに合わせる必要はありません。
    //0.1sのstep_width（整数）倍でエージェントを動かすこともできます。この設定をいじってSMDPにしても良いでしょう。
    if (passed_time < step_width) {
	passed_time++;
	return actions;
    }
    passed_time = 0;
		
    //Playerクラスから継承した next_obs_states に 環境から与えられた states を代入
    //ここで内部状態を定義するメソッドを挟んでも良い。（位置を離散化するとか）
    next_obs_states = states;
    
    if (episode_started) { //エピソード開始時はs,s'の2つが定義できないため，スルー
	    //報酬関数 R(s,a,s')
	    //報酬は環境ではなくエージェントに設定してもらいます。
	    reward = CalcReward (obs_states, actions, next_obs_states);

	    //QtableとかValueFunctionとか更新
	    Learn (obs_states, actions, next_obs_states, reward);
    }
    episode_started = true;
    
    //方策 a=pi(s)
    actions = Policy (next_obs_states);
		
    //次のstepに備えてPlayerクラスから継承した obs_states に next_obs_states を代入
    obs_states = next_obs_states; 
    
    return actions;
}
```
エピソード終了時の処理はEndEpisodeメソッドに書いてください。<br>
EndEpisodeメソッドは状態（Statesクラス）を引数に取ります。<br>
「どちらかが死んだ後，環境がリセットされる前に勝ち負けを価値関数に反映させる」といった使い方を想定しています。<br>
EndEpisodeメソッドには，Q-learningの
<img src="https://latex.codecogs.com/gif.latex?r&space;&plus;&space;\gamma&space;\max&space;Q(s',a')" />
を
<img src="https://latex.codecogs.com/gif.latex?r" />
に変える，
初期化するときの状態遷移を学習しないようにepisode_startedをfalseにするなどの処理を，以下のように書き込むと良いでしょう。
```csharp
public void EndEpisode(States states){
	//状態を離散値に
	int state_i = State2Index (states);
	float alpha = 0.1f;
	float gamma = 0.9f;
	float reward = 0f;
	//勝ったら報酬1
	if ((isRed && states.HP2 <= 0 )|| (!isRed && states.HP1 <= 0))
		reward = 1f;
	//負けたら報酬-1
	if ((!isRed && states.HP2 <= 0)|| (isRed && states.HP1 <= 0))
		reward = -1f;
	//即時報酬だけで価値関数を更新(エピソードが終了するため将来の報酬が0だから)
	Qtable [state_i, action_index] += alpha * (reward - Qtable [state_i, action_index]);
	//初期化するときの状態遷移(s -> s0)を使って学習させない。
	episode_started = false;
}

```

### Actions クラス
以下のようなフィールドを持っています。  
#### *float* yaw  
ヨー方向への旋回速度を指定できます。値は0~1で指定します。  
0で左いっぱい，0.5でニュートラル，1で右いっぱいに旋回します。
#### *float* speed  
前進する速度を指定できます。値は0~1で指定します。
0で最低速度，1で最高速度になります。
#### *bool* shoot
弾を撃つかどうか指定できます。  

### States クラス
以下のようなフィールドを持っています。  
#### *bool* train  
訓練モードかどうか。  
ゲームの挙動には関わらない値ですが，trueなら探索や価値関数の更新をするように，
falseなら価値観数の更新を止めgreedyな行動選択をするようにしてください。  
「trainをtrueにしてしばらく学習をさせてから，trainをfalseに切り替えて学習の結果を見る」  
といったような使い方を想定しています。
#### *bool* endless  
エンドレスルールが適用されているかどうか。  
基本は適用されている前提で行きましょう。
#### *int* step_i  
現在のエピソードで何ステップ経過したか。  
#### *int* episode_i  
何エピソード経過したか。  
学習率や温度係数の更新に用いると良いでしょう。
#### *Vector2* pos1, pos2  
それぞれ各機体（赤, 緑）のワールド座標。  
画面を見て 右がx，上がy。
#### *float* theta1, theta2  
それぞれ各機体（赤, 緑）のワールド角度（向いている方向）。  
画面を見て 右を0度として 左回りに360度まで定義されている。
#### *int* HP1, HP2
それぞれ各機体（赤, 緑）の体力。  
#### *int* bullet_num1, bullet_num2  
それぞれ各機体（赤, 緑）の残り弾数。
#### *bool* isShooting1, isShooting2  
それぞれ各機体（赤, 緑）が弾を撃っているかどうか。  
#### *bool* isDamaged1, isDamaged2  
それぞれ各機体（赤, 緑）に弾が当たっているかどうか。  
#### *bool* isDamaged1Before, isDamaged2Before  
それぞれ各機体（赤, 緑）に弾が当たっていた場合，それが何step前に発射された弾によるものか。  
弾が当たっていない場合，この値は更新されない。
#### *List\<Bullet\>* bullets_info;
発射された弾丸の情報が格納されているリスト。  
Bulletクラスには以下のフィールドが存在
* *Vector2* pos  
 弾丸のワールド座標
* *float* theta  
 弾丸の飛んでいくワールド角度
* *int* life  
 弾丸が自動消滅するまでのステップ数
* *bool* isRed  
 機体赤から発射されたかどうか
#### *Vector2* rel_pos12, rel_pos21  
rel_pos12は機体2（緑）から見た機体1（赤）のワールド座標の相対値。  
rel_pos21はその逆。観測側の機体の角度は考慮していない。  
![rel_pos](https://github.com/shinjimp3/RLFighter/blob/master/rel_pos.png?raw=true)

#### *float* target_theta12, target_theta21
target_theta12は機体2（緑）から機体1（赤）がどのように見えるかという値。
target_thteta21はその逆。  
観測側の機体から見て右側を0度として 左回りに360度まで定義されている。
![target_theta](https://github.com/shinjimp3/RLFighter/blob/master/target_theta.png?raw=true)

#### *float* rel_theta12, rel_theta21  
rel_theta12は機体2（緑）から見た機体1（赤）の角度の相対値。  
rel_theta21はその逆。  
観測側の機体から見て右側を0度として 左回りに360度まで定義されている。  
![rel_theta](https://github.com/shinjimp3/RLFighter/blob/master/rel_theta.png?raw=true)
