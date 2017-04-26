# RLFighter

専攻公開用デモの候補です。  
2D戦闘機ゲームを題材として，強化学習ができる環境が構築されています。  

2機の戦闘機は2次元平面上で動きます。  
エージェントは出力として，速度，旋回速度，射撃の有無を決められます。  
戦闘機は円形の当たり判定を持ちます。  
弾が当たると当たった機体のHPが減りますが，現在（2017/04/26）HPが0になったときの処理はありません。  
敵のHPを自分より少なくすることを考えてエージェントを構築してみて下さい。

## ゲームの操作方法
左上の2つのドロップダウンで，各機体（赤，緑）にPlayerを割り当てます。  
Playerの内，Agent Player1, Agent Player2は後述するAgentPlayer1.cs, AgentPlayer2.csによって動作します。
Random Playerはランダムに，Hand Coded Playerはハンドコーディングで動作します。
Human PlayerはWASDキーで左右旋回と速度の設定ができます。Fキーで弾を撃ちます。
Iteration Startボタンでゲームが始まります。
Vキーを押すことで描画の有無を切り替えます。　　
描画をするとリアルタイムにゲームが動きますが，描画を切ると高速でstepが進みます。

## 各スクリプト・クラスについて
### AgentPlayer1.cs, AgentPlayer2.cs  
このクラスを編集することでエージェントを構築して下さい。  
環境とのインタラクションはRunStepメソッドを用いて行います。
RunStepメソッドは状態（Statesクラス）を引数に取り，行動（Actionsクラス）を返します。
ゲーム内時間0.1s毎に呼びだされ，その後環境が状態遷移します。
ランダムな行動を取らせるならRunStepメソッドを以下のようにしましょう。
```
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
```
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
    
    //報酬関数 R(s,a,s')
    //報酬は環境ではなくエージェントに設定してもらいます。
    reward = CalcReward (obs_states, actions, next_obs_states);
    
    //QtableとかValueFunctionとか更新
    Learn (obs_states, actions, next_obs_states, reward);
		
    //方策 a=pi(s)
    actions = Policy (next_obs_states);
		
    //次のstepに備えてPlayerクラスから継承した obs_states に next_obs_states を代入
    obs_states = next_obs_states; 
    
    return actions;
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
弾数の制限がまだ無いため，常時trueにして無限に撃っても問題ありません。

### States クラス
以下のようなフィールドを持っています。  
#### *Vector2* pos1, pos2  
それぞれ各機体（赤, 緑）のワールド座標。  
画面を見て 右がx，上がy。
#### *float* theta1, theta2  
それぞれ各機体（赤, 緑）のワールド角度（向いている方向）。  
画面を見て 右を0度として 左回りに360度まで定義されている。
#### *int* HP1, HP2
それぞれ各機体（赤, 緑）の体力。  
現在（2017/04/25）HPが0になっても機体は死なない。
#### *int* bullet_num1, bullet_num2  
未使用。  
#### *bool* isShooting1, isShooting2  
それぞれ各機体（赤, 緑）が弾を撃っているかどうか。  
#### *bool* isDamaged1, isDamaged2  
それぞれ各機体（赤, 緑）に弾が当たっているかどうか。  
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
