<div align="center">

# ⚔️ 哈基米大冒险 · Hakimi Adventure

**类国王密令 · 第一人称即时动作 RPG**

[![Godot](https://img.shields.io/badge/Godot-4.7%20Mono-478CBF?logo=godot-engine&logoColor=white)](https://godotengine.org)
[![C#](https://img.shields.io/badge/C%23-.NET%208.0-239120?logo=csharp&logoColor=white)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

</div>

---

## 📖 概述

《哈基米大冒险》是一款受 **《国王密令》(King's Field)** 启发的第一人称即时动作 RPG。玩家将在 3D 迷宫中探索、战斗、成长，体验体力制即时战斗、分支对话和角色养成系统。

> **项目状态:** 核心系统完整，可玩原型 ✅  
> **预估工时:** ~175 小时（单人开发）  
> **引擎:** Godot 4.7 Mono · 全 C# 实现

---

## 🎮 游戏特性

| 特性 | 说明 |
|------|------|
| 🏃 **第一人称移动** | WASD + 鼠标环顾，冲刺/head bob/FOV 动态调节 |
| ⚔️ **即时体力制战斗** | 非回合制，体力管理攻防节奏，锁定+连击 |
| 👹 **多种敌人** | 哥布林/骷髅兵/弓箭手 + 3 个 BOSS（石像守卫/吸血鬼/冰巨人） |
| 🗺️ **迷宫探索** | 3 个程序化迷宫（普通/墓穴/冰洞），岔路+隐藏区域 |
| 💀 **死亡惩罚** | 扣 10% 金币 → 重生至检查点 |
| 🎒 **背包系统** | 有限格子，拾取/使用/装备/丢弃，装备加成 |
| 🔮 **法术系统** | 火球术/回复术/闪电术，冷却+MP 管理 |
| 📈 **角色成长** | 经验值升级 → 属性加点（力量/敏捷） |
| 🧙 **分支对话** | NPC 交互，选项影响游戏变量 |
| 🔊 **音效系统** | 三轨 AudioBus（BGM/SFX/Voice），BGM 自动切换 |
| 💾 **存档系统** | 检查点自动存档，ConfigFile 序列化 |
| 🎨 **纯 C# 动画** | 字符串驱动 AnimationPlayer，无需 AnimationTree |

---

## 🏗️ 系统架构

```
Scripts/
├── Core/         GameManager · SaveManager · CheckPoint · NPC · MazeGenerator · LoadingScreen
├── Player/       PlayerController · CameraController
├── Combat/       StaminaSystem · WeaponController · LockOnSystem · DamageSystem
├── Enemy/        EnemyAI · BossAI · RangedEnemyAI · FastEnemyAI · VampireBossAI · IceGolemBossAI
├── Magic/        SpellData · MagicSystem
├── Inventory/    ItemData · InventorySystem · InventoryUI · PickupItem · EquipmentSystem
├── Growth/       ExperienceManager · AttributeSystem · LevelUpController
├── Dialogue/     DialogueResource · DialogueController · DialogueUI
├── Audio/        AudioManager · SfxGenerator
├── VFX/          VFXManager
└── UI/           HUD · MainMenu
```

---

## 🚀 快速开始

### 前置要求

| 工具 | 版本 |
|------|------|
| Godot Engine | **4.7 Mono** ([下载](https://godotengine.org/download/)) |
| .NET SDK | **8.0** ([下载](https://dotnet.microsoft.com/download)) |

### 运行

```bash
git clone https://github.com/Superior-Josh/HakimiAdventure.git
cd HakimiAdventure
# 用 Godot 4.7 Mono 打开 project.godot，按 F5 运行
```

### 导出

```
Godot → Project → Export → 选择目标平台 → Export Project
```

支持平台：Windows (.exe) · Linux (x86_64) · macOS (.dmg)

---

## 🗺️ 开发路线

| Sprint | 名称 | 工时 | 🏁 |
|--------|------|------|-----|
| 0 | 项目骨架 & 移动相机 | 12h | ✅ |
| 1 | 战斗核心闭环 | 20h | ✅ |
| 2 | 死亡循环 & 反馈 | 16h | ✅ MVP |
| 3 | 对话/NPC/菜单 | 16h | ✅ |
| 4 | 背包/法术/成长 | 26h | ✅ |
| 5 | 关卡 & BOSS | 30h | ✅ |
| 6 | 内容填充 & 发布 | 55h | ✅ |
| **总计** | | **~175h** | **🎯 完成** |

---

## 🎮 操作说明

| 按键 | 动作 |
|------|------|
| W/A/S/D | 移动 |
| 鼠标 | 视角 |
| Shift | 冲刺 |
| 左键 | 攻击 |
| Q | 锁定/切换敌人 |
| F | 交互（对话/拾取） |
| I | 打开背包 |
| 1 / 2 | 释放法术 |
| ESC | 释放/捕获鼠标 |

---

## 📊 项目规模

```
C# 脚本: 42 个 (~3927 行)
.tres 数据: 10 个 (物品/法术/对话)
.tscn 场景: 5 个 (主菜单/迷宫/测试)
配置文件: 6 个
迷宫数量: 3 (普通/墓穴/冰洞)
BOSS 数量: 3 (石像守卫/吸血鬼/冰巨人)
敌人类型: 4 (哥布林/骷髅兵/弓箭手)
法术数量: 3 (火球/回复/闪电)
```

---

## 📝 技术栈

| 层 | 技术 |
|----|------|
| **引擎** | Godot 4.7 Mono |
| **语言** | C# (.NET 8.0) |
| **动画** | C# 字符串驱动 (`AnimationPlayer.Play()`) |
| **存档** | ConfigFile (检查点制) |
| **对话** | 自写 C# Resource 驱动 |
| **音效** | 程序化生成（可替换为真实音频） |
| **3D 模型** | 占位 BoxMesh（待替换为 Blender 自制模型） |

---

## 📄 许可

本项目基于 MIT 许可证开源。详见 [LICENSE](LICENSE) 文件。

---

<div align="center">

**Made with ❤️ by [Superior-Josh](https://github.com/Superior-Josh)**

</div>
