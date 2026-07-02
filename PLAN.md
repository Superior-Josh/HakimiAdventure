# 哈基米大冒险 — 项目计划

> 基于项目现状审查（2026-07-03）制定的完整开发路线图。

---

## 🏗️ 已完成

当前项目已搭建了第一人称动作 RPG 的**地基**：

| 模块 | 状态 | 说明 |
|------|------|------|
| 物理引擎 | ✅ 完成 | MC 风格移动/冲刺/潜行/跳跃 + 鼠标自由视角 |
| 项目配置 | ✅ 完成 | Godot 4.7 C#、输入映射、Web/Git 就绪 |
| 主场景 | ✅ 完成 | 地面 + 方向光 + 玩家实例 |
| 战斗骨架 | ✅ 代码就位 | TurnManager + CombatStats（待接入游戏循环） |
| 对话骨架 | ✅ 代码就位 | DialogueManager（待创建 UI 场景并连线） |
| UI 骨架 | ✅ 代码就位 | UIManager（待创建 HUD 场景） |
| 存档骨架 | ✅ 代码就位 | SaveManager（检查点读/写框架完成） |
| 数据模型 | ✅ 已定义 | JobData / EnemyData Resource 可在编辑器填表 |

---

## 🏗️ 待实现（按优先级）

### 第一阶段：战斗系统

当前 TurnManager 是信号驱动的阶段切换，目标游戏需要**类国王密令即时体力制**。需重构。

| # | 任务 | 说明 |
|---|------|------|
| 1 | 重新设计战斗架构 | 阶段切换 → 实时攻击/格挡/闪避 + 体力条 |
| 2 | 实现体力条 | 攻击耗体、缓慢恢复、体力为 0 无法攻击 |
| 3 | 近战攻击 | 鼠标左键 → 挥砍判定 → 前摇/后摇/硬直 |
| 4 | 格挡/翻滚 | 右键格挡，Space（战斗中）翻滚闪避 |
| 5 | 敌人 AI | 巡逻 → 发现玩家 → 追踪 → 攻击 |
| 6 | 受击反馈 | 屏幕红晕 / 硬直 / 音效 |
| 7 | 死亡与惩罚 | HP 归零 → 触发存档惩罚 → 回检查点 |

### 第二阶段：对话与交互

| # | 任务 | 说明 |
|---|------|------|
| 8 | Dialogic 插件集成 | 安装 Dialogic 2，替代手写引擎 |
| 9 | 对话 UI 场景 | 对话框 CanvasLayer → 头像/文本/选项 |
| 10 | 分支变量系统 | 对话选择影响游戏状态 |
| 11 | NPC 交互触发 | F 键 → 射线检测 → 打开对话 |

### 第三阶段：UI / HUD

| # | 任务 | 说明 |
|---|------|------|
| 12 | HUD 场景 | HP 条、体力条、检查点名称 |
| 13 | 战斗 UI | 敌人 HP 条、武器/技能指示器 |
| 14 | 菜单系统 | 暂停菜单、装备栏、物品栏 |

### 第四阶段：关卡与内容

| # | 任务 | 说明 |
|---|------|------|
| 15 | 天空/环境 | WorldEnvironment 配置天空 + 雾效 |
| 16 | icon.svg | 创建项目图标 |
| 17 | 玩家模型 | Blender 做第一人称身体 |
| 18 | 第一个迷宫 | CSG / GridMap 搭建新手关 |
| 19 | 检查点实体 | 场景中放置 CheckPoint 区域 |
| 20 | 第 1 个 NPC | 模型 + 对话树 |
| 21 | 第 1 个敌人 | 史莱姆 / 骷髅 |
| 22 | 第 1 个 Boss | 特殊攻击模式 |

### 第五阶段：数据与配置

| # | 任务 | 说明 |
|---|------|------|
| 23 | 职业配置 | 用 JobData 配置 3 个职业（剑士/弓手/法师） |
| 24 | 敌人配置 | 用 EnemyData 配置 5 种敌人 |
| 25 | 物品系统 | ItemData Resource（武器/防具/消耗品） |
| 26 | 对话数据 | 第一个对话树 JSON |

---

## 📐 架构方向

### Combat — 即时体力制

```
当前：TurnManager → CombatStats（信号无人监听）

目标：PlayerController → CombatSystem
                            ├── StaminaBar
                            ├── WeaponController
                            ├── EnemyAI
                            └── DamageSystem
```

TurnManager 保留为**战斗/非战斗状态机**，不再驱动攻击节奏。

### Dialogue — 推荐 Dialogic 2 插件

手写引擎成本高。Dialogic 2 提供可视化对话树编辑器、打字机效果、分支变量，开箱即用。

---

## 🗺️ 里程碑

```
里程碑 1：可打架
  ├─ 即时体力制战斗
  ├─ 1 个敌人 + 攻击受击
  └─ 死亡回检查点
  📅 1 周

里程碑 2：可对话
  ├─ Dialogic 对话
  ├─ HUD 就位
  └─ 第 1 个 NPC
  📅 +1 周

里程碑 3：可探索
  ├─ 第一个迷宫关卡
  ├─ 1 个 Boss
  └─ 职业/装备初版
  📅 +2 周

里程碑 4：可发布
  ├─ 内容填充
  ├─ 音效/音乐/调优
  └─ 打包发布
  📅 1-2 个月
```

---

## 📊 文件变更预期

```
CREATE:
  ├── Assets/Textures/icon.svg
  ├── Scenes/UI/HUD.tscn
  ├── Scenes/UI/DialogueUI.tscn
  ├── Scenes/Levels/Maze01.tscn
  ├── Scenes/Actors/Enemy01.tscn
  ├── Scripts/Combat/StaminaSystem.cs
  ├── Scripts/Combat/WeaponController.cs
  ├── Scripts/Combat/DamageSystem.cs
  ├── Scripts/Enemy/EnemyAI.cs
  └── Resources/Items/ItemData.cs

MODIFY:
  ├── project.godot
  ├── Scripts/Combat/TurnManager.cs
  ├── Scripts/Combat/CombatStats.cs
  ├── Scenes/Main.tscn
  └── Scenes/Player.tscn
```
