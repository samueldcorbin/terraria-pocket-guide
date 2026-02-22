using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PocketGuide;

public class InactivePocketGuideItem : ModItem
{
	public override string Texture => $"Terraria/Images/Item_{ItemID.DontHurtComboBookInactive}";

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.DontHurtComboBookInactive);
		Item.rare = ItemRarityID.Orange;
	}

	public override bool CanRightClick() => Item.stack == 1;

	public override bool ConsumeItem(Player player) => false;

	public override void RightClick(Player player)
	{
		Item.ChangeItemType(ModContent.ItemType<PocketGuideItem>());
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		string fullTooltip = Language.GetTextValue("ItemTooltip.DontHurtCrittersBookInactive");
		int lastNewline = fullTooltip.LastIndexOf('\n');
		string toggleText = lastNewline >= 0 ? fullTooltip[(lastNewline + 1)..] : fullTooltip;
		toggleText = Lang.SupportGlyphs(toggleText);

		var line = new TooltipLine(Mod, "PocketGuideToggle", toggleText);
		int idx = tooltips.FindIndex(t => t.Name == "JourneyResearch");
		if (idx >= 0)
			tooltips.Insert(idx, line);
		else
			tooltips.Add(line);
	}
}
