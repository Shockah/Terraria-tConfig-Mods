public bool CanUse(Player player, int pID) {
	return ModPlayer.HasFreeStack(player);
}

public void UseItem(Player player, int pID) { 
    ModGeneric.WeightedRandom<Item> wr = new ModGeneric.WeightedRandom<Item>();
	Item item;
	
	item = new Item();
	item.SetDefaults("Candy Cane Block");
	item.stack = wr.rnd.Next(20,50+1);
	wr.Add(item,7);
	
	item = new Item();
	item.SetDefaults("Green Candy Cane Block");
	item.stack = wr.rnd.Next(20,50+1);
	wr.Add(item,7);
	
	if (Main.hardMode) {
		item = new Item();
		item.SetDefaults("Snow Globe");
		wr.Add(item,1);
	}
	
	ModPlayer.GiveItem(player,wr.Get());
}