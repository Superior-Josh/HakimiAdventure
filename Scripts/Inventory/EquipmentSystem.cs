using Godot;
using System.Collections.Generic;

namespace HakimiAdventure.Inventory;

/// <summary>
/// 装备系统 — 武器/防具槽位，装备加成属性。
/// </summary>
[GlobalClass]
public partial class EquipmentSystem : Node
{
    [Export] public int WeaponSlot  { get; set; } // 背包格子索引
    [Export] public int ArmorSlot   { get; set; } = -1;
    [Export] public int AccessorySlot { get; set; } = -1;

    public ItemData? EquippedWeapon => GetSlotItem(WeaponSlot);
    public ItemData? EquippedArmor  => GetSlotItem(ArmorSlot);

    // 总加成
    public float BonusHP      => SumBonus(d => d.BonusHP);
    public float BonusMP      => SumBonus(d => d.BonusMP);
    public float BonusStr     => SumBonus(d => d.BonusStr);
    public float BonusAgi     => SumBonus(d => d.BonusAgi);
    public float BonusAttack  => SumBonus(d => d.BonusAttack);
    public float BonusDefense => SumBonus(d => d.BonusDefense);

    private InventorySystem? _inv;

    public override void _Ready()
    {
        _inv = GetParent<Player.PlayerController>().GetNodeOrNull<InventorySystem>("InventorySystem");
    }

    private ItemData? GetSlotItem(int slot)
    {
        if (slot < 0 || _inv == null) return null;
        return _inv.Slots[slot].IsEmpty ? null : _inv.Slots[slot].Data;
    }

    private float SumBonus(System.Func<ItemData, float> selector)
    {
        float total = 0;
        if (EquippedWeapon != null) total += selector(EquippedWeapon);
        if (EquippedArmor != null)  total += selector(EquippedArmor);
        return total;
    }

    /// <summary> 装备物品（移到指定槽位） </summary>
    public void Equip(int inventorySlot, int equipSlot)
    {
        if (_inv == null || inventorySlot < 0) return;
        var item = _inv.Slots[inventorySlot].Data;
        if (item == null) return;
        if (item.Type == ItemType.Weapon) WeaponSlot = inventorySlot;
        else if (item.Type == ItemType.Armor) ArmorSlot = inventorySlot;
    }

    /// <summary> 卸下装备 </summary>
    public void Unequip(int equipSlot)
    {
        if (equipSlot == 0) WeaponSlot = -1;
        else if (equipSlot == 1) ArmorSlot = -1;
    }
}
