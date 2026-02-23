using System;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace PocketGuide;

public class PocketGuideSystem : ModSystem
{
	public static bool ForceGuideUI { get; private set; }
	private static bool _wasForceGuideUI;

	public override void Load()
	{
		IL_Main.DrawInventory += PatchCloseGuard;

		MonoModHooks.Modify(
			typeof(ItemSlot).GetMethod("OverrideHover",
				BindingFlags.Public | BindingFlags.Static, null,
				new[] { typeof(Item[]), typeof(int), typeof(int) }, null),
			PatchOverrideHover);

		MonoModHooks.Modify(
			typeof(ItemSlot).GetMethod("OverrideLeftClick",
				BindingFlags.NonPublic | BindingFlags.Static),
			PatchOverrideLeftClick);

		MonoModHooks.Modify(
			typeof(ItemSlot).GetMethod("LeftClick",
				BindingFlags.Public | BindingFlags.Static, null,
				new[] { typeof(Item[]), typeof(int), typeof(int) }, null),
			PatchLeftClick);

		MonoModHooks.Modify(
			typeof(ItemSlot).GetMethod("RightClick",
				BindingFlags.Public | BindingFlags.Static, null,
				new[] { typeof(Item[]), typeof(int), typeof(int) }, null),
			PatchRightClick);

		On_Player.dropItemCheck += HookDropItemCheck;
	}

	public override void PostUpdateInput()
	{
		if (Main.gameMenu || Main.netMode == NetmodeID.Server)
			return;

		var player = Main.LocalPlayer;
		bool active = player.GetModPlayer<PocketGuidePlayer>().DollPresent;
		bool guideAlive = NPC.AnyNPCs(NPCID.Guide);

		ForceGuideUI = active && guideAlive && Main.playerInventory
			&& player.chest == -1 && Main.npcShop == 0 && !Main.InReforgeMenu;

		if (ForceGuideUI)
		{
			Main.InGuideCraftMenu = true;

			Item source = !Main.mouseItem.IsAir ? Main.mouseItem
				: !Main.HoverItem.IsAir ? Main.HoverItem
				: new Item();

			if (source.type != Main.guideItem.type)
			{
				Main.guideItem = source.Clone();
				Recipe.FindRecipes();
			}
		}
		else if (_wasForceGuideUI)
		{
			Main.guideItem = new Item();
			Recipe.FindRecipes();
		}

		_wasForceGuideUI = ForceGuideUI;
	}

	// Prevent dropItemCheck from "returning" virtual guideItem to inventory.
	private static void HookDropItemCheck(On_Player.orig_dropItemCheck orig, Player self)
	{
		if (ForceGuideUI)
			Main.guideItem = new Item();
		orig(self);
	}

	// Bypass talkNPC == -1 in the InGuideCraftMenu close guard.
	// Vanilla: if (chest != -1 || npcShop != 0 || talkNPC == -1 || InReforgeMenu)
	// Patch: replace talkNPC value with 0 when ForceGuideUI so the == -1 check fails.
	private static void PatchCloseGuard(ILContext il)
	{
		var c = new ILCursor(il);

		// Skip past first InGuideCraftMenu load (reforge close guard at ~line 54130)
		c.GotoNext(i => i.MatchLdsfld<Main>(nameof(Main.InGuideCraftMenu)));
		// Find second InGuideCraftMenu load (the else-if branch at ~line 54259)
		c.GotoNext(i => i.MatchLdsfld<Main>(nameof(Main.InGuideCraftMenu)));
		// Find talkNPC field load in the close guard condition
		c.GotoNext(MoveType.After, i => i.MatchLdfld<Player>("talkNPC"));
		// Replace talkNPC value on stack
		c.EmitDelegate<Func<int, int>>(talkNPC => ForceGuideUI ? 0 : talkNPC);
	}

	// Suppress cursorOverride = 9 on inventory materials when ForceGuideUI.
	// Two occurrences of: else if (context == 0 && Main.InGuideCraftMenu)
	private static void PatchOverrideHover(ILContext il)
	{
		var c = new ILCursor(il);
		for (int i = 0; i < 2; i++)
		{
			c.GotoNext(MoveType.After,
				instr => instr.MatchLdsfld<Main>(nameof(Main.InGuideCraftMenu)));
			c.EmitDelegate<Func<bool, bool>>(
				inGuide => inGuide && !ForceGuideUI);
		}
	}

	// Suppress shift-click swap into guideItem when ForceGuideUI.
	// Target: else if (Main.InGuideCraftMenu) { Utils.Swap(...) }
	private static void PatchOverrideLeftClick(ILContext il)
	{
		var c = new ILCursor(il);
		c.GotoNext(MoveType.After,
			instr => instr.MatchLdsfld<Main>(nameof(Main.InGuideCraftMenu)));
		c.EmitDelegate<Func<bool, bool>>(
			inGuide => inGuide && !ForceGuideUI);
	}

	// Prevent direct click on guide slot (context 7) when ForceGuideUI.
	private static void PatchLeftClick(ILContext il)
	{
		var c = new ILCursor(il);
		var skip = c.DefineLabel();
		c.Emit(OpCodes.Ldarg_1);
		c.Emit(OpCodes.Ldc_I4_7);
		c.Emit(OpCodes.Bne_Un, skip);
		c.EmitDelegate<Func<bool>>(() => ForceGuideUI);
		c.Emit(OpCodes.Brfalse, skip);
		c.Emit(OpCodes.Ret);
		c.MarkLabel(skip);
	}

	// Prevent right-click on guide slot (context 7) when ForceGuideUI.
	private static void PatchRightClick(ILContext il)
	{
		var c = new ILCursor(il);
		var skip = c.DefineLabel();
		c.Emit(OpCodes.Ldarg_1);
		c.Emit(OpCodes.Ldc_I4_7);
		c.Emit(OpCodes.Bne_Un, skip);
		c.EmitDelegate<Func<bool>>(() => ForceGuideUI);
		c.Emit(OpCodes.Brfalse, skip);
		c.Emit(OpCodes.Ret);
		c.MarkLabel(skip);
	}
}
