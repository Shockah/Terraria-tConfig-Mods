public int OverrideItemRemove() {
	Item toRemove = null;
	int toRemoveId = -1;
	for (int i = 0; i < Main.item.Length-1; i++) {
		toRemove = compare(toRemove,Main.item[i],false);
		if (Object.ReferenceEquals(toRemove,Main.item[i])) toRemoveId = i;
	}
	return toRemoveId;
}

private Item compare(Item item1, Item item2, bool best) {
	if ((item1 == null) ^ (item2 == null)) return item2 == null ? item1 : item2;
	if (item1.active ^ item2.active) return item1.active ? item1 : item2;
	if (!item1.active) return null;
	
	Item i1 = best ? item1 : item2, i2 = best ? item2 : item1;
	if (item1.rare != item2.rare) return item1.rare > item2.rare ? i1 : i2;
	if (item1.value*item1.stack != item2.value*item2.stack) return item1.value*item1.stack > item2.value*item2.stack ? i1 : i2;
	if (item1.spawnTime != item2.spawnTime) return item1.spawnTime < item2.spawnTime ? i1 : i2;
	return item1;
}