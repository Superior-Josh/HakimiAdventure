# 哈基米大冒险

> **3D 第一人称回合制 RPG** — 灵感来自《国王密令》(King's Field)

## 🎮 游戏简介

一款 3D 第一人称迷宫探索 RPG，玩家在精心设计的线性地图中探索、战斗、对话，体验多职业成长与故事分支。

**核心玩法：**
- 🏰 第一人称 3D 迷宫探索
- ⚔️ 回合制战斗，多职业/流派（近战/远程）
- 🗺️ 精心设计的线性地图（非随机生成），含检查点与 NPC
- 💬 分支对话系统，选择影响流程走向
- ☠️ 死亡惩罚机制

## 🛠️ 技术栈

| 技术 | 说明 |
|------|------|
| **引擎** | Godot 4.7 (Mono / C#) |
| **语言** | C# (.NET 8.0) |
| **3D 建模** | Blender |
| **2D 美术** | Krita / GIMP |
| **音效** | Audacity + sfxr |

## 🎯 当前进度

- [x] 第一人称物理引擎（Minecraft 风格）
- [x] 重力 / 加速 / 摩擦 / 动量
- [x] 跳跃（带缓冲）
- [x] 冲刺 / 潜行
- [x] 鼠标自由视角
- [ ] 回合制战斗系统
- [ ] 对话系统
- [ ] 职业 / 装备系统
- [ ] 迷宫关卡搭建

## 🎮 操作手册

| 按键 | 行为 |
|------|------|
| **鼠标** | 自由视角（左键捕获 / ESC 释放） |
| **W / A / S / D** | 前后左右移动 |
| **Shift(按住)** | 冲刺 |
| **Ctrl(按住)** | 潜行（降低身高，头顶有障碍自动保持） |
| **Space** | 跳跃（带 3 帧缓冲，落地前预按有效） |
| **F** | 交互（预留） |

### 物理参数

| 参数 | 值 | 说明 |
|------|----|------|
| 行走速度 | 4.317 | Minecraft 步行基准 |
| 冲刺速度 | 5.612 | Minecraft 疾跑基准 |
| 潜行速度 | 1.295 | Minecraft 潜行基准 |
| 重力 | 25 m/s² | 跳跃干脆利落 |
| 跳跃速度 | 7.9 m/s | ≈ 1.25 格高 |
| 地面加速 | 50 | 一按就满速 |
| 地面摩擦 | 50 | 一松就停 |
| 空中操控 | 3% | 基本飘定 |

## 📁 项目结构

```
├── project.godot              # Godot 项目配置
├── HakimiAdventure.csproj     # C# 项目文件
│
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs     # 全局单例管理器
│   │   └── SaveManager.cs     # 检查点存档 + 死亡惩罚
│   ├── Player/
│   │   └── PlayerController.cs # 第一人称物理控制器
│   ├── Combat/
│   │   ├── TurnManager.cs     # 回合制战斗
│   │   └── CombatStats.cs     # 战斗数值计算
│   ├── Dialogue/
│   │   └── DialogueManager.cs # 对话系统
│   └── UI/
│       └── UIManager.cs       # HUD + 战斗菜单
│
├── Resources/
│   ├── Jobs/JobData.cs        # 职业数据配置（编辑器填表）
│   └── Enemies/EnemyData.cs   # 敌人数据配置
│
├── Scenes/
│   ├── Main.tscn              # 主场景（游戏入口）
│   └── Player.tscn            # 玩家预制体
│
└── Assets/                    # 美术 / 音频资源（待填充）
    ├── Textures/
    ├── Models/
    └── Audio/
```

## 🚀 开发指南

### 前置条件

- Godot 4.7 Mono（C# 版本）
- .NET SDK 8.0

### 启动项目

```bash
# 双击 project.godot 或通过命令行
cd 项目目录
godot .
```

按 **F5** 运行游戏。

### 构建

```bash
dotnet build HakimiAdventure.csproj
```

## 🧠 AI 辅助开发

本项目全程使用 AI Agent 辅助编码。C# 的强类型系统配合编译器能有效拦截 AI 幻觉导致的低级错误——编译通过后再运行，大幅减少运行时崩溃。
