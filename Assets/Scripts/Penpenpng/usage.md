# Agent Strategy
RLFighterのAIを実装するためのクラス群

## Usage
`using Assets.Scripts.Penpenpng;`して、AgentPlayer1.cs(またはAgentPlayer2.cs)を以下の通り書き換えます/

1. `readonly Strategy strategy`フィールドを加える
1. コンストラクタ内で`strategy`を初期化
1. `RunStep()`内を`return strategy.RunStep(states);`とする

## Example
AgentPlayer2.cs

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts.Penpenpng;

public class AgentPlayer2 : Player
{
    readonly Strategy strategy;

    public AgentPlayer2(bool isRed) : base(isRed)
    {
        actions = new Actions();
        this.isRed = isRed;

        //状態空間、行動空間、報酬関数を与えて初期化
        strategy = new SimpleQTable<State1, Action1>(
            (s1, a, s2) =>
            {
                float reward = 0;
                //弾の溜めすぎや使いすぎはよくないこと
                if (s2.RawState.bullet_num2 < 1 || 9 < s2.RawState.bullet_num2) reward += -1;
                //離れ過ぎはよくないこと
                if (s1.RawState.rel_pos12.magnitude < s2.RawState.rel_pos12.magnitude) reward += -1;
                //自分が相手を向いているのはよいこと
                if (s2.AbsPhi == 0) reward += 1;
                //相手がダメージを受けるのはよいこと
                if (s2.RawState.isDamaged1) reward += 10;
                //自分がダメージを受けるのはよくないこと
                if (s2.RawState.isDamaged2) reward += -10;
                return reward;
            });
    }

    new public Actions RunStep(States states) { return strategy.RunStep(states); }
}
```