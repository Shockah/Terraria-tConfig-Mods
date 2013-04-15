public void Initialize() {
	if (item.maxStack > 1 && !item.name.EndsWith(" Coin")) item.maxStack *= ModGeneric.maxStackMultiplier;
}