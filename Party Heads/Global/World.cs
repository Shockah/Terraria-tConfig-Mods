private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};

public static void DrawStringShadowed(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, Color colorShadow, Vector2 origin = default(Vector2), float scale = 1f, float rotation = 0f, SpriteEffects effects = SpriteEffects.None) {
	if (text == null) return;
	foreach (Vector2 vecOff in shadowOffset) sb.DrawString(font,text,new Vector2(pos.X+vecOff.X,pos.Y+vecOff.Y),colorShadow,(float)(rotation*Math.PI/180f),origin,scale,effects,0f);
	sb.DrawString(font,text,pos,color,(float)(rotation*Math.PI/180f),origin,scale,effects,0f);
}
public static float LdirX(double dist, double angle) {
	return (float)(-Math.Cos((angle+180)*Math.PI/180f)*dist);
}
public static float LdirY(double dist, double angle) {
	return (float)(Math.Sin((angle+180)*Math.PI/180f)*dist);
}

public static Color GetColorByRatio(float ratio) {
	if (ratio < .25f) return Color.Lerp(Color.Red,Color.Orange,ratio*4f);
	if (ratio < .5f) return Color.Lerp(Color.Orange,Color.Yellow,(ratio-.25f)*4f);
	return Color.Lerp(Color.Yellow,Color.Lime,(ratio-.5f)*2f);
}

public bool PreDrawPartyText(SpriteBatch sb) {
	if (!Settings.GetBool("enabled")) return true;
	
	bool CONST_COMBINE = Settings.GetBool("combine");
	
	List<Object> party = ModPlayer.ExternalGetPartyInfo(Main.myPlayer);
	if (party == null) return false;
	Player p = Main.player[Main.myPlayer];
	
	List<Head> heads = new List<Head>();
	foreach (Player p2 in (List<Player>)party[1]) {
		if (p2.dead) continue;
		if (new System.Drawing.Rectangle((int)-p2.width,(int)-p2.height,(int)(Main.screenWidth+p2.width*2),(int)(Main.screenHeight+p2.height*2)).Contains(new System.Drawing.Rectangle((int)(p2.position.X-Main.screenPosition.X),(int)(p2.position.Y-Main.screenPosition.Y),(int)p2.width,(int)p2.height))) continue;
		
		double angle = Math.Atan2(p.position.Y-p2.position.Y,p2.position.X-p.position.X)*180d/Math.PI;
		if (CONST_COMBINE) {
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
		}
		
		heads.Add(new Head(p2,angle));
		L: {}
	}
	
	int
		CONST_EDGE = Settings.GetInt("edge"),
		CONST_MIN = Settings.GetInt("scaleMin"),
		CONST_MAX = Settings.GetInt("scaleMax"),
		CONST_DIST = Settings.GetInt("distMax");
	string CONST_TEXT_STYLE = Settings.GetChoice("textStyle");
	bool
		CONST_TEXT_COLORHP = Settings.GetBool("textColorHP"),
		CONST_TEXT_DISPLAY_NAME = Settings.GetBool("textDisplayName"),
		CONST_TEXT_DISPLAY_HP = Settings.GetBool("textDisplayHP");
	
	Vector2 center = new Vector2(Main.screenWidth/2,Main.screenHeight/2);
	foreach (Head h in heads) {
		for (int i = 0; i < h.players.Count; i++) {
			Player dp = h.players[i];
			double angle = h.angle-(h.players.Count-1)*5d+(i*10);
			while (angle >= 360) angle -= 360;
			while (angle < 0) angle += 360;
			
			int side = 0;
			Vector2 v = new Vector2((float)(-Math.Cos((angle+180d)*Math.PI/180d)),(float)(Math.Sin((angle+180d)*Math.PI/180d)));
			Vector2 v2 = new Vector2(v.X,v.Y);
			v2 = v*((Main.screenHeight/2-CONST_EDGE)/v.Y);
			if (Math.Abs(v2.X) > Main.screenWidth/2-CONST_EDGE) {
				v2 = v*((Main.screenWidth/2-CONST_EDGE)/v.X);
				side = 2;
			}
			if (Math.Sign(v2.X) == Math.Sign(v.X)) {
				v2 *= -1;
				side++;
			}
			
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
			
			if (CONST_TEXT_DISPLAY_NAME || CONST_TEXT_DISPLAY_HP) {
				string text = "";
				if (CONST_TEXT_DISPLAY_NAME) text += dp.name;
				if (CONST_TEXT_DISPLAY_HP) {
					if (text != "") text += "\n";
					text += ""+dp.statLife+"/"+dp.statLifeMax2;
				}
				if (text == "") break;
				
				Color textcolor = Color.White;
				if (CONST_TEXT_COLORHP) textcolor = GetColorByRatio(1f*dp.statLife/dp.statLifeMax2);
				
				Vector2 measure = Main.fontMouseText.MeasureString(text)*(float)(ratio/2f+.5f);
				switch (CONST_TEXT_STYLE) {
					case "rotating": {
						bool b = (angle >= 0 && angle < 90) || (angle >= 270 && angle < 360);
						float textangle = b ? (float)(-angle) : (float)(-angle-180f);
						Vector2 textpos = pos+new Vector2(LdirX(dp.bodyFrame.Width*scale/100f*.8f,textangle)*(b ? -1 : 1),-LdirY(dp.bodyFrame.Width*scale/100f*.8f,textangle)*(b ? -1 : 1));
						DrawStringShadowed(sb,Main.fontMouseText,text,textpos,textcolor,Color.Black,measure/2,(float)(ratio/2f+.5f),textangle);
					} break;
					case "static": {
						Vector2 textpos = pos;
						switch (side) {
							case 0: textpos += new Vector2(-measure.X/2,dp.bodyFrame.Width*scale/100f*.4f); break;
							case 1: textpos += new Vector2(-measure.X/2,-dp.bodyFrame.Width*scale/100f*.4f-measure.Y); break;
							case 2: textpos += new Vector2(dp.bodyFrame.Width*scale/100f*.4f,-measure.Y/2); break;
							case 3: textpos += new Vector2(-dp.bodyFrame.Width*scale/100f*.4f-measure.X,-measure.Y/2); break;
						}
						DrawStringShadowed(sb,Main.fontMouseText,text,textpos,textcolor,Color.Black,default(Vector2),(float)(ratio/2f+.5f));
					} break;
					default: break;
				}
			}
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