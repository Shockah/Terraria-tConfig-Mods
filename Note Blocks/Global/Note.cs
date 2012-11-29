public class Note {
	public static Texture2D tex = Main.goreTexture[Config.goreID["Note"]];
	
	public Vector2 pos;
	public Color color;
	public float speed = 8f, alpha = 1f;
	
	public Note(Vector2 pos, Color color) {
		this.pos = new Vector2(pos.X,pos.Y);
		this.color = new Color(color.R,color.G,color.B);
	}
	
	public void Update() {
		if (alpha <= 0) return;
		pos.Y -= speed;
		speed /= 2f;
		
		if (speed <= .25f) alpha -= .1f;
	}
	public void Draw(SpriteBatch sb) {
		if (alpha <= 0) return;
		sb.Draw(tex,new Vector2(pos.X-Main.screenPosition.X,pos.Y-Main.screenPosition.Y),new Rectangle?(new Rectangle(0,0,tex.Width,tex.Height)),color,0f,new Vector2(tex.Width/2,tex.Height/2),alpha,SpriteEffects.None,0f);
	}
}