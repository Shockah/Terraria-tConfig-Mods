public bool AccCheck(Player player, int slot) {
	for (int i = 3; i <= 7; i++) if (player.armor[i].type == item.type) return false;
	for (int i = 0; i < ModGeneric.extraSlots; i++) if (ModWorld.slots.itemSlots[i].type == item.type) return false;
	return true;
}