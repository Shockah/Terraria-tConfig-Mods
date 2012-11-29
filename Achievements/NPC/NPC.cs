public void DealtNPC(double damage, Player player) {
	if (player.whoAmi == Main.myPlayer) {
		ModPlayer.ExternalAchievementProgress("TERRARIA_TOTALDMG100K",(int)damage);
		ModPlayer.ExternalAchievementProgress("TERRARIA_TOTALDMG10M",(int)damage);
	}
	
	if (Main.netMode == ModWorld.MODE_CLIENT) {
		if (ModWorld.npcEntries[npc.whoAmI] == null || !Object.ReferenceEquals(ModWorld.npcEntries[npc.whoAmI].npc,npc)) ModWorld.npcEntries[npc.whoAmI] = new ModWorld.NPCEntry(npc);
		
		ModWorld.NPCEntry entry = ModWorld.npcEntries[npc.whoAmI];
		if (!entry.hitBy.Contains(player.whoAmi)) entry.hitBy.Add(player.whoAmi);
		
		if (npc.life > 0) return;
		if (entry.hitBy.Contains(Main.myPlayer)) {
			if (npc.name == "Pinky") for (int i = 0; i < entry.hitBy.Count; i++) if (entry.hitBy.Contains(Main.myPlayer)) ModPlayer.ExternalAchieve("TERRARIA_KILLPINKY");
			if (npc.name == "Guide") for (int i = 0; i < entry.hitBy.Count; i++) if (entry.hitBy.Contains(Main.myPlayer)) ModPlayer.ExternalAchieve("TERRARIA_KILLGUIDE");
			if (npc.name == "Wyvern" || npc.displayName == "Wyvern") for (int i = 0; i < entry.hitBy.Count; i++) if (entry.hitBy.Contains(Main.myPlayer) && !ModWorld.tookDamage[Main.myPlayer]) ModPlayer.ExternalAchieve("TERRARIA_HM_KILLWYVERNNODMG");
		}
		ModWorld.npcEntries[npc.whoAmI] = null;
	} else if (Main.netMode == ModWorld.MODE_SOLO) {
		if (npc.life+damage >= npc.lifeMax) ModWorld.tookDamage[Main.myPlayer] = false;
		
		if (npc.life > 0) return;
		if (npc.name == "Pinky") ModPlayer.ExternalAchieve("TERRARIA_KILLPINKY");
		if (npc.name == "Guide") ModPlayer.ExternalAchieve("TERRARIA_KILLGUIDE");
		if ((npc.name == "Wyvern" || npc.displayName == "Wyvern") && !ModWorld.tookDamage[Main.myPlayer]) ModPlayer.ExternalAchieve("TERRARIA_HM_KILLWYVERNNODMG");
	}
}