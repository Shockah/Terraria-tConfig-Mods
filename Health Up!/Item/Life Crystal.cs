public void Initialize() {
	if (!Settings.Initialized()) return;
	if (item.toolTip == "Permanently increases maximum life by") item.toolTip += " "+(20*Settings.GetInt("multCrystalLife"));
}

public void UseItem(Player player, int playerID) {
	int val = 20*Settings.GetInt("multCrystalLife");
	
	player.itemTime = player.inventory[player.selectedItem].useTime;
	player.statLifeMax += val;
	player.statLife += val;
	if (Main.myPlayer == player.whoAmi) player.HealEffect(val);
}

public bool CanUse(Player player, int playerID) {
	return player.statLifeMax < 400*Settings.GetInt("multLife");
}