using System;
using System.Reflection;
using Mono.Cecil;
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
		IL_Main.DrawInventory += PatchSkipMaterialSlot;

		MonoModHooks.Modify(
			typeof(ItemSlot).GetMethod("OverrideHover",
				BindingFlags.Public | BindingFlags.Static, null,
				new[] { typeof(Item[]), typeof(int), typeof(int) }, null),
			PatchOverrideHover);

		MonoModHooks.Modify(
			typeof(ItemSlot).GetMethod("OverrideLeftClick",
				BindingFlags.NonPublic | BindingFlags.Static),
			PatchOverrideLeftClick);

		On_Player.dropItemCheck += HookDropItemCheck;
	}

	public override void PostUpdateInput()
	{
		if (Main.gameMenu || Main.netMode == NetmodeID.Server)
			return;

		var player = Main.LocalPlayer;
		bool active = player.GetModPlayer<PocketGuidePlayer>().DollPresent;
		bool guideAlive = NPC.AnyNPCs(NPCID.Guide);

		bool pocketGuideActive = active && guideAlive && Main.playerInventory
			&& player.chest == -1 && Main.npcShop == 0 && !Main.InReforgeMenu;

		if (pocketGuideActive)
		{
			Item source = !Main.mouseItem.IsAir ? Main.mouseItem
				: !Main.HoverItem.IsAir ? Main.HoverItem
				: new Item();

			if (source.type != Main.guideItem.type)
			{
				Main.guideItem = source.Clone();
				Recipe.FindRecipes();
			}

			ForceGuideUI = !Main.guideItem.IsAir && Main.numAvailableRecipes > 0;
			Main.InGuideCraftMenu = ForceGuideUI;
		}
		else
		{
			ForceGuideUI = false;
			if (_wasForceGuideUI)
			{
				Main.InGuideCraftMenu = false;
				Main.guideItem = new Item();
				Recipe.FindRecipes();
			}
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
		// Find talkNPC property getter in the close guard condition
		c.GotoNext(MoveType.After, i =>
			i.OpCode == OpCodes.Callvirt
			&& i.Operand is MethodReference mr
			&& mr.Name == "get_talkNPC");
		// Replace talkNPC value on stack
		c.EmitDelegate<Func<int, int>>(talkNPC => ForceGuideUI ? 0 : talkNPC);
	}

	// Skip the material slot block (hit-test, interactions, Draw) after DrawGuideCraftText
	// when ForceGuideUI is active. This prevents the "Place a material here" slot from
	// appearing and avoids HoverItem feedback loops.
	private static void PatchSkipMaterialSlot(ILContext il)
	{
		var c = new ILCursor(il);

		// Find the DrawGuideCraftText call
		c.GotoNext(MoveType.After, i =>
			i.OpCode == OpCodes.Call
			&& i.Operand is MethodReference mr
			&& mr.Name == "DrawGuideCraftText");

		// Find the ItemSlot.Draw call for guideItem (context 7) that ends the block.
		// This is the first ItemSlot.Draw after DrawGuideCraftText.
		var end = c.Clone();
		end.GotoNext(MoveType.After, i =>
			i.OpCode == OpCodes.Call
			&& i.Operand is MethodReference mr
			&& mr.Name == "Draw"
			&& mr.DeclaringType.Name == "ItemSlot");

		// Mark the instruction after ItemSlot.Draw as our branch target
		var skipTarget = end.DefineLabel();
		end.MarkLabel(skipTarget);

		// Emit: if (ForceGuideUI) goto skipTarget
		c.EmitDelegate<Func<bool>>(() => ForceGuideUI);
		c.Emit(OpCodes.Brtrue, skipTarget);
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
}
