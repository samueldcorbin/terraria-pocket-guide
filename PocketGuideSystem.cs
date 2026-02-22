using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PocketGuide;

public class PocketGuideSystem : ModSystem
{
	public static List<Recipe> HoverRecipes { get; private set; } = new();

	private static int _lastHoverItemType;

	public override void PostDrawInterface(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
	{
		var mp = Main.LocalPlayer.GetModPlayer<PocketGuidePlayer>();
		int hoverType = Main.HoverItem.type;

		if (!mp.DollActive || !mp.DollPresent || Main.HoverItem.IsAir)
		{
			HoverRecipes.Clear();
			_lastHoverItemType = 0;
			return;
		}

		if (hoverType == _lastHoverItemType)
			return;

		_lastHoverItemType = hoverType;
		CollectRecipesUsing(hoverType);
	}

	private static void CollectRecipesUsing(int itemType)
	{
		HoverRecipes.Clear();

		for (int i = 0; i < Recipe.maxRecipes; i++)
		{
			Recipe recipe = Main.recipe[i];
			if (recipe.createItem.type == ItemID.None)
				break;
			if (recipe.Disabled)
				continue;

			for (int j = 0; j < recipe.requiredItem.Count; j++)
			{
				Item req = recipe.requiredItem[j];
				if (req.type == ItemID.None)
					break;

				if (req.type == itemType || recipe.AcceptedByItemGroups(itemType, req.type))
				{
					HoverRecipes.Add(recipe);
					break;
				}
			}
		}
	}
}
