#INCLUDE "ImplItemSlotRender.cs"

private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};

public void OnSetup() {
	ItemSlotRender.Register(new ImplItemSlotRender());
}

public static void DrawStringShadowed(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, Color colorShadow, Vector2 vec = default(Vector2), float scale = 1f, SpriteEffects effects = SpriteEffects.None) {
	foreach (Vector2 vecOff in shadowOffset) sb.DrawString(font,text,new Vector2(pos.X+vecOff.X,pos.Y+vecOff.Y),colorShadow,0f,vec,scale,effects,0f);
	sb.DrawString(font,text,pos,color,0f,vec,scale,effects,0f);
}