using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace PocketGuide;

public class PocketGuideSystem : ModSystem
{
	public static List<Recipe> HoverRecipes { get; private set; } = new();

	private static int _lastHoverItemType;

	public override void PostDrawInterface(SpriteBatch spriteBatch)
	{
		var mp = Main.LocalPlayer.GetModPlayer<PocketGuidePlayer>();
		int hoverType = Main.HoverItem.type;

		if (!mp.DollActive || !mp.DollPresent || Main.HoverItem.IsAir)
		{
			HoverRecipes.Clear();
			_lastHoverItemType = 0;
			return;
		}

		if (hoverType != _lastHoverItemType)
		{
			_lastHoverItemType = hoverType;
			CollectRecipesUsing(hoverType);
		}

		DrawRecipeList(spriteBatch);
	}

	private static void DrawRecipeList(SpriteBatch spriteBatch)
	{
		if (HoverRecipes.Count == 0)
			return;

		var font = FontAssets.MouseText.Value;
		float x = 20f;
		float y = 80f;
		float lineHeight = font.MeasureString("A").Y + 4f;
		var sb = new StringBuilder();

		for (int i = 0; i < HoverRecipes.Count; i++)
		{
			Recipe recipe = HoverRecipes[i];
			sb.Clear();

			// "Result (qty) = Ingredient1 (qty) + Ingredient2 (qty) @ Station"
			sb.Append(recipe.createItem.Name);
			if (recipe.createItem.stack > 1)
				sb.Append(" (").Append(recipe.createItem.stack).Append(')');

			sb.Append(" = ");

			for (int j = 0; j < recipe.requiredItem.Count; j++)
			{
				if (j > 0)
					sb.Append(" + ");
				Item req = recipe.requiredItem[j];
				sb.Append(req.Name);
				if (req.stack > 1)
					sb.Append(" (").Append(req.stack).Append(')');
			}

			if (recipe.requiredTile.Count > 0)
			{
				sb.Append(" @ ");
				for (int t = 0; t < recipe.requiredTile.Count; t++)
				{
					if (t > 0)
						sb.Append(", ");
					int tileId = recipe.requiredTile[t];
					sb.Append(Lang.GetMapObjectName(MapHelper.TileToLookup(tileId, 0)));
				}
			}

			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, sb.ToString(),
				new Vector2(x, y), Color.White, 0f, Vector2.Zero, Vector2.One);
			y += lineHeight;
		}
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
