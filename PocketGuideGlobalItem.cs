using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PocketGuide;

public class PocketGuideGlobalItem : GlobalItem
{
	public override bool AppliesToEntity(Item entity, bool lateInstantiation)
	{
		return entity.type == ItemID.GuideVoodooDoll;
	}

	public override bool CanRightClick(Item item)
	{
		return true;
	}

	public override bool ConsumeItem(Item item, Player player)
	{
		return false;
	}

	public override void RightClick(Item item, Player player)
	{
		var mp = player.GetModPlayer<PocketGuidePlayer>();
		mp.DollActive = !mp.DollActive;
	}

	public override void UpdateInventory(Item item, Player player)
	{
		player.GetModPlayer<PocketGuidePlayer>().DollPresent = true;
	}

	public override void UpdateAccessory(Item item, Player player, bool hideVisual)
	{
		player.GetModPlayer<PocketGuidePlayer>().DollPresent = true;
	}

	public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		var mp = Main.LocalPlayer.GetModPlayer<PocketGuidePlayer>();
		if (!mp.DollActive)
		{
			drawColor *= 0.5f;
			spriteBatch.Draw(
				Terraria.GameContent.TextureAssets.Item[item.type].Value,
				position,
				frame,
				drawColor,
				0f,
				origin,
				scale,
				SpriteEffects.None,
				0f);
			return false;
		}
		return true;
	}
}
