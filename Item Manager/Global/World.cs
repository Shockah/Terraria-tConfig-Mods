public int OverrideItemRemove() {
	if (!Settings.GetBool("order")) {
		int num = 0, num2 = 0;
		for (int j = 0; j < 200; j++) if (Main.item[j].spawnTime > num2) {
			num2 = Main.item[j].spawnTime;
			num = j;
		}
		return num;
	}
	
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
	
	int val1 = item1.value*item1.stack, val2 = item2.value*item2.stack;
	float f = (float)(1f*Math.Max(val1+1,val2+1)/Math.Min(val1+1,val2+2));
	if (f > 3) return val1 > val2 ? i1 : i2;
	if (item1.spawnTime != item2.spawnTime) return item1.spawnTime < item2.spawnTime ? i1 : i2;
	if (f <= 3) return val1 > val2 ? i1 : i2;
	return item1;
}