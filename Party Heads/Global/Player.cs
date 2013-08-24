public static List<Object> ExternalGetPartyInfo(int pID) {
	if (pID < 0 || pID > Main.player.Length-1) return null;
	List<Object> ret = new List<Object>();
	
	Player p = Main.player[pID];
	ret.Add(p.team);
	
	List<Player> members = new List<Player>();
	if (p.team != 0) foreach (Player p2 in Main.player) {
		if (!p2.active || p2.whoAmi == pID) continue;
		if (p.team == p2.team) members.Add(p2);
	}
	ret.Add(members);
	
	return ret;
}