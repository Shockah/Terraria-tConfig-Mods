public void UseItem(Player player, int playerID) {
	player.itemTime = player.inventory[player.selectedItem].useTime;
	player.statManaMax += 20;
	player.statMana += 20;
	if (Main.myPlayer == player.whoAmi) player.ManaEffect(20);
}

public bool CanUse(Player player, int playerID) {
	return player.statManaMax < ModGeneric.HealthUpMaxMana;
}