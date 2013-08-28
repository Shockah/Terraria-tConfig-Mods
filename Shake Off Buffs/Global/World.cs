public void PostDraw(SpriteBatch sb) {
	for (int i = 0; i < Main.player.Length; i++) {
		Player p = Main.player[i];
		if (p.active) continue;
		
		ModPlayer.countIronskin[i] = 0;
		ModPlayer.countMagicPower[i] = 0;
		
		ModPlayer.oldHasBuff[i] = new Dictionary<string,int>();
		foreach (string s in ModPlayer.checkBuffs) ModPlayer.oldHasBuff[i][s] = -1;
	}
}

public void PlayerConnected(int pID) {
	ModPlayer.countIronskin[pID] = 0;
	ModPlayer.countMagicPower[pID] = 0;
	
	ModPlayer.oldHasBuff[pID] = new Dictionary<string,int>();
	foreach (string s in ModPlayer.checkBuffs) ModPlayer.oldHasBuff[pID][s] = -1;
}