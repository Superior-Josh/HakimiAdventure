# 哈基米大冒险 — 技术决策记录

> 类国王密令第一人称即时动作 RPG · 技术栈与架构设计文档

---

## 🎮 游戏定义

**第一人称即时动作 RPG**

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

## 🛠️ 技术栈

| 层 | 技术 | 说明 |
|----|------|------|
| **引擎** | Godot 4.7 Mono | C# 支持版本 |
| **语言** | C# (.NET 8.0) | 强类型，AI 辅助友好。核心系统全部 C# |
| **动画** | C# 字符串驱动 | 用 `AnimationPlayer.Play("name")` 手动管理，降低学习成本并便于 AI 辅助 |
| **3D 建模** | Blender | 开源免费 |
| **2D/贴图** | Krita / GIMP | 开源免费 |
| **音效** | Audacity + sfxr | 开源免费 |
| **对话系统** | 自写 C# Resource 驱动 | MVP 阶段自写极简对话系统，用 .tres/.json 资源文件驱动，后期评估是否集成 Dialogic |
| **资产策略** | 免费资产先做原型 | MVP 使用 Kenney / Godot Asset Library / Itch.io 免费包快速搭建原型，确定玩法后逐个替换为自制模型 |
| **版本控制** | Git + GitHub | 已配置 |

---

## 🏗️ 架构设计

### 项目目录结构

```
HakimiAdventure/
├── project.godot
├── HakimiAdventure.csproj
├── Scripts/
│   ├── Core/           # GameManager, SaveManager
│   ├── Player/         # PlayerController, CameraController
│   ├── Combat/         # CombatSystem, StaminaSystem, WeaponController, DamageSystem, LockOnSystem
│   ├── Magic/          # MagicSystem, SpellData
│   ├── Inventory/      # InventorySystem, ItemData
│   ├── Growth/         # LevelSystem, AttributeSystem
│   ├── VFX/            # VFXManager, ParticleEffectController
│   ├── Audio/          # AudioManager, AudioBusConfig
│   ├── Enemy/          # EnemyAI
│   ├── Dialogue/       # DialogueSystem
│   └── UI/             # UIManager, HUD, InventoryUI, MenuUI, SettingsUI
├── Scenes/
│   ├── Main.tscn
│   ├── Levels/
│   └── Actors/         # 玩家、敌人、NPC 预制体
├── Resources/
│   ├── Jobs/           # 职业数据
│   ├── Enemies/        # 敌人数据
│   ├── Items/          # 物品数据
│   ├── Spells/         # 法术数据
│   ├── Dialogues/      # 对话资源文件
│   └── VFX/            # 特效资源
└── Assets/
    ├── Textures/
    ├── Models/
    └── Audio/
```

---

## 🔧 核心系统

### 1. 战斗系统 — 即时体力制

```
PlayerController (输入)
    ↓
CombatSystem (状态机)
    ├── LockOnSystem        — 锁定/切换目标，相机跟随
    ├── StaminaBar          — 攻击耗体，缓慢恢复
    ├── WeaponController    — 挥砍判定，前摇/后摇/硬直
    ├── EnemyAI             — 巡逻/追踪/攻击行为树
    └── DamageSystem        — 伤害计算 + 受击反馈
```

- 非回合制，实时发生
- 体力 = 核心资源，管理攻防节奏
- 攻击有前摇动作，可被敌人打断
- 锁定系统：近战自动面向锁定敌人，远程半自动吸附
- 死亡 → 扣金币 → 回检查点

### 2. 法术/技能系统

```
MagicSystem
    ├── SpellData (Resource)  — 法术配置（消耗 MP/体力、伤害、冷却）
    ├── SpellController       — 施法前摇/后摇、弹道/范围
    ├── CooldownManager       — 各法术独立冷却
    └── VFXManager            — 法术粒子/光效/命中特效
```

- 体力 + MP 双资源管理
- 不同职业/流派有不同技能树
- 冷却系统 + 施法硬直

### 3. 背包系统

```
InventorySystem
    ├── ItemData (Resource)       — 物品配置（类型/效果/描述/图标）
    ├── InventoryGrid             — 有限格子背包
    ├── ItemActions               — 使用/装备/丢弃
    └── InventoryUI               — 网格/列表视图，分类筛选
```

- 有限格子管理
- 可拾取、使用、装备、丢弃
- 装备影响角色属性（武器/防具）

### 4. 角色成长

```
LevelSystem
    ├── ExperienceManager     — 杀敌获得经验
    ├── LevelUpController     — 升级时加点（HP/MP/力量/敏捷等）
    ├── AttributeSystem       — 基础属性计算
    └── EquipmentBonus        — 装备附加属性
```

- 经验值升级
- 每次升级可分配属性点
- 装备提供额外属性加成

### 5. 动画系统 — C# 字符串驱动

```
PlayerController / EnemyAI / WeaponController
    ↓
AnimationPlayer (场景节点)
    ↓
AnimationLibrary (.res 资源)
```

- 动画文件放在 `AnimationLibrary` 资源中
- C# 通过 `AnimationPlayer.Play("animation_name")` 调用
- 关键事件（攻击判定帧、音效触发帧）通过 **Call Method Track** 或 **Audio Track** 实现
- 用 `enum CharacterState` + `switch` 手动控制切换，无需 AnimationTree

