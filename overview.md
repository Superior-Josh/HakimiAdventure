# 哈基米大冒险 — 技术决策记录

> 项目重启前的最终技术栈与架构记录。

---

## 🎮 游戏定义

**类国王密令第一人称即时动作 RPG**

- 3D 第一人称视角
- 即时体力制战斗（非回合制）
  - 多种职业/流派，近战/远程
- 迷宫探索玩法
  - 非随机地图，即精心设计的线性地图
  - 地图包含检查点和 NPC
  - 流程包含普通敌人与 BOSS
  - 死亡惩罚
- 分支对话系统
  - 对话选项影响流程走向

---

## 🛠️ 技术栈（最终确定）

| 层 | 技术 | 说明 |
|----|------|------|
| **引擎** | Godot 4.7 Mono | C# 支持版本 |
| **语言** | C# (.NET 8.0) | 强类型，AI 辅助友好 |
| **3D 建模** | Blender | 开源免费 |
| **2D/贴图** | Krita / GIMP | 开源免费 |
| **音效** | Audacity + sfxr | 开源免费 |
| **版本控制** | Git + GitHub | 已配置 |
| **对话插件** | Dialogic 2（待集成） | Godot 4 可视化对话树编辑器 |

---

## 🏗️ 架构设计

### 项目目录结构

```
HakimiAdventure/
├── project.godot
├── HakimiAdventure.csproj
├── Scripts/
│   ├── Core/           # GameManager, SaveManager
│   ├── Player/         # PlayerController
│   ├── Combat/         # CombatSystem, StaminaSystem, WeaponController, DamageSystem
│   ├── Enemy/          # EnemyAI
│   └── UI/             # UIManager, HUD
├── Scenes/
│   ├── Main.tscn
│   ├── Levels/         # 迷宫关卡
│   └── Actors/         # 玩家、敌人、NPC 预制体
├── Resources/
│   ├── Jobs/           # 职业数据
│   ├── Enemies/        # 敌人数据
│   └── Items/          # 物品数据
└── Assets/
    ├── Textures/
    ├── Models/
    └── Audio/
```

### 核心系统

#### 战斗系统 — 即时体力制

```
PlayerController (输入)
    ↓
CombatSystem (状态机)
    ├── StaminaBar      — 攻击耗体，缓慢恢复
    ├── WeaponController — 挥砍判定，前摇/后摇/硬直
    ├── EnemyAI          — 巡逻/追踪/攻击行为树
    └── DamageSystem     — 伤害计算 + 受击反馈
```

关键设计：
- 非回合制，实时发生
- 体力 = 核心资源，管理攻防节奏
- 攻击有前摇动作，可被敌人打断
- 死亡 → 扣金币 → 回检查点

#### 对话系统 — Dialogic 2

- 可视化对话树编辑器
- 分支变量（影响游戏状态）
- 打字机效果 + 头像
- NPC 交互：F 键 → 射线检测 → 触发对话

#### 存档系统

- ConfigFile 格式
- 检查点制（场景中放置 CheckPoint 区域）
- 存储内容：位置/朝向/HP/MP/金币/物品
- 死亡惩罚：损失 10% 金币

---

## 🗺️ 开发路线

1. **战斗原型** — 即时体力制 + 1 个敌人 + 攻击受击 + 死亡回检
2. **对话交互** — Dialogic 接入 + NPC + HUD
3. **迷宫关卡** — 第一个迷宫 + Boss + 职业/装备初版
4. **内容填充** — 多职业/多敌人/完整流程
5. **打磨发布** — 音效/画面/性能/打包

---

## 📝 历史

| 日期 | 事件 |
|------|------|
| 2026-07-03 | 初始项目搭建（Godot 4.7 + C#）|
| 2026-07-03 | 物理引擎完成（MC 风格）|
| 2026-07-03 | 战斗/对话/UI/存档 骨架代码完成 |
| 2026-07-03 | 项目重启，清理至单文档重新开始 |
