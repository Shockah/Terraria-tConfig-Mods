public class Spark {
	public static Texture2D ptFuzzy = Main.goreTexture[Config.goreID["ParticleFuzzy"]];
	
	public static Rectangle? GetTexRectangle(Texture2D tex) {
		return new Rectangle?(new Rectangle(0,0,tex.Width,tex.Height));
	}
	public static Vector2 GetTexCenter(Texture2D tex) {
		return new Vector2(tex.Width/2f,tex.Height/2f);
	}
	public static float GetTexScale(Texture2D tex, float px) {
		return 1f/tex.Width*px;
	}
	
	public static Rectangle? GetRectFuzzy() {return GetTexRectangle(ptFuzzy);}
	public static Vector2 GetCenterFuzzy() {return GetTexCenter(ptFuzzy);}
	public static float GetScaleFuzzy(float px) {return GetTexScale(ptFuzzy,px);}
	
	public Vector2 pos;
	public bool dead = false;
	
	public Spark(Vector2 pos) {
		this.pos = pos;
	}
	
	public virtual void Draw(SpriteBatch sb, Projectile p, Firework f) {}
}