```csharp
public enum CharacterState { Idle, Walk, Attack, Hit, Death }

private void UpdateAnimation(CharacterState state) {
    switch (state) {
        case CharacterState.Idle:    animPlayer.Play("idle");    break;
        case CharacterState.Walk:    animPlayer.Play("walk");    break;
        case CharacterState.Attack:  animPlayer.Play("attack");  break;
        case CharacterState.Hit:     animPlayer.Play("hit");     break;
        case CharacterState.Death:   animPlayer.Play("death");   break;
    }
}
```

### 6. 对话系统

```
DialogueSystem
    ├── DialogueResource (Resource)  — 节点ID + 文本 + 选项 + 分支跳转
    ├── DialogueController           — 打字机效果 + 选项显示 + 状态影响
    ├── DialogueTrigger              — 场景触发（F键射线检测）
    └── DialogueUI                   — 对话框 + 头像 + 选项按钮
```

- `Resource` 子类存储对话树数据，JSON 或 .tres 序列化
- 分支变量记录玩家选择，影响游戏状态
- MVP 先做单轮对话 + 简单分支

### 7. 视觉特效 (VFX)

```
VFXManager
    ├── ParticleController      — GPU Particles 管理
    ├── HitEffect               — 命中闪光/火花/血花
    ├── SpellEffect             — 法术施放/弹道/爆炸粒子
    ├── PickupEffect            — 拾取光效
    └── ScreenEffect            — 受击屏幕闪红、抖动
```

- 所有特效通过 `VFXManager` 统一生命周期管理
- 使用 `GpuParticles3D` + 预制场景

### 8. 音效系统

```
AudioManager
    ├── AudioBusLayout          — BGM / SFX / Voice 分轨
    ├── SpatialAudioController  — 3D 空间音效
    ├── BGMController           — 战斗/探索/菜单 BGM 淡入淡出切换
    ├── SFXController           — 脚步、命中、技能、UI 音效
    └── AudioData (Resource)    — 音效配置（音量/音高随机范围/3D 衰减）
```

- AudioBus 分三轨：BGM、SFX、Voice
- 3D 空间音效用于敌人脚步声、技能落点等
- BGM 根据场景状态（探索→战斗）自动切换

### 9. 存档系统

- ConfigFile 格式
- 检查点制（场景中放置 CheckPoint 区域）
- 存储内容：位置/朝向/HP/MP/金币/物品/等级/属性/技能
- 死亡惩罚：损失 10% 金币

---

## 🗺️ 开发路线

### Phase 1 — 战斗原型（MVP）

> **目标：实现"战斗→死亡→重生→击败敌人"的完整闭环**

1. **第一人称移动 + 相机** — WASD + 鼠标视角 + 全套相机效果
2. **近战战斗系统** — 体力管理 + 武器前摇/后摇/硬直 + 锁定系统
3. **敌人 AI（一个敌人）** — 巡逻→发现→追踪→攻击 + 受击反馈
4. **伤害系统** — 玩家打敌人 + 敌人打玩家 + HP/体力 HUD
5. **死亡惩罚 + 检查点** — 扣 10% 金币 + 重生至检查点
6. **基础 VFX** — 命中闪光、受击屏幕效果
7. **基础音效** — 脚步声、攻击音效、受击音效
8. **测试关卡** — 1-2 个房间 + 检查点 + 1 个敌人

#### MVP 明确排除

| 排除项 | 加入阶段 |
|--------|---------|
| 分支对话 / NPC | Phase 2 |
| 法术/技能 | Phase 2 |
| 背包 UI | Phase 2 |
| 完整迷宫 | Phase 2 |
| 主菜单 / 设置 | Phase 2 |
| BOSS 战 | Phase 3 |
| 多职业 | Phase 3 |

### Phase 2 — 核心系统扩展

1. 对话系统 + NPC + HUD
2. 背包系统（有限格子 + 使用/装备/丢弃）
3. 法术/技能系统（≥2 种法术 + 冷却管理）
4. 角色成长（经验值 + 升级加点）
5. VFX 完整化（法术粒子、拾取光效）
6. 音效架构完整化（AudioBus 分轨、BGM 切换）
7. 主菜单 + 设置界面

### Phase 3 — 内容扩展

1. 第一个完整迷宫关卡 + Boss 战
2. 多职业/流派初版（近战/法师）
3. 多敌人类型
4. 装备系统（武器/防具）

### Phase 4 — 内容填充

1. 多个迷宫 + Boss
2. 完整剧情流程
3. 更多敌人/物品/法术

### Phase 5 — 打磨发布

1. 自制模型替换免费资产
2. 音效/画面优化
3. 性能调优
4. 打包发布

---

## 📝 技术决策

| 决策 | 结论 |
|------|------|
| 锁定系统 | ✅ 需要锁定系统 |
| 背包系统 | ✅ 完整背包 UI |
| 技能系统 | ✅ 完整法术/技能系统 |
| 角色成长 | ✅ 经验值升级 + 加点 |
| 动画方案 | ✅ C# 字符串驱动 |
| 手柄支持 | ⌨️ 仅键鼠，后期再加 |
| 相机效果 | ✅ 全套（FOV/晃动/抖动） |
| 资产策略 | ✅ 免费资产先做原型 |
| VFX 特效 | ✅ 完整 VFX 系统 |
| 音效架构 | ✅ 完整音效架构 |
| 对话系统 | ✅ 自写 C# Resource 驱动 |
