public abstract class GuiCheat : Interfaceable {
	#INCLUDE "Util.cs"
	
	private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};
	public static Microsoft.Xna.Framework.Input.MouseState? state = null, stateOld = null;
	public static Texture2D whiteTex = null;
	
	static GuiCheat() {
		if (Main.dedServ) return;
		
		whiteTex = new Texture2D(Config.mainInstance.GraphicsDevice,1,1);
		whiteTex.SetData(new Color[]{Color.White});
	}
	
	public static void DrawStringShadowed(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, Color colorShadow, Vector2 origin = default(Vector2), float scale = 1f, SpriteEffects effects = SpriteEffects.None) {
		if (text == null) return;
		foreach (Vector2 vecOff in shadowOffset) sb.DrawString(font,text,new Vector2(pos.X+vecOff.X,pos.Y+vecOff.Y),colorShadow,0f,origin,scale,effects,0f);
		sb.DrawString(font,text,pos,color,0f,origin,scale,effects,0f);
	}
	
	public static Texture2D CreateCircleTexture(int radius) {
		int outerRadius = radius*2+2;
		Texture2D texture = new Texture2D(Config.mainInstance.GraphicsDevice,outerRadius,outerRadius);

		Color[] data = new Color[outerRadius*outerRadius];
		Color TransparentWhite = new Color(255,255,255,0);
		for (int i = 0; i < data.Length; i++) data[i] = TransparentWhite;

		double angleStep = 1f/radius;
		for (double angle = 0; angle < Math.PI*2; angle += angleStep) {
			int x = (int)Math.Round(radius+radius*Math.Cos(angle));
			int y = (int)Math.Round(radius+radius*Math.Sin(angle));
			data[y*outerRadius+x+1] = Color.White;
		}

		texture.SetData(data);
		return texture;
	}
	
	public abstract bool DropSlot(int slot);
	public abstract void ButtonClicked(int num);
	public abstract void PlaceSlot(int slot);
	public abstract bool CanPlaceSlot(int slot, Item item);
	
	public virtual void PreDrawInterface(SpriteBatch sb) {}
	public virtual void PostDraw(SpriteBatch sb) {}
	public virtual bool PretendChat() {
		return false;
	}
}