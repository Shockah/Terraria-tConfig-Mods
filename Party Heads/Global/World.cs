public bool PreDrawPartyText(SpriteBatch sb) {
	if (!Settings.GetBool("enabled")) return true;
	
	List<Object> party = ModPlayer.ExternalGetPartyInfo(Main.myPlayer);
	if (party == null) return false;
	Player p = Main.player[Main.myPlayer];
	
	List<Head> heads = new List<Head>();
	foreach (Player p2 in (List<Player>)party[1]) {
		if (p2.dead) continue;
		if (new System.Drawing.Rectangle((int)-p2.width,(int)-p2.height,(int)(Main.screenWidth+p2.width*2),(int)(Main.screenHeight+p2.height*2)).Contains(new System.Drawing.Rectangle((int)(p2.position.X-Main.screenPosition.X),(int)(p2.position.Y-Main.screenPosition.Y),(int)p2.width,(int)p2.height))) continue;
		
		double angle = Math.Atan2(p.position.Y-p2.position.Y,p2.position.X-p.position.X)*180d/Math.PI;
		foreach (Head h in heads) {
			double diff = Math.Abs(h.angle-angle);
			if (diff > 180d) {
				diff = 360d-diff;
				angle -= 360d;
			}
			if (diff < h.players.Count*16d-8d) {
				h.angle = (h.angle*h.players.Count+angle)/(h.players.Count+1);
				h.players.Add(p2);
				goto L;
			}
		}
		
		heads.Add(new Head(p2,angle));
		L: {}
	}
	
	int
		CONST_EDGE = Settings.GetInt("edge"),
		CONST_MIN = Settings.GetInt("scaleMin"),
		CONST_MAX = Settings.GetInt("scaleMax"),
		CONST_DIST = Settings.GetInt("distMax");
	
	Vector2 center = new Vector2(Main.screenWidth/2,Main.screenHeight/2);
	foreach (Head h in heads) {
		for (int i = 0; i < h.players.Count; i++) {
			Player dp = h.players[i];
			double angle = h.angle-(h.players.Count-1)*5d+(i*10);
			while (angle >= 360) angle -= 360;
			while (angle < 0) angle += 360;
			
			Vector2 v = new Vector2((float)(-Math.Cos((angle+180d)*Math.PI/180d)),(float)(Math.Sin((angle+180d)*Math.PI/180d)));
			Vector2 v2 = new Vector2(v.X,v.Y);
			v2 = v*((Main.screenHeight/2-CONST_EDGE)/v.Y);
			if (Math.Abs(v2.X) > Main.screenWidth/2-CONST_EDGE) v2 = v*((Main.screenWidth/2-CONST_EDGE)/v.X);
			if (Math.Sign(v2.X) == Math.Sign(v.X)) v2 *= -1;
			
			SpriteEffects effects = SpriteEffects.None;
			Color color = Color.White;
			Color color2 = dp.eyeColor;
			Color color3 = dp.hairColor;
			Color color4 = dp.skinColor;
			if (dp.gravDir == 1f) {
				effects = dp.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			} else {
				effects = dp.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.FlipHorizontally|SpriteEffects.FlipVertically;
			}
			
			if (Codable.RunPlayerMethodRef("DrawBeforeHead",false,dp,sb,true,color4,color,color2,color3,Color.Transparent)) {
				if (Codable.customMethodRefReturn != null) {
					object[] ret = Codable.customMethodRefReturn;
					color4 = (Color)ret[3];
					color = (Color)ret[4];
					color2 = (Color)ret[5];
					color3 = (Color)ret[6];
				}
			}
			
			Vector2 pos = center-v2;
			Vector2 origin = new Vector2(dp.bodyFrame.Width/2,dp.bodyFrame.Height/2-8);
			if (dp.gravDir != 1f) pos.Y -= 12;
			Rectangle? rect = new Rectangle?(new Rectangle(0,0,dp.bodyFrame.Width,dp.bodyFrame.Height));
			double dist = Math.Sqrt(Math.Pow(p.position.X-dp.position.X,2)+Math.Pow(p.position.Y-dp.position.Y,2))/8d;
			float ratio = (float)(1-Math.Min(Math.Max(dist/CONST_DIST,0),1));
			float scale = CONST_MIN+(CONST_MAX-CONST_MIN)*ratio;
			
			sb.Draw(Main.playerHeadTexture,pos,rect,color4,0f,origin,scale/100f,effects,0f);
			sb.Draw(Main.playerEyeWhitesTexture,pos,rect,color,0f,origin,scale/100f,effects,0f);
			sb.Draw(Main.playerEyesTexture,pos,rect,color2,0f,origin,scale/100f,effects,0f);
			sb.Draw(Main.playerHairTexture[dp.hair],pos,rect,color3,0f,origin,scale/100f,effects,0f);
		}
	}
	
	return false;
}

public class Head {
	public List<Player> players = new List<Player>();
	public double angle;
	
	public Head(Player player, double angle) {
		players.Add(player);
		this.angle = angle;
	}
}