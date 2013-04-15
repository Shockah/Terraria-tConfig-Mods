public class FireworkTwoWave : Firework {
	public bool fired = false;
	
	public FireworkTwoWave(int seed) : base(seed,0) {}
	
	public override bool DrawProjectile(SpriteBatch sb, Projectile p) {
		return false;
	}
	public override void Draw(SpriteBatch sb, Projectile p) {
		Vector2 pc = p.position+new Vector2(p.width/2f,p.height/2f);
		
		if (fired) {
			sb.End();
			sb.Begin(SpriteSortMode.Immediate,BlendState.Additive);
			
			foreach (Spark spark in sparks) spark.Draw(sb,p,this);
			for (int i = 0; i < sparks.Count; i++) if (sparks[i].dead) sparks.RemoveAt(i--);
			
			sb.End();
			sb.Begin();
			
			if (sparks.Count == 0) p.Kill();
		} else {
			fired = true;
			
			int r, g, b;
			Color c;
			float degrees;
			
			byte[] bytes = BitConverter.GetBytes(seed);
			Random rnd = new Random();
			
			degrees = (float)(rnd.NextDouble()*360d);
			HsvToRgb(bytes[0]*360f/255f,bytes[1]/255f,1f,out r,out g,out b);
			c = new Color(r,g,b);
			for (int i = 0; i < 20; i++) sparks.Add(new MySpark(pc,Util.Vector((float)(1.5f+rnd.NextDouble()*.75f),360f/20f*i+degrees),c,80+rnd.Next(40)));
			
			degrees = (float)(rnd.NextDouble()*360d);
			HsvToRgb(bytes[2]*360f/255f,bytes[3]/255f,1f,out r,out g,out b);
			c = new Color(r,g,b);
			for (int i = 0; i < 40; i++) sparks.Add(new MySpark(pc,Util.Vector((float)(3f+rnd.NextDouble()*1.5f),360f/40f*i+degrees),c,80+rnd.Next(40)));
		}
	}
	
	public class MySpark : Spark {
		public List<Vector2> trail = new List<Vector2>();
		
		public Vector2 vel;
		public Color color;
		public int timeLeft;
		
		public MySpark(Vector2 pos, Vector2 vel, Color color, int timeLeft) : base(pos) {
			this.vel = vel;
			this.color = color;
			this.timeLeft = timeLeft;
		}
		
		public override void Draw(SpriteBatch sb, Projectile p, Firework f) {
			trail.Insert(0,new Vector2(pos.X,pos.Y));
			for (int i = 0; i < Math.Min(trail.Count,10); i++) {
				Vector2 v = trail[i];
				float alpha = (10-i)/10f;
				sb.Draw(ptFuzzy,v-Main.screenPosition,GetRectFuzzy(),new Color(color.R,color.G,color.B,(byte)(color.A*alpha*.5f)),0f,GetCenterFuzzy(),GetScaleFuzzy(36f),SpriteEffects.None,0f);
				sb.Draw(ptFuzzy,v-Main.screenPosition,GetRectFuzzy(),new Color(color.R,color.G,color.B,(byte)(color.A*alpha)),0f,GetCenterFuzzy(),GetScaleFuzzy(12f),SpriteEffects.None,0f);
			}
			if (trail.Count > 10) trail.RemoveAt(10);
			Lighting.addLight((int)Math.Round(pos.X/16f),(int)Math.Round(pos.Y/16f),color.R/255f*(color.A/255f),color.G/255f*(color.A/255f),color.B/255f*(color.A/255f));
			
			pos += vel;
			vel.Y += .075f;
			
			if (timeLeft > 0) {
				timeLeft--;
			} else {
				color.A = (byte)Math.Max(color.A-10,0);
				if (color.A == 0) dead = true;
			}
		}
	}
}