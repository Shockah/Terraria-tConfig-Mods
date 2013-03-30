public class OSIAchievements : OnScreenInterfaceable {
	public static void Register() {
		OnScreenInterface.Register(new OSIAchievements(),OnScreenInterface.LAYER_INTERFACE_SCREEN);
	}
	
	public void DrawOnScreen(SpriteBatch sb, double layer) {
		for (int i = 0; i < notifiers.Count; i++) notifiers[i].Draw(sb,i);
		GuiAchievements.Draw(sb);
	}
}