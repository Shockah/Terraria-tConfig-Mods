public const int
	MODIFIER_IRONSKIN = 30,
	MODIFIER_CURSED = 10,
	MODIFIER_WEAK = 7;

public readonly static string[] checkBuffs = new string[]{"Obsidian Skin","Regeneration","Swiftness","Ironskin","Magic Power","Mana Regeneration","Featherfall","Water Walking","Gravitation"};
public static Dictionary<string,int>[] oldHasBuff;
public static int[]
	countIronskin,
	countMagicPower;

public void Initialize() {
	oldHasBuff = new Dictionary<string,int>[Main.player.Length];
	for (int i = 0; i < oldHasBuff.Length; i++) {
		oldHasBuff[i] = new Dictionary<string,int>();
		foreach (string s in checkBuffs) oldHasBuff[i][s] = -1;
	}
	
	countIronskin = new int[Main.player.Length];
	countMagicPower = new int[Main.player.Length];
}

public void UpdatePlayer(Player player) {
	Dictionary<string,int> hasBuff = new Dictionary<string,int>();
	foreach (string s in checkBuffs) hasBuff[s] = -1;
	
	bool
		CONST_POSITIVE = Settings.GetBool("positive"),
		CONST_NEGATIVE = Settings.GetBool("negative"),
		CONST_LONGER_BUFF = Settings.GetBool("extendBuff");
	
	for (int i = 0; i < player.buffType.Length; i++) {
		if (player.buffType[i] <= 0 || player.buffTime[i] <= 0) continue;
		string bname = Main.buffName[player.buffType[i]];
		
		if (hasBuff.ContainsKey(bname)) hasBuff[bname] = i;
		
		if (CONST_POSITIVE) {
			switch (bname) {
				case "Obsidian Skin": if (player.lavaWet) player.buffTime[i]--; break;
				case "Regeneration": if (player.statLife < player.statLifeMax2) player.buffTime[i]--; break;
				case "Swiftness": if (Math.Abs(player.velocity.X) >= 1) player.buffTime[i]--; break;
				case "Ironskin": case "Thorns": {int diff = (int)((countIronskin[player.whoAmi]-player.statLife)/1.5f); if (diff > 0) player.buffTime[i] -= diff; if (player.buffTime[i] < 0) player.buffTime[i] = 0;} break;
				case "Magic Power": case "Clairvoyance": {int diff = (countMagicPower[player.whoAmi]-player.statMana)*2; if (diff > 0) player.buffTime[i] -= diff; if (player.buffTime[i] < 0) player.buffTime[i] = 0;} break;
				case "Mana Regeneration": if (player.statMana < player.statManaMax2) player.buffTime[i]--; break;
				case "Featherfall": if (player.velocity.Y > 0) player.buffTime[i]--; break;
				case "Water Walking": {Tile tile = Main.tile[(int)Math.Round(player.Center.X/16f),(int)Math.Round((player.position.Y+player.height)/16f)]; if (!tile.active && tile.liquid >= 16 && !tile.lava) player.buffTime[i]--;} break;
				case "Gravitation": if (player.gravDir != 1) player.buffTime[i]--; break;
			}
		}
		if (CONST_NEGATIVE) {
			bool inTown = false;
			foreach (NPC npc in Main.npc) {
				if (!npc.active && npc.life <= 0 && !npc.townNPC) continue;
				if (Math.Sqrt(Math.Pow(player.position.X-npc.position.X,2)+Math.Pow(player.position.Y-npc.position.Y,2)) < 1500) {
					inTown = true;
					break;
				}
			}
			
			switch (bname) {
				case "Darkness": if (Lighting.Brightness((int)Math.Round(player.Center.X/16f),(int)Math.Round(player.Center.Y/16f)) >= 1f) player.buffTime[i]--; break;
				case "Cursed": case "Silenced": {if (player.controlUseItem && player.releaseUseItem) player.buffTime[i] -= MODIFIER_CURSED; if (player.buffTime[i] < 0) player.buffTime[i] = 0;} break;
				case "On Fire!": case "Slow": case "Cursed Inferno": if (Math.Abs(player.velocity.Y) >= 1) player.buffTime[i]--; break;
				case "Tipsy": if (Math.Abs(player.velocity.X) >= 1 || Math.Abs(player.velocity.Y) >= 1) player.buffTime[i]--; break;
				case "Poisoned": case "Bleeding": case "Confused": if (Math.Abs(player.velocity.X) < 1 || Math.Abs(player.velocity.Y) < 1) player.buffTime[i]--; break;
				case "Weak": case "Broken Armor": {if (inTown) player.buffTime[i] -= MODIFIER_WEAK; if (player.buffTime[i] < 0) player.buffTime[i] = 0;} break;
			}
		}
	}
	
	foreach (string s in checkBuffs) {
		int hb = hasBuff[s];
		if (CONST_LONGER_BUFF && hb != -1 && player.buffTime[hb] > oldHasBuff[player.whoAmi][s]) player.buffTime[hb] *= 2;
		oldHasBuff[player.whoAmi][s] = hb == -1 ? -1 : player.buffTime[hb];
	}
	
	countIronskin[player.whoAmi] = player.statLife;
	countMagicPower[player.whoAmi] = player.statMana;
}