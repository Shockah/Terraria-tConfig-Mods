public void PostItemCheck(Player player, int playerID) {
	if (player == null) return;
	if (player.whoAmi != Main.myPlayer) return;
	
	if (player.inventory[player.selectedItem].name == "Magic Mirror") ModPlayer.ExternalAchieve("TERRARIA_MAGICMIRROR");
	if (player.inventory[player.selectedItem].name == "Ale") ModPlayer.ExternalAchieve("TERRARIA_ALE");
	if (player.inventory[player.selectedItem].name == "Whoopie Cushion") ModPlayer.ExternalAchieve("TERRARIA_WHOOPIECUSHION");
}