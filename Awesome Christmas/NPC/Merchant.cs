public void SetupShop(Chest chest) {
	Item item;
	
	item = new Item(); item.SetDefaults("Mining Helmet"); AddToChest(chest,item);
	item = new Item(); item.SetDefaults("Piggy Bank"); AddToChest(chest,item);
	item = new Item(); item.SetDefaults("Iron Anvil"); AddToChest(chest,item);
	item = new Item(); item.SetDefaults("Copper Pickaxe"); AddToChest(chest,item);
	item = new Item(); item.SetDefaults("Copper Axe"); AddToChest(chest,item);
	item = new Item(); item.SetDefaults("Torch"); AddToChest(chest,item);
	item = new Item(); item.SetDefaults("Lesser Healing Potion"); AddToChest(chest,item);
	if (Main.player[Main.myPlayer].statManaMax >= 200) {item = new Item(); item.SetDefaults("Lesser Mana Potion"); AddToChest(chest,item);}
	item = new Item(); item.SetDefaults("Wooden Arrow"); AddToChest(chest,item);
	item = new Item(); item.SetDefaults("Shuriken"); AddToChest(chest,item);
	if (Main.bloodMoon) {item = new Item(); item.SetDefaults("Throwing Knife"); AddToChest(chest,item);}
	if (!Main.dayTime) {item = new Item(); item.SetDefaults("Glowstick"); AddToChest(chest,item);}
	if (NPC.downedBoss3) {item = new Item(); item.SetDefaults("Safe"); AddToChest(chest,item);}
	if (Main.hardMode) {item = new Item(); item.SetDefaults(488,false); AddToChest(chest,item);}
	
	item = new Item(); item.SetDefaults("Wrapping Paper"); AddToChest(chest,item);
}

private void AddToChest(Chest chest, Item item) {
	for (int i = 0; i < Chest.maxItems; i++) {
		if (chest.item[i] == null || chest.item[i].name == null || chest.item[i].name == "" || chest.item[i].stack <= 0) {
			chest.item[i] = item;
			break;
		}
	}
}