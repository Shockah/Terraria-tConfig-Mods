public void Initialize() {
	if (!Settings.Initialized()) return;
	if (item.maxStack > 1 && !item.name.EndsWith(" Coin") && item.name != "Light Disc") {
		if (item.maxStack == 99) item.maxStack = 100;
		item.maxStack *= Settings.GetInt("mult");
	}
}