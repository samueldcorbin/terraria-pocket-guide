using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PocketGuide;

public class PocketGuideGlobalItem : GlobalItem
{
	public override bool AppliesToEntity(Item entity, bool lateInstantiation)
	{
		return entity.type == ItemID.GuideVoodooDoll || entity.ModItem is InactiveGuideVoodooDoll;
	}

	public override bool CanRightClick(Item item)
	{
		return item.stack == 1;
	}

	public override bool ConsumeItem(Item item, Player player)
	{
		return false;
	}

	public override void RightClick(Item item, Player player)
	{
		if (item.type == ItemID.GuideVoodooDoll)
			item.ChangeItemType(ModContent.ItemType<InactiveGuideVoodooDoll>());
		else
			item.ChangeItemType(ItemID.GuideVoodooDoll);
	}

	public override void UpdateInventory(Item item, Player player)
	{
		if (item.type == ItemID.GuideVoodooDoll)
			player.GetModPlayer<PocketGuidePlayer>().DollPresent = true;
	}

	public override void UpdateAccessory(Item item, Player player, bool hideVisual)
	{
		if (item.type == ItemID.GuideVoodooDoll)
			player.GetModPlayer<PocketGuidePlayer>().DollPresent = true;
	}

	public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
	{
		bool equippedIsDoll = equippedItem.type == ItemID.GuideVoodooDoll || equippedItem.ModItem is InactiveGuideVoodooDoll;
		bool incomingIsDoll = incomingItem.type == ItemID.GuideVoodooDoll || incomingItem.ModItem is InactiveGuideVoodooDoll;
		if (equippedIsDoll && incomingIsDoll)
			return false;
		return true;
	}
}
