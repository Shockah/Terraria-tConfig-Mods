public class Firework {
	public static List<ModGeneric.Pair<string,Color>> colors = new List<ModGeneric.Pair<string,Color>>();
	
	static Firework() {
		colors.Add(new ModGeneric.Pair<string,Color>("Amethyst",new Color(255,0,255)));
		colors.Add(new ModGeneric.Pair<string,Color>("Topaz",new Color(255,191,0)));
		colors.Add(new ModGeneric.Pair<string,Color>("Sapphire",new Color(31,31,255)));
		colors.Add(new ModGeneric.Pair<string,Color>("Emerald",new Color(0,255,0)));
		colors.Add(new ModGeneric.Pair<string,Color>("Ruby",new Color(255,0,0)));
		colors.Add(new ModGeneric.Pair<string,Color>("Diamond",new Color(255,255,255)));
		//TODO add more items / colors
	}
	
	public static Color? GetColor(params Item[] items) {
		int r = 0, g = 0, b = 0, total = 0;
		foreach (Item item in items) {
			if (item != null && item.stack > 0 && item.name != null && item.name != "") {
				foreach (ModGeneric.Pair<string,Color> pair in colors) if (item.name == pair.A) {
					r += pair.B.R;
					g += pair.B.G;
					b += pair.B.B;
					total++;
					break;
				}
			}
		}
		if (total > 0) return new Color?(new Color((int)(1f*r/total),(int)(1f*g/total),(int)(1f*b/total)));
		return new Color?();
	}
	
	public readonly int seed;
	public Random rnd;
	public int animLeft;
	public List<Spark> sparks = new List<Spark>();
	
	public Firework(int seed, int animTime) {
		this.seed = seed;
		rnd = new Random(seed);
		animLeft = animTime;
	}
	
	public virtual bool DrawProjectile(SpriteBatch sb, Projectile p) {
		return true;
	}
	public virtual void Draw(SpriteBatch sb, Projectile p) {}
	public virtual void PostKill(Projectile p) {}
}