public void NPCLoot() {
	if (!ModWorld.flagFrostmaw) return;
	
	if (npc.friendly) return;
	if (npc.value == 0 && npc.npcSlots == 0f) return;
	if (Main.rand.Next(1000) == 0) {
		Item.NewItem((int)npc.position.X,(int)npc.position.Y,npc.width,npc.height,"Divine Stone of Returning Soul",1,false,0);
	}
}