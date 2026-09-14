# GGJ 2026 上海站 · 节奏回怼大作战

> Global Game Jam 2026 · Shanghai Site
>
> 一款以"春节亲戚灵魂拷问"为主题的节奏对战游戏。在节拍中精准出招，用技术回怼、已读乱回、沉默吃饭三种方式，把催婚催工作的 Boss 怼到哑口无言！

[![Unity](https://img.shields.io/badge/Unity-2022.3.17f1c1-57B9E7?logo=unity)](https://unity.com/)
[![URP](https://img.shields.io/badge/Render%20Pipeline-URP-8B5CF6)](https://unity.com/srp/universal-render-pipeline)
[![Game Jam](https://img.shields.io/badge/Global%20Game%20Jam-2026-FF6B6B)](https://globalgamejam.org/)

---

## 📺 游戏演示

<a href="https://www.bilibili.com/video/BV1j3tq6BErL/">
  <img src="https://img.shields.io/badge/B站-观看演示-00A1D6?logo=bilibili&logoColor=white" alt="Bilibili" height="28">
</a>

---

## 🎮 游戏简介

过年回家，最怕的不是春运，而是亲戚的连环拷问——

> "咋又一个人回来的？"
> "啥时候找对象啊？"
> "女人过三十就贬值啦！"
> "年终奖多少呀？"
> "考个公，光宗耀祖！"

在这款节奏游戏中，你将面对一位火力全开的"亲戚 Boss"。她的每一句灵魂拷问都会化作弹幕袭来，而你需要踩着节拍，用三种方式霸气回怼：

| 按键 | 招式 | 效果 |
|:---:|------|------|
| **Q** | 🔪 技术回怼 | 高伤害反击，但自身也会受到反噬伤害 |
| **W** | 💬 已读乱回 | 稳定输出伤害，随机播放搞笑回复 |
| **E** | 🍚 沉默吃饭 | 不攻击，但回复自身生命值 |

把 Boss 的血量怼到零，你就是年夜饭桌上最靓的仔！

---

## 🕹️ 玩法说明

### 核心机制

- **节拍条**：屏幕上方有 7 个"锅盖"节拍点，时间条从左向右匀速移动
- **出招时机**：当时间条到达**没有盖子**的节拍点时，按下 Q / W / E 进行回怼
- **判定等级**：根据按键与节拍的偏差时间判定
  - **Perfect** ≤ 30ms — 最高伤害 / 最高回复
  - **Great** ≤ 80ms — 正常效果
  - **Good** ≤ 120ms — 效果减弱
  - **Miss** — 错过节拍，无效果
- **血量系统**：玩家和 Boss 各有血量，任一方归零则游戏结束
- **弹幕对话**：Boss 的拷问和玩家的回怼都会以弹幕形式飞过屏幕

### 操作

| 按键 | 功能 |
|:---:|------|
| `Q` | 技术回怼（高伤 + 自损） |
| `W` | 已读乱回（稳定伤害） |
| `E` | 沉默吃饭（回复生命） |
| `ESC` | 暂停 / 继续 |
| `F12` | 重置新手教程记录 |

---

## 📂 关卡内容

### 第一关：催婚风暴

Boss 围绕"找对象、结婚、生孩子"展开 15 轮连环拷问：

- "咋又一个人回来的？"
- "啥时候找对象啊？"
- "女人过三十就贬值啦！"
- "你高中同学都有娃了！"
- "不孝有三，无后为大！"
- ……

玩家的经典回怼：

- "不然半扇人回来？"
- "等我找到我的另半扇"
- "全靠大姨衬托"
- "喜欢有家的"
- ……

### 第二关：职场PUA

Boss 切换到"工作、赚钱、考公"主题，21 轮灵魂拷问：

- "还在那小地方上班呢？"
- "累成这样，年终奖多少呀"
- "私企要倒闭的！"
- "没编制咋办？"
- "考个公，光宗耀祖！"
- "这样混，书都白读了！"
- ……

---

## 🏗️ 技术架构

### 引擎与工具

- **引擎**：Unity 2022.3.17f1c1
- **渲染管线**：Universal Render Pipeline (URP) 14.0.9
- **UI 框架**：UGUI + TextMeshPro 3.0.6
- **动画**：DOTween
- **音频**：Unity 原生 AudioSource + 对象池管理
- **数据**：JSON 配置驱动（JsonUtility）

### 项目结构

```
GGJ/
├── Assets/
│   ├── Scripts/
│   │   ├── BeatBar.cs              # 核心节奏引擎（时间条、判定、BPM）
│   │   ├── BeatListener.cs         # 玩家输入处理 & 战斗逻辑
│   │   ├── PotManager.cs           # 7 个节拍锅盖的显隐管理
│   │   ├── FaceManager.cs          # 玩家表情切换
│   │   ├── RoundController.cs      # 回合数 UI 显示
│   │   ├── JudgementResult.cs      # 判定结果数据结构
│   │   ├── BeatHitData.cs          # 节拍事件数据
│   │   ├── StageEndData.cs         # 关卡结算数据
│   │   ├── Data/
│   │   │   ├── DataStruct.cs       # JSON 数据模型 & 加载器
│   │   │   ├── ManipulateType.cs   # 招式类型 & 伤害配置表
│   │   │   ├── EmojiDataSturct.cs  # 表情包数据
│   │   │   └── HealthUnit.cs       # 血量单位（可绑定事件）
│   │   ├── Level/
│   │   │   └── LevelBlackBoard.cs  # 关卡数据黑板（查询节拍/事件）
│   │   ├── Sound/
│   │   │   ├── AudioManager.cs     # 音频管理器（BGM/SFX/语音池）
│   │   │   └── SoundData.cs        # 音效数据
│   │   ├── Structure/
│   │   │   ├── EventCenter/        # 事件中心（发布-订阅）
│   │   │   ├── Singleton/          # 单例基类
│   │   │   ├── GameEnums/          # 场景枚举
│   │   │   └── BindableProperty.cs # 可绑定属性
│   │   ├── UI/
│   │   │   ├── UIManager.cs        # UI 总管理器
│   │   │   ├── GamePlayPanel.cs    # 战斗面板（血条/弹幕/表情包）
│   │   │   ├── StartGamePanel.cs   # 开始面板
│   │   │   ├── ResultPanel.cs      # 结算面板
│   │   │   ├── PausePanel.cs       # 暂停面板
│   │   │   ├── SettingsPanel.cs    # 设置面板
│   │   │   ├── TutorialPanel.cs    # 新手教程
│   │   │   ├── DanmakuBullet.cs    # 弹幕子弹
│   │   │   ├── EmojiObject.cs      # 表情包对象
│   │   │   └── Effects/            # 相机震动、故障动画等
│   │   └── 事件接收指南.md          # 事件系统开发文档
│   ├── Resources/
│   │   ├── Stage1.json             # 第一关节拍配置
│   │   ├── Stage2.json             # 第二关节拍配置
│   │   └── Event.json              # 全部对话事件表
│   ├── Scenes/
│   │   ├── UITest01.unity          # 标题 / 开始界面
│   │   ├── SampleScene.unity       # 第一关
│   │   ├── Level02.unity           # 第二关
│   │   └── UITest02.unity          # 测试场景
│   ├── Prefabs/                    # UI 预制体
│   ├── Images/                     # 美术资源（背景、角色、表情包）
│   ├── SoundClips/                 # BGM、音效、语音
│   └── Arts/                       # 艺术源文件
├── Packages/manifest.json          # 依赖包清单
└── ProjectSettings/                # 项目设置
```

### 核心设计

**1. 事件驱动架构**

游戏各系统通过 `EventCenter` 解耦，核心事件包括：

| 事件 | 参数 | 触发时机 |
|------|------|----------|
| `OnBeatHit` | `BeatHitData` | 时间条到达节拍点 |
| `OnPlayerJudgement` | `JudgementResult` | 玩家输入被判定 |
| `OnBossAction` | `string` | Boss 发起拷问弹幕 |
| `OnPlayVoice` | `int` | 播放特定语音 |
| `OnShowEmoji` | `string` | 显示表情包 |
| `OnDoDMG` | `int` | 对 Boss 造成伤害 |
| `OnCure` | `int` | 玩家回复 / 受伤 |
| `OnStageEnd` | `StageEndData` | 关卡结束结算 |

**2. 高精度节奏同步**

- 使用 `AudioSettings.dspTime` 作为时间基准，避免 `Time.deltaTime` 波动
- 基于理论时间累加计算循环起点，消除误差累积
- 支持音频延迟补偿（`audioLatencyMs`），提前调度音效
- 暂停 / 恢复时自动修正时间轴偏移

**3. JSON 数据驱动关卡**

每个关卡由 `StageN.json` 定义每一小节（Bar）的：
- `BossBeat` / `BossAEvent`：Boss 出招节拍 & 对话事件 ID
- `PlayerBeat` / `PlayerEvent`：玩家可出招节拍 & 回怼事件 ID
- `Note`：设计备注

`Event.json` 统一管理所有对话文本和对应语音文件名，便于文案调整和本地化。

**4. 伤害结算表**

```
招式 × 类别(Specific/Generic) × 质量(Perfect/Normal/Fail) → 伤害/回复
```

例如：技术回怼(Q) 在特定事件 Perfect 时造成 5 点 Boss 伤害 + 5 点自损；沉默吃饭(E) 在特定事件 Perfect 时回复 20 点生命。

---

## 🚀 快速开始

### 环境要求

- Unity Hub
- Unity **2022.3.17f1c1**（通过 Unity Hub 安装，项目打开时会自动提示）
- Windows / macOS

### 运行步骤

1. **克隆仓库**
   ```bash
   git clone https://github.com/Ahri-MIko/GGJ2026-1_SHANGHAI.git
   ```

2. **用 Unity Hub 打开项目**
   - 打开 Unity Hub → Add → 选择 `GGJ2026-1_SHANGHAI/GGJ` 文件夹
   - 等待 Unity 自动导入资源和编译脚本

3. **打开起始场景**
   - 在 Project 窗口中找到 `Assets/Scenes/UITest01.unity`
   - 双击打开，点击 ▶️ Play 按钮开始游戏

4. **直接进入关卡**（跳过标题界面）
   - 打开 `Assets/Scenes/SampleScene.unity`（第一关）或 `Level02.unity`（第二关）
   - 点击 Play 直接进入战斗

### 自定义关卡

1. 复制 `Assets/Resources/Stage1.json`，命名为 `Stage3.json`
2. 按照 JSON 结构编辑每小节的 Boss 和玩家节拍配置
3. 在 `Event.json` 中添加新的对话事件（EventId 不重复即可）
4. 在场景中将 `LevelBlackBoard` 的 `CurrentLevelID` 改为 `3`
5. 参考 `GGJ/示例关卡配置.txt` 了解基础节奏参数

---

## 👥 团队成员

| 成员 | 职责 |
|------|------|
| **zrcheng** | 核心节奏引擎、判定系统、关卡数据、音频逻辑 |
| **YourTiming** | UI 系统、弹幕、表情包、面板管理、结算界面 |
| **Ahri-MIko** | 项目管理 / 仓库维护 |

> 本项目为 Global Game Jam 2026 上海站 48 小时极限开发作品。

---

## 📄 许可证

本项目仅供学习和交流使用。美术、音频等资源版权归原作者所有，未经许可请勿用于商业用途。

---

## 🙏 致谢

- Global Game Jam 组委会及上海站志愿者
- 所有为游戏配音的小伙伴
- Unity 引擎 & DOTween & TextMeshPro 等开源工具
