public class Notifier {
	public static Texture2D frame1 = null, frame2 = null;
	
	static Notifier() {
		if (Main.dedServ) return;
		
		frame1 = new Texture2D(Config.mainInstance.GraphicsDevice,1,56);
		Color[] bgc = new Color[56];
		for (int i = 0; i < bgc.Length; i++) bgc[i] = Color.Lerp(new Color(95,95,95),new Color(15,15,15),1f*i/bgc.Length);
		bgc[0] = Color.Lerp(bgc[0],Color.White,.5f);
		bgc[bgc.Length-1] = Color.Lerp(bgc[bgc.Length-1],Color.White,.5f);
		frame1.SetData(bgc);
		
		frame2 = new Texture2D(Config.mainInstance.GraphicsDevice,1,56);
		for (int i = 1; i < bgc.Length-1; i++) bgc[i] = Color.Lerp(bgc[i],Color.White,.5f);
		frame2.SetData(bgc);
	}
	
	public readonly ModPlayer.Achievement ac;
	public int y = 0, life = 0;
	public bool dead = false;
	
	public Notifier(ModPlayer.Achievement ac) {
		this.ac = ac;
	}
	
	public void Update(int index) {
		if (dead) return;
		
		if (life < 300) {
			if (y < (index+1)*56) y += 4;
			else if (y > (index+1)*56) y -= 4;
			else life++;
		} else {
			y -= 4;
			if (y <= 0) dead = true;
		}
	}
	public void Draw(SpriteBatch sb, int index) {
		if (dead) return;
		int w = (int)(Math.Max(Main.fontMouseText.MeasureString(ac.title).X,Main.fontMouseText.MeasureString(ac.description).X*.75f)+(ac.tex == null ? 0 : 52)+8);
		
		int xx = (int)((Main.screenWidth-w)/2f), yy = Main.screenHeight-y;
		
		sb.Draw(frame1,new Rectangle(xx,yy,w,56),Color.White);
		sb.Draw(frame2,new Rectangle(xx,yy,1,56),Color.White);
		sb.Draw(frame2,new Rectangle(xx+w,yy,1,56),Color.White);
		
		xx += 4;
		if (ac.tex != null) {
			float scale = 1f;
			if (ac.tex.Width > 48) scale = 48f/ac.tex.Width;
			if (ac.tex.Height*scale > 48f) scale = 48f/ac.tex.Height;
			sb.Draw(ac.tex,new Rectangle((int)(xx+(48-ac.tex.Width*scale)/2f),(int)(yy+4+(48-ac.tex.Height*scale)/2f),(int)(ac.tex.Width*scale),(int)(ac.tex.Height*scale)),Color.White);
			xx += 52;
		}
		ModWorld.DrawStringShadowed(sb,Main.fontMouseText,ac.title,new Vector2(xx,yy+8),Color.White,Color.Black);
		ModWorld.DrawStringShadowed(sb,Main.fontMouseText,ac.description,new Vector2(xx,yy+28),Color.White,Color.Black,default(Vector2),.75f);
	}
}