public void Initialize() {
	if (item.maxStack > 1 && !item.name.EndsWith(" Coin")) {
		if (item.maxStack == 99) item.maxStack = 100;
		item.maxStack *= ModGeneric.maxStackMultiplier;
	}
}