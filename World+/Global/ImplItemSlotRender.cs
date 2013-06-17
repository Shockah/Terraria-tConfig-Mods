public class ImplItemSlotRender : IItemSlotRender {
	public void PreDrawItemInSlot(SpriteBatch sb, Color color, Item item, Vector2 pos, float sc, ref bool letDraw) {
		if (item.name != "Firefly in a Jar") return;
		
		if (!item.RunMethod("ExternalGetFirefly")) return;
		ModWorld.EffectFirefly firefly = (ModWorld.EffectFirefly)Codable.customMethodReturn;
		if (firefly == null) return;
		
		sb.End();
		sb.Begin(SpriteSortMode.Immediate,BlendState.Additive);
		firefly.DrawItem(sb,pos.X,(int)(pos.Y+2*sc),sc);
		sb.End();
		sb.Begin();
		ModWorld.effectsExtraUpdate.Add(firefly);
	}
	public void PostDrawItemInSlot(SpriteBatch sb, Color color, Item item, Vector2 pos, float sc, bool ranVanilla) {}
}