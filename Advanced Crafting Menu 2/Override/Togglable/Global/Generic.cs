public static bool instaShow = false;

public void OnRecipeRefresh() {
	if (Config.tileInterface != null && Config.tileInterface.code is ModWorld.GuiCraft) {
		ModWorld.GuiCraft.Filter();
		ModWorld.GuiCraft.Sort();
		ModWorld.GuiCraft.Create(Config.tileInterface.code is ModWorld.GuiCraft ? ModWorld.GuiCraft.scroll : 0);
	}
}