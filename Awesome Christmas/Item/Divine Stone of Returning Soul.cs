public bool CanUse(Player player, int pID) {
	if (Main.netMode == 0) return false;
	foreach (Player p in Main.player) {
		if (!p.active) continue;
		if (p.whoAmi == player.whoAmi) continue;
		if (ModPlayer.ghosts[p.whoAmi]) return true;
	}
	return false;
}

public void UseItem(Player player, int pID) { 
	List<Player> list = new List<Player>();
	foreach (Player p in Main.player) {
		if (!p.active) continue;
		if (p.whoAmi == player.whoAmi) continue;
		if (ModPlayer.ghosts[p.whoAmi]) list.Add(p);
	}
	
	double dist = -1;
	Player nearest = null;
	foreach (Player p in list) {
		double _d = Math.Sqrt(Math.Pow(p.position.X-player.position.X,2)+Math.Pow(p.position.Y-player.position.Y,2));
		if (dist == -1 || _d < dist) {
			dist = _d;
			nearest = p;
		}
	}
	
	if (Main.netMode == 1) {
		NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_UNGHOST,-1,-1,(byte)nearest.whoAmi);
	}
}