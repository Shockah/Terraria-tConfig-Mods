public class OSIEffects : OnScreenInterfaceable {
	public static void Register() {
		OnScreenInterface.Register(new OSIEffects(),OnScreenInterface.LAYER_TILE);
	}
	
	public void DrawOnScreen(SpriteBatch sb, double layer) {
		sb.End();
		sb.Begin(SpriteSortMode.Immediate,BlendState.Additive);
		foreach (ModWorld.Effect e in ModWorld.effects) e.Draw(sb);
		sb.End();
		sb.Begin();
	}
}