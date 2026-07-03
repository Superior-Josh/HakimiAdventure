using Godot;

namespace HakimiAdventure.Inventory;

public enum ItemType { Consumable, Weapon, Armor, Key, Material }

/// <summary> 物品数据配置 </summary>
[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string   ID           { get; set; } = "";
    [Export] public string   Name         { get; set; } = "物品";
    [Export] public string   Description  { get; set; } = "";
    [Export] public ItemType Type         { get; set; }
    [Export] public int      MaxStack     { get; set; } = 1;
    [Export] public int      BuyPrice     { get; set; } = 10;
    [Export] public int      SellPrice    { get; set; } = 5;

    // 消耗品效果
    [Export] public float    HealHP       { get; set; }
    [Export] public float    HealMP       { get; set; }

    // 装备加成
    [Export] public float    BonusHP      { get; set; }
    [Export] public float    BonusMP      { get; set; }
    [Export] public float    BonusStr     { get; set; }
    [Export] public float    BonusAgi     { get; set; }
    [Export] public float    BonusAttack  { get; set; }
    [Export] public float    BonusDefense { get; set; }
}
