public void Initialize() {
	if (!Settings.Initialized()) return;
	if (item.toolTip == "Permanently increases maximum mana by") item.toolTip += " "+(20*Settings.GetInt("multCrystalMana"));
}

public void UseItem(Player player, int playerID) {
	int val = 20*Settings.GetInt("multCrystalMana");
	
	player.itemTime = player.inventory[player.selectedItem].useTime;
	player.statManaMax += val;
	player.statMana += val;
	if (Main.myPlayer == player.whoAmi) player.ManaEffect(val);
}

public bool CanUse(Player player, int playerID) {
	return player.statManaMax < 200*Settings.GetInt("multMana");
}