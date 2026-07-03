using Godot;
using System.Collections.Generic;

namespace HakimiAdventure.Inventory;

public struct ItemSlot
{
    public ItemData? Data;
    public int       Count;
    public bool      IsEmpty => Data == null || Count <= 0;
}

/// <summary>
/// 背包系统 — 有限格子，支持拾取/使用/装备/丢弃。
/// </summary>
[GlobalClass]
public partial class InventorySystem : Node
{
    [Export] public int SlotCount { get; set; } = 20;

    public ItemSlot[] Slots { get; private set; } = null!;

    // ── 事件 ──
    [Signal] public delegate void InventoryChangedEventHandler();

    public override void _Ready()
    {
        Slots = new ItemSlot[SlotCount];
    }

    /// <summary> 尝试拾取物品，返回实际拾取数量 </summary>
    public int PickupItem(ItemData data, int count = 1)
    {
        if (data.MaxStack > 1)
        {
            // 先找同ID堆叠
            foreach (var i in EnumerateSlots())
            {
                if (Slots[i].Data?.ID == data.ID && Slots[i].Count < data.MaxStack)
                {
                    var space = data.MaxStack - Slots[i].Count;
                    var add = Mathf.Min(space, count);
                    Slots[i] = new ItemSlot { Data = data, Count = Slots[i].Count + add };
                    count -= add;
                    if (count <= 0) { EmitChanged(); return add; }
                }
            }
        }

        // 找空格
        for (var i = 0; i < SlotCount; i++)
        {
            if (Slots[i].IsEmpty)
            {
                var add = Mathf.Min(count, data.MaxStack);
                Slots[i] = new ItemSlot { Data = data, Count = add };
                count -= add;
                if (count <= 0) { EmitChanged(); return add; }
            }
        }

        EmitChanged();
        return count; // 剩余未拾取的
    }

    /// <summary> 使用物品（消耗品） </summary>
    public void UseItem(int slotIndex, Player.PlayerController player)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return;
        var slot = Slots[slotIndex];
        if (slot.IsEmpty) return;

        if (slot.Data!.Type == ItemType.Consumable)
        {
            player.CurrentHP = Mathf.Min(player.CurrentHP + slot.Data.HealHP, player.MaxHP);
            player.MP = Mathf.Min(player.MP + slot.Data.HealMP, player.MaxMP);
            RemoveItem(slotIndex, 1);
        }
        // Weapon/Armor: equip handled in inventory UI
    }

    /// <summary> 丢弃物品 </summary>
    public void DiscardItem(int slotIndex, int count = 1)
    {
        RemoveItem(slotIndex, count);
    }

    /// <summary> 移除指定数量 </summary>
    public void RemoveItem(int slotIndex, int count = 1)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return;
        var slot = Slots[slotIndex];
        if (slot.IsEmpty) return;

        slot.Count -= count;
        if (slot.Count <= 0)
            Slots[slotIndex] = new ItemSlot { Data = null, Count = 0 };
        else
            Slots[slotIndex] = slot;

        EmitChanged();
    }

    public void SetSlot(int index, ItemData? data, int count = 1)
    {
        if (index < 0 || index >= SlotCount) return;
        Slots[index] = new ItemSlot { Data = data, Count = count };
        EmitChanged();
    }

    /// <summary> 清空背包 </summary>
    public void Clear()
    {
        for (var i = 0; i < SlotCount; i++)
            Slots[i] = new ItemSlot { Data = null, Count = 0 };
        EmitChanged();
    }

    public void EmitChanged() => EmitSignal(SignalName.InventoryChanged);

    private System.Collections.Generic.IEnumerable<int> EnumerateSlots()
    {
        for (var i = 0; i < SlotCount; i++) yield return i;
    }
}
