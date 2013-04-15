public bool isOn = false;
public int fuel = 0;

public void Save(BinaryWriter bw) {
	bw.Write(isOn);
	bw.Write((short)fuel);
}
public void Load(BinaryReader br, int version) {
	isOn = br.ReadBoolean();
	fuel = br.ReadInt16();
}

public void UseItem(Player player, int playerID) {
	isOn = !isOn;
	fuel = 0;
}

public void HoldStyle(Player player) {
	if (isOn) {
		if (fuel == 0) {
			Item item = new Item();
			item.SetDefaults("Gel");
			if (ModPlayer.EatItem(player,item)) fuel += 60*5;
		}
		if (fuel > 0) {
			if (player.wet || player.lavaWet) {
				isOn = false;
				fuel = 0;
				return;
			}
			fuel--;
			
			Random rnd = new Random();
			Vector2 pc = player.position+new Vector2(player.width/2f,player.height/2f);
			Vector2 offset = new Vector2(20f*player.direction,-10f*player.gravDir);
			Vector2 v = pc+offset;
			Color c = new Color(Math.Min((int)((1f-.1f+rnd.NextDouble()*.2f)*255f),255),Math.Min((int)((.9f-.2f+rnd.NextDouble()*.4f)*255f),255),Math.Min((int)((.7f-.2f+rnd.NextDouble()*.4f)*255f),255));
			AddSpark(new MySpark(v,ModWorld.Util.Vector((float)(.5f+rnd.NextDouble()*.25f),(float)(rnd.NextDouble()*360d)),c,10+rnd.Next(5)));
			Lighting.addLight((int)Math.Round(v.X/16f),(int)Math.Round(v.Y/16f),c.R/255f/2f,c.G/255f/2f,c.B/255f/2f);
		} else isOn = false;
	}
}

public void AddSpark(ModWorld.Spark spark) {
	if (spark == null) return;
	if (Main.netMode == 2) return;
	ModWorld.sparks.Add(spark);
}

public class MySpark : ModWorld.Spark {
	public List<Vector2> trail = new List<Vector2>();
	
	public Vector2 vel;
	public Color color;
	public int timeLeft;
	
	public MySpark(Vector2 pos, Vector2 vel, Color color, int timeLeft) : base(pos) {
		this.vel = vel;
		this.color = color;
		this.timeLeft = timeLeft;
	}
	
	public override void Draw(SpriteBatch sb, Projectile p, ModWorld.Firework f) {
		trail.Insert(0,new Vector2(pos.X,pos.Y));
		for (int i = 0; i < Math.Min(trail.Count,5); i++) {
			Vector2 v = trail[i];
			float alpha = (5-i)/5f;
			sb.Draw(ptFuzzy,v-Main.screenPosition,GetRectFuzzy(),new Color(color.R,color.G,color.B,(byte)(color.A*alpha*.5f)),0f,GetCenterFuzzy(),GetScaleFuzzy(12f),SpriteEffects.None,0f);
			sb.Draw(ptFuzzy,v-Main.screenPosition,GetRectFuzzy(),new Color(color.R,color.G,color.B,(byte)(color.A*alpha)),0f,GetCenterFuzzy(),GetScaleFuzzy(4f),SpriteEffects.None,0f);
		}
		if (trail.Count > 5) trail.RemoveAt(5);
		
		pos += vel;
		vel.Y += .05f;
		
		if (timeLeft > 0) {
			timeLeft--;
		} else {
			color.A = (byte)Math.Max(color.A-26,0);
			if (color.A == 0) dead = true;
		}
	}
}