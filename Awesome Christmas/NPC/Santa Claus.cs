public void SetupShop(Chest chest) {
	Item item;
	
	item = new Item(); item.SetDefaults(588,false); AddToChest(chest,item);
	item = new Item(); item.SetDefaults(589,false); AddToChest(chest,item);
	item = new Item(); item.SetDefaults(590,false); AddToChest(chest,item);
	item = new Item(); item.SetDefaults(597,false); AddToChest(chest,item);
	item = new Item(); item.SetDefaults(598,false); AddToChest(chest,item);
	item = new Item(); item.SetDefaults(596,false); AddToChest(chest,item);
	
	item = new Item(); item.SetDefaults("Pine Tree"); AddToChest(chest,item);
}

private void AddToChest(Chest chest, Item item) {
	for (int i = 0; i < Chest.maxItems; i++) {
		if (chest.item[i] == null || chest.item[i].name == null || chest.item[i].name == "" || chest.item[i].stack <= 0) {
			chest.item[i] = item;
			break;
		}
	}
}