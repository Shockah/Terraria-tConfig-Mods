public void UseItem(Player player, int playerID) {
	player.itemTime = player.inventory[player.selectedItem].useTime;
	player.statLifeMax += 20;
	player.statLife += 20;
	if (Main.myPlayer == player.whoAmi) player.HealEffect(20);
}

public bool CanUse(Player player, int playerID) {
	return player.statLifeMax < ModGeneric.HealthUpMaxHealth;
}