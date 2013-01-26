public void UpdateSpawn() {
	if (ModWorld.enabledNoSpawns) {
		NPC.spawnRate = 0;
		NPC.maxSpawns = 0;
	}
}