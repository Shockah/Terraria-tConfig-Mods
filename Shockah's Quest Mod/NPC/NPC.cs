public void DealtNPC(double damage, Player player) {
	if (npc.life > 0) return;
	if (player.whoAmi != Main.myPlayer) return;
	
	for (int i = 0; i < ModPlayer.quest.Length; i++) {
		if (ModPlayer.quest[i] == null) continue;
		if (!(ModPlayer.quest[i] is ModPlayer.QuestMonster)) continue;
		
		if (npc.townNPC || npc.friendly || npc.dontTakeDamage) continue;
		if (npc.value < 1) continue;
		
		ModPlayer.QuestMonster q = (ModPlayer.QuestMonster)ModPlayer.quest[i];
		
		string npcName1 = (npc.displayName != null && npc.displayName != "") ? npc.displayName : npc.name;
		string npcName2 = (q.kill.displayName != null && q.kill.displayName != "") ? q.kill.displayName : q.kill.name;
		if (npcName1 != npcName2) continue;
		
		q.progress++;
		if (q.progress >= q.progressTotal) {
			q.endQuest();
			ModPlayer.quest[i] = null;
		}
		break;
	}
}