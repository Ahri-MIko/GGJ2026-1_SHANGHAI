using System;
using System.Collections.Generic;

public enum ActionType { 技术回怼, 已读乱回, 沉默吃饭 }
public enum Quality { Perfect, Normal, Fail }
public enum Category { Specific, Generic } // 对应“针对具体语音”和“括号空项”

public struct Effect
{
    public int EnemyDamage; // 对敌方伤害
    public int SelfDamage;  // 自己受到的伤害 (负数表示回复)

    public Effect(int enemyDamage, int selfDamage)
    {
        EnemyDamage = enemyDamage;
        SelfDamage = selfDamage;
    }
}

public static class GameConfig
{
    public static readonly Dictionary<ActionType, Dictionary<Category, Dictionary<Quality, Effect>>> Data =
        new Dictionary<ActionType, Dictionary<Category, Dictionary<Quality, Effect>>>
    {
        {
            ActionType.技术回怼, new Dictionary<Category, Dictionary<Quality, Effect>> {
                { Category.Specific, new Dictionary<Quality, Effect> {
                    { Quality.Perfect, new Effect(10, 5) },
                    { Quality.Normal,  new Effect(5, 5) },
                    { Quality.Fail,    new Effect(0, 15) }
                }},
                { Category.Generic, new Dictionary<Quality, Effect> {
                    { Quality.Perfect, new Effect(5, 0) },
                    { Quality.Normal,  new Effect(2, 0) },
                    { Quality.Fail,    new Effect(0, 0) }
                }}
            }
        },
        {
            ActionType.已读乱回, new Dictionary<Category, Dictionary<Quality, Effect>> {
                { Category.Specific, new Dictionary<Quality, Effect> {
                    { Quality.Perfect, new Effect(5, 0) },
                    { Quality.Normal,  new Effect(3, 0) },
                    { Quality.Fail,    new Effect(0, 0) }
                }},
                { Category.Generic, new Dictionary<Quality, Effect> {
                    { Quality.Perfect, new Effect(5, 0) },
                    { Quality.Normal,  new Effect(2, 0) },
                    { Quality.Fail,    new Effect(0, 5) }
                }}
            }
        },
        {
            ActionType.沉默吃饭, new Dictionary<Category, Dictionary<Quality, Effect>> {
                { Category.Specific, new Dictionary<Quality, Effect> {
                    { Quality.Perfect, new Effect(0, -20) }, // 回复20记为-20自伤
                    { Quality.Normal,  new Effect(0, -10) },
                    { Quality.Fail,    new Effect(0, 0) }
                }},
                { Category.Generic, new Dictionary<Quality, Effect> {
                    { Quality.Perfect, new Effect(0, -10) },
                    { Quality.Normal,  new Effect(0, -5) },
                    { Quality.Fail,    new Effect(0, 0) }
                }}
            }
        }
    };
}