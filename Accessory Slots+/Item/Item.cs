public bool AccCheck(Player player, int slot) {
	for (int i = 3; i <= 7; i++) if (player.armor[i].type == item.type) return false;
	for (int i = 0; i < Settings.GetInt("slots"); i++) if (ModPlayer.accessories[i].type == item.type) return false;
	return true;
}