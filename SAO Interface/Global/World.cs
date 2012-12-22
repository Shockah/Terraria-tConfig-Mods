public const int
	INTERFACE_DEFAULT = 0,
	INTERFACE_SAO = 1,
	INTERFACE_SAO_TEXT = 2,
	INTERFACE_ALO = 3,
	INTERFACE_ALO_TEXT = 4;

private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};
private static Texture2D texBar, texBar2, texPBar, texBarBorder, texBarBorder2, texPBarBorder;

public static void Initialize() {
	switch (ModGeneric.InterfaceType) {
		case INTERFACE_SAO: case INTERFACE_SAO_TEXT: {
			texBar = Main.goreTexture[Config.goreID["SAOBar"]];
			texBar2 = Main.goreTexture[Config.goreID["SAOBar2"]];
			texBarBorder = Main.goreTexture[Config.goreID["SAOBarBorder"]];
			texBarBorder2 = Main.goreTexture[Config.goreID["SAOBarBorder2"]];
			
			texPBar = Main.goreTexture[Config.goreID["SAOPBar"]];
			texPBarBorder = Main.goreTexture[Config.goreID["SAOPBarBorder"]];
		} break;
		case INTERFACE_ALO: case INTERFACE_ALO_TEXT: {
			texBar = Main.goreTexture[Config.goreID["ALOBar"]];
			texBar2 = Main.goreTexture[Config.goreID["ALOBar2"]];
			texBarBorder = Main.goreTexture[Config.goreID["ALOBarBorder"]];
			
			texPBar = Main.goreTexture[Config.goreID["ALOPBar"]];
			texPBarBorder = Main.goreTexture[Config.goreID["ALOPBarBorder"]];
		} break;
		default: break;
	}
}

public static bool PreDrawLifeHearts(SpriteBatch sb) {
	Player p = Main.player[Main.myPlayer];
	if (p == null) return false;
	
	switch (ModGeneric.InterfaceType) {
		case INTERFACE_SAO: case INTERFACE_SAO_TEXT: {
			if (p.ghost) return false;
			if (Main.playerInventory) return false;
			
			int x = 16, y = 76;
			
			if (ModGeneric.InterfaceType == INTERFACE_SAO_TEXT) {
				string text = Lang.inter[0]+" "+p.statLife+"/"+p.statLifeMax;
				DrawStringShadowed(sb,Main.fontMouseText,text,new Vector2(x+texBarBorder.Width+4,y+2),Color.White,Color.Black,default(Vector2),.75f,SpriteEffects.None);
			}
			
			sb.Draw(p.statManaMax2 > 0 ? texBarBorder2 : texBarBorder,new Vector2(x,y),new Rectangle(0,0,texBarBorder.Width,texBarBorder.Height),Color.White);
			
			int times = (int)Math.Ceiling(texBar.Height/2f);
			float percent = p.statLifeMax == 0 ? 0f : 1f*p.statLife/p.statLifeMax;
			int w = (int)(Math.Floor(texBar.Width/2f*percent)*2);
			Color c = percent <= .2f ? Color.Red : (percent <= .5f ? Color.Yellow : Color.LawnGreen);
			for (int i = 0; i < times; i++) {
				int ww = w-(i/2)*2;
				if (ww > 0) sb.Draw(texBar,new Vector2(x+4,y+4+i*2),new Rectangle(0,i*2,ww,2),c);
			}
			
			if (p.team > 0) {
				int drawn = 0;
				bool infoText = p.accCompass > 0 || p.accDepthMeter > 0 || p.accWatch > 0;
				bool buffs = false;
				for (int i = 0; i < 10; i++) if (p.buffType[i] > 0 && p.buffTime[i] > 0) {
					buffs = true;
					break;
				}
				y += texBarBorder.Height+8+(infoText || buffs ? 2*48 : 0);
				
				for (int i = 0; i < Main.player.Length; i++) {
					if (i == Main.myPlayer) continue;
					Player p2 = Main.player[i];
					if (p2 == null) continue;
					if (!p2.active) continue;
					if (p2.ghost) continue;
					if (p2.team != p.team) continue;
					
					DrawStringShadowed(sb,Main.fontMouseText,p2.name,new Vector2(x,y),Color.White,Color.Black);
					y += 20;
					if (ModGeneric.InterfaceType == INTERFACE_SAO_TEXT) {
						string text = p2.statLife+"/"+p2.statLifeMax;
						DrawStringShadowed(sb,Main.fontMouseText,text,new Vector2(x+texPBarBorder.Width+4,y+2),Color.White,Color.Black,default(Vector2),.75f);
					}
					
					sb.Draw(texPBarBorder,new Vector2(x,y),new Rectangle(0,0,texPBarBorder.Width,texPBarBorder.Height),Color.White);
					
					times = (int)Math.Ceiling(texPBar.Height/2f);
					percent = p2.statLifeMax == 0 ? 0f : 1f*p2.statLife/p2.statLifeMax;
					w = (int)(Math.Floor(texPBar.Width/2f*percent)*2);
					c = percent <= .2f ? Color.Red : (percent <= .5f ? Color.Yellow : Color.LawnGreen);
					for (int j = 0; j < times; j++) {
						int ww = w-(j/2)*2;
						if (ww > 0) sb.Draw(texPBar,new Vector2(x+4,y+4+j*2),new Rectangle(0,j*2,ww,2),c);
					}
					
					drawn++;
					y += texPBarBorder.Height+4;
				}
			}
		} break;
		case INTERFACE_ALO: case INTERFACE_ALO_TEXT: {
			if (p.ghost) return false;
			if (Main.playerInventory) return false;
			
			int x = 16, y = 76;
			
			if (ModGeneric.InterfaceType == INTERFACE_ALO_TEXT) {
				string text = Lang.inter[0]+" "+p.statLife+"/"+p.statLifeMax;
				DrawStringShadowed(sb,Main.fontMouseText,text,new Vector2(x+texBarBorder.Width,y+8),Color.White,Color.Black,default(Vector2),.75f,SpriteEffects.None);
			}
			
			sb.Draw(texBarBorder,new Vector2(x,y),new Rectangle(0,0,texBarBorder.Width,texBarBorder.Height),Color.White);
			if (Codable.RunGlobalMethod("ModPlayer","GetYYYRace",new object[]{p.whoAmi})) {
				Texture2D raceIcon = Main.goreTexture[Config.goreID["RaceIcon"+((string)Codable.customMethodReturn)]];
				sb.Draw(raceIcon,new Vector2(x+texBarBorder.Height/2+18-raceIcon.Width/2,y+texBarBorder.Height/2-raceIcon.Height/2),new Rectangle(0,0,raceIcon.Width,raceIcon.Height),Color.White);
			} else DrawPVPIcon(sb,x+texBarBorder.Height/2,y+texBarBorder.Height/2);
			
			int times = (int)Math.Ceiling(texBar.Height/2f);
			float percent = p.statLifeMax == 0 ? 0f : 1f*p.statLife/p.statLifeMax;
			int w = (int)(Math.Floor(texBar.Width/2f*percent)*2);
			Color c = percent <= .2f ? Color.Red : (percent <= .5f ? Color.Yellow : Color.LawnGreen);
			for (int i = 0; i < times; i++) {
				int ww = w-6+((i+1)/2)*2;
				if (ww > 0) sb.Draw(texBar,new Vector2(x+76,y+12+i*2),new Rectangle(0,i*2,ww,2),c);
			}
			
			if (p.team > 0) {
				int drawn = 0;
				bool infoText = p.accCompass > 0 || p.accDepthMeter > 0 || p.accWatch > 0;
				bool buffs = false;
				for (int i = 0; i < 10; i++) if (p.buffType[i] > 0 && p.buffTime[i] > 0) {
					buffs = true;
					break;
				}
				y += texBarBorder.Height+8+(infoText || buffs ? 2*48 : 0);
				
				for (int i = 0; i < Main.player.Length; i++) {
					if (i == Main.myPlayer) continue;
					Player p2 = Main.player[i];
					if (p2 == null) continue;
					if (!p2.active) continue;
					if (p2.ghost) continue;
					if (p2.team != p.team) continue;
					
					DrawStringShadowed(sb,Main.fontMouseText,p2.name,new Vector2(x,y),Color.White,Color.Black);
					y += 20;
					if (ModGeneric.InterfaceType == INTERFACE_ALO_TEXT) {
						string text = p2.statLife+"/"+p2.statLifeMax;
						DrawStringShadowed(sb,Main.fontMouseText,text,new Vector2(x+texPBarBorder.Width+4,y+2),Color.White,Color.Black,default(Vector2),.75f);
					}
					
					sb.Draw(texPBarBorder,new Vector2(x,y),new Rectangle(0,0,texPBarBorder.Width,texPBarBorder.Height),Color.White);
					
					times = (int)Math.Ceiling(texPBar.Height/2f);
					percent = p2.statLifeMax == 0 ? 0f : 1f*p2.statLife/p2.statLifeMax;
					w = (int)(Math.Floor(texPBar.Width/2f*percent)*2);
					c = percent <= .2f ? Color.Red : (percent <= .5f ? Color.Yellow : Color.LawnGreen);
					for (int j = 0; j < times; j++) {
						int ww = w-(j/2)*2;
						if (ww > 0) sb.Draw(texPBar,new Vector2(x+4,y+4+j*2),new Rectangle(0,j*2,ww,2),c);
					}
					
					drawn++;
					y += texPBarBorder.Height+4;
				}
			}
		} break;
		default: return true;
	}
	
	return false;
}

public static bool PreDrawManaStars(SpriteBatch sb) {
	Player p = Main.player[Main.myPlayer];
	if (p == null) return false;
	
	switch (ModGeneric.InterfaceType) {
		case INTERFACE_SAO: case INTERFACE_SAO_TEXT: {
			if (p.ghost) return false;
			if (p.statManaMax2 <= 0) return false;
			if (Main.playerInventory) return false;
			
			int x = 16, y = 76;
			
			if (ModGeneric.InterfaceType == INTERFACE_SAO_TEXT) {
				string text = Lang.inter[2]+": "+p.statMana+"/"+p.statManaMax2;
				DrawStringShadowed(sb,Main.fontMouseText,text,new Vector2(x+texBarBorder.Width,y+16),Color.White,Color.Black,default(Vector2),.75f,SpriteEffects.None);
			}
			
			int times = (int)Math.Ceiling(texBar2.Height/2f);
			float percent = 1f*p.statMana/p.statManaMax2;
			int w = (int)(Math.Floor((texBar2.Width-2)/2f*percent)*2);
			for (int i = 0; i < times; i++) sb.Draw(texBar2,new Vector2(x+194-(i+1)/2*2,y+18+i*2),new Rectangle(4-(i+1)/2*2,i*2,w,2),Color.White);
		} break;
		case INTERFACE_ALO: case INTERFACE_ALO_TEXT: {
			if (p.ghost) return false;
			if (p.statManaMax2 <= 0) return false;
			if (Main.playerInventory) return false;
			
			int x = 16, y = 76;
			
			if (ModGeneric.InterfaceType == INTERFACE_ALO_TEXT) {
				string text = Lang.inter[2]+": "+p.statMana+"/"+p.statManaMax2;
				DrawStringShadowed(sb,Main.fontMouseText,text,new Vector2(x+texBarBorder.Width,y+24),Color.White,Color.Black,default(Vector2),.75f,SpriteEffects.None);
			}
			
			int times = (int)Math.Ceiling(texBar2.Height/2f);
			float percent = p.statManaMax2 == 0 ? 0f : 1f*p.statMana/p.statManaMax2;
			int w = (int)(Math.Floor(texBar2.Width/2f*percent)*2);
			Color c = new Color(0,204,255);
			for (int i = 0; i < times; i++) {
				int ww = w-(i/2)*2;
				if (ww > 0) sb.Draw(texBar2,new Vector2(x+76,y+26+i*2),new Rectangle(0,i*2,ww,2),c);
			}
		} break;
		default: return true;
	}
	
	return false;
}

public static bool PreDrawBuffsList(SpriteBatch sb) {
	Player p = Main.player[Main.myPlayer];
	if (p == null) return false;
	
	switch (ModGeneric.InterfaceType) {
		case INTERFACE_SAO: case INTERFACE_SAO_TEXT: case INTERFACE_ALO: case INTERFACE_ALO_TEXT: {
			if (p.ghost) return false;
			if (Main.playerInventory) return false;
			
			bool infoText = p.accCompass > 0 || p.accDepthMeter > 0 || p.accWatch > 0;
			DrawBuffsList(sb,30+(infoText ? 5*38 : 0),76+texBarBorder.Height+4);
		} break;
		default: return true;
	}
	
	return false;
}
public static bool PreDrawInformationTexts(SpriteBatch sb) {
	Player p = Main.player[Main.myPlayer];
	if (p == null) return false;
	
	switch (ModGeneric.InterfaceType) {
		case INTERFACE_SAO: case INTERFACE_SAO_TEXT: case INTERFACE_ALO: case INTERFACE_ALO_TEXT: {
			DrawInfoText(sb,22,76+texBarBorder.Height+4);
		} break;
		default: return true;
	}
	
	return false;
}
public static bool PreDrawLifeText(SpriteBatch sb) {
	Player p = Main.player[Main.myPlayer];
	if (p == null) return false;
	
	switch (ModGeneric.InterfaceType) {
		case INTERFACE_SAO: case INTERFACE_SAO_TEXT: {
			if (Main.playerInventory) return false;
			
			int x = 16, y = 76;
			if (Main.mouseX >= x+194 && Main.mouseY >= y+18 && Main.mouseX < x+194+texBar2.Width && Main.mouseY < y+18+texBar2.Height) DrawManaText(sb,p);
			else if (Main.mouseX >= x+4 && Main.mouseY >= y+4 && Main.mouseX < x+4+texBar.Width && Main.mouseY < y+4+texBar.Height) DrawLifeText(sb,p);
			else if (p.team > 0) {
				int drawn = 0;
				bool infoText = p.accCompass > 0 || p.accDepthMeter > 0 || p.accWatch > 0;
				bool buffs = false;
				for (int i = 0; i < 10; i++) if (p.buffType[i] > 0 && p.buffTime[i] > 0) {
					buffs = true;
					break;
				}
				y += texBarBorder.Height+8+(infoText || buffs ? 2*48 : 0);
				
				for (int i = 0; i < Main.player.Length; i++) {
					if (i == Main.myPlayer) continue;
					Player p2 = Main.player[i];
					if (p2 == null) continue;
					if (!p2.active) continue;
					if (p2.ghost) continue;
					if (p2.team != p.team) continue;
					
					y += 20;
					if (Main.mouseX >= x+4 && Main.mouseY >= y+4 && Main.mouseX < x+4+texPBar.Width && Main.mouseY < y+4+texPBar.Height) DrawLifeText(sb,p2);
					
					drawn++;
					y += texPBarBorder.Height+4;
				}
			}
		} break;
		case INTERFACE_ALO: case INTERFACE_ALO_TEXT: {
			if (Main.playerInventory) return false;
			
			int x = 16, y = 76;
			if (Main.mouseX >= x+76 && Main.mouseY >= y+12 && Main.mouseX < x+76+texBar.Width && Main.mouseY < y+12+texBar.Height) DrawLifeText(sb,p);
			else if (Main.mouseX >= x+76 && Main.mouseY >= y+26 && Main.mouseX < x+76+texBar2.Width && Main.mouseY < y+26+texBar2.Height) DrawManaText(sb,p);
			else if (p.team > 0) {
				int drawn = 0;
				bool infoText = p.accCompass > 0 || p.accDepthMeter > 0 || p.accWatch > 0;
				bool buffs = false;
				for (int i = 0; i < 10; i++) if (p.buffType[i] > 0 && p.buffTime[i] > 0) {
					buffs = true;
					break;
				}
				y += texBarBorder.Height+8+(infoText || buffs ? 2*48 : 0);
				
				for (int i = 0; i < Main.player.Length; i++) {
					if (i == Main.myPlayer) continue;
					Player p2 = Main.player[i];
					if (p2 == null) continue;
					if (!p2.active) continue;
					if (p2.ghost) continue;
					if (p2.team != p.team) continue;
					
					y += 20;
					if (Main.mouseX >= x+4 && Main.mouseY >= y+4 && Main.mouseX < x+4+texPBar.Width && Main.mouseY < y+4+texPBar.Height) DrawLifeText(sb,p2);
					
					drawn++;
					y += texPBarBorder.Height+4;
				}
			}
		} break;
		default: return true;
	}
	
	return false;
}
public static bool PreDrawManaText(SpriteBatch sb) {
	return false;
}

public static void DrawPVPIcon(SpriteBatch sb, int x, int y) {
	int num69 = x;
	int num70 = y-14;
	
	if (Main.player[Main.myPlayer].hostile) {
		SpriteBatch spriteBatch36 = sb;
		Texture2D texture5 = Main.itemTexture[4];
		Vector2 position36 = new Vector2((float)(num69 - 2), (float)num70);
		Rectangle? sourceRectangle5 = new Rectangle?(new Rectangle(0, 0, Main.itemTexture[4].Width, Main.itemTexture[4].Height));
		Color color14 = Main.teamColor[Main.player[Main.myPlayer].team];
		float rotation36 = 0f;
		Vector2 origin = default(Vector2);
		spriteBatch36.Draw(texture5, position36, sourceRectangle5, color14, rotation36, origin, 1f, SpriteEffects.None, 0f);
		SpriteBatch spriteBatch37 = sb;
		Texture2D texture6 = Main.itemTexture[4];
		Vector2 position37 = new Vector2((float)(num69 + 2), (float)num70);
		Rectangle? sourceRectangle6 = new Rectangle?(new Rectangle(0, 0, Main.itemTexture[4].Width, Main.itemTexture[4].Height));
		Color color15 = Main.teamColor[Main.player[Main.myPlayer].team];
		float rotation37 = 0f;
		origin = default(Vector2);
		spriteBatch37.Draw(texture6, position37, sourceRectangle6, color15, rotation37, origin, 1f, SpriteEffects.FlipHorizontally, 0f);
	} else {
		SpriteBatch spriteBatch38 = sb;
		Texture2D texture7 = Main.itemTexture[4];
		Vector2 position38 = new Vector2((float)(num69 - 16), (float)(num70 + 14));
		Rectangle? sourceRectangle7 = new Rectangle?(new Rectangle(0, 0, Main.itemTexture[4].Width, Main.itemTexture[4].Height));
		Color color16 = Main.teamColor[Main.player[Main.myPlayer].team];
		float rotation38 = -0.785f;
		Vector2 origin = default(Vector2);
		spriteBatch38.Draw(texture7, position38, sourceRectangle7, color16, rotation38, origin, 1f, SpriteEffects.None, 0f);
		SpriteBatch spriteBatch39 = sb;
		Texture2D texture8 = Main.itemTexture[4];
		Vector2 position39 = new Vector2((float)(num69 + 2), (float)(num70 + 14));
		Rectangle? sourceRectangle8 = new Rectangle?(new Rectangle(0, 0, Main.itemTexture[4].Width, Main.itemTexture[4].Height));
		Color color17 = Main.teamColor[Main.player[Main.myPlayer].team];
		float rotation39 = -0.785f;
		origin = default(Vector2);
		spriteBatch39.Draw(texture8, position39, sourceRectangle8, color17, rotation39, origin, 1f, SpriteEffects.None, 0f);
	}
	if (Main.mouseX > num69 && Main.mouseX < num69 + 34 && Main.mouseY > num70 - 2 && Main.mouseY < num70 + 34) {
		Main.player[Main.myPlayer].mouseInterface = true;
		if (Main.mouseLeft && Main.mouseLeftRelease && Main.teamCooldown == 0) {
			Main.teamCooldown = Main.teamCooldownLen;
			Main.PlaySound(12, -1, -1, 1);
			Main.player[Main.myPlayer].hostile = !Main.player[Main.myPlayer].hostile;
			NetMessage.SendData(30, -1, -1, "", Main.myPlayer, 0f, 0f, 0f, 0);
		}
	}
}
public static void DrawBuffsList(SpriteBatch sb, int x, int y) {
	if (Main.player[Main.myPlayer] == null) return;
	if (Main.player[Main.myPlayer].ghost) return;
	
	Main.buffString = "";
	int num63 = -1;
	for (int num64 = 0; num64 < 10; num64++) {
		if (Main.player[Main.myPlayer].buffType[num64] > 0) {
			int num65 = Main.player[Main.myPlayer].buffType[num64];
			int num66 = x + (num64%5) * 38;
			int num67 = y + (num64/5) * 48;
			Color color11 = new Color(Main.buffAlpha[num64], Main.buffAlpha[num64], Main.buffAlpha[num64], Main.buffAlpha[num64]);
			SpriteBatch spriteBatch34 = sb;
			Texture2D texture4 = Main.buffTexture[num65];
			Vector2 position34 = new Vector2(num66,num67);
			Rectangle? sourceRectangle4 = new Rectangle?(new Rectangle(0, 0, Main.buffTexture[num65].Width, Main.buffTexture[num65].Height));
			Color color12 = color11;
			float rotation34 = 0f;
			Vector2 origin = default(Vector2);
			spriteBatch34.Draw(texture4, position34, sourceRectangle4, color12, rotation34, origin, 1f, SpriteEffects.None, 0f);
			if (num65 != 28 && num65 != 34 && num65 != 37 && num65 != 38 && num65 != 40) {
				string text44 = "0 s";
				if (Main.player[Main.myPlayer].buffTime[num64] / 60 >= 60) text44 = System.Math.Round((double)(Main.player[Main.myPlayer].buffTime[num64] / 60) / 60.0) + " m";
				else text44 = System.Math.Round((double)Main.player[Main.myPlayer].buffTime[num64] / 60.0) + " s";
				SpriteBatch spriteBatch35 = sb;
				SpriteFont spriteFont31 = Main.fontItemStack;
				string text45 = text44;
				Vector2 position35 = new Vector2(num66,num67+Main.buffTexture[num65].Height);
				Color color13 = color11;
				float rotation35 = 0f;
				origin = default(Vector2);
				spriteBatch35.DrawString(spriteFont31, text45, position35, color13, rotation35, origin, 0.8f, SpriteEffects.None, 0f);
			}
			if (Main.mouseX < num66 + Main.buffTexture[num65].Width && Main.mouseY < num67 + Main.buffTexture[num65].Height && Main.mouseX > num66 && Main.mouseY > num67) {
				num63 = num64;
				ArrayHandler<float> arrayHandler;
				int i2;
				(arrayHandler = Main.buffAlpha)[i2 = num64] = arrayHandler[i2] + 0.1f;
				if (Main.mouseRight && Main.mouseRightRelease && !Main.debuff[num65]) {
					Main.PlaySound(12, -1, -1, 1);
					Main.player[Main.myPlayer].DelBuff(num64);
				}
			} else {
				ArrayHandler<float> arrayHandler;
				int i2;
				(arrayHandler = Main.buffAlpha)[i2 = num64] = arrayHandler[i2] - 0.05f;
			}
			if (Main.buffAlpha[num64] > 1f) Main.buffAlpha[num64] = 1f;
			else if ((double)Main.buffAlpha[num64] < 0.4) Main.buffAlpha[num64] = 0.4f;
		} else Main.buffAlpha[num64] = 0.4f;
	}
	if (num63 >= 0) {
		int num68 = Main.player[Main.myPlayer].buffType[num63];
		if (num68 > 0) {
			Main.buffString = Main.buffTip[num68];
			Config.mainInstance.MouseText(Main.buffName[num68], 0, 0);
		}
	}
}
public static void DrawInfoText(SpriteBatch sb, int x, int y) {
	Player p = Main.player[Main.myPlayer];
	List<string> lines = new List<string>();
	
	if (p.accCompass > 0) {
		double dist = (p.position.X+p.width/2d)/8d-Main.maxTilesX;
		if (Math.Round(dist) >= 1) lines.Add("Position: "+(int)(Math.Round(dist))+" feet east");
		else if (Math.Round(dist) <= 1) lines.Add("Position: "+(int)(Math.Round(-dist))+" feet west");
		else lines.Add("Position: center");
	}
	if (p.accDepthMeter > 0) {
		double dist = (p.position.Y+p.height)/8d-Main.worldSurface*2d;
		if (Math.Round(dist) >= 1) lines.Add("Depth: "+(int)(Math.Round(dist))+" feet below");
		else if (Math.Round(dist) <= 1) lines.Add("Depth: "+(int)(Math.Round(-dist))+" feet above");
		else lines.Add("Depth: Level");
	}
	if (p.accWatch > 0) {
		string text85 = "AM";
		double num173 = Main.time;
		if (!Main.dayTime) num173 += 54000.0;
		num173 = num173 / 86400.0 * 24.0;
		double num174 = 7.5;
		num173 = num173 - num174 - 12.0;
		if (num173 < 0.0) num173 += 24.0;
		if (num173 >= 12.0) text85 = "PM";
		int num175 = (int)num173;
		double num176 = num173 - (double)num175;
		num176 = (double)((int)(num176 * 60.0));
		string text86 = string.Concat(num176);
		if (num176 < 10.0) text86 = "0" + text86;
		if (num175 > 12) num175 -= 12;
		if (num175 == 0) num175 = 12;
		if (Main.player[Main.myPlayer].accWatch == 1) text86 = "00";
		else {
			if (Main.player[Main.myPlayer].accWatch == 2) {
				if (num176 < 30.0) text86 = "00";
				else text86 = "30";
			}
		}
		lines.Add(Lang.inter[34]+" "+num175+":"+text86+" "+text85);
	}
	
	for (int i = 0; i < lines.Count; i++) DrawStringShadowed(sb,Main.fontMouseText,lines[i],new Vector2(x,y+22*i),Color.White,Color.Black);
}
public static void DrawLifeText(SpriteBatch sb, Player p) {
	if (p.whoAmi == Main.myPlayer) p.showItemIcon = false;
	Config.mainInstance.MouseText(""+p.statLife+"/"+p.statLifeMax,0,0);
}
public static void DrawManaText(SpriteBatch sb, Player p) {
	if (p.statManaMax2 <= 0) return;
	if (p.whoAmi == Main.myPlayer) p.showItemIcon = false;
	Config.mainInstance.MouseText(""+p.statMana+"/"+p.statManaMax2,0,0);
}

public static Color Alpha(Color color, float alpha) {
	Color ret = new Color();
	ret.R = color.R;
	ret.G = color.G;
	ret.B = color.B;
	ret.A = (byte)(color.A*alpha);
	return ret;
}
public static Color Merge(Color c1, Color c2, float value, bool noAlpha) {
	value = Math.Min(Math.Max(value,0),1);
	float R = c1.R/255f-((c1.R/255f-c2.R/255f)*value);
	float G = c1.G/255f-((c1.G/255f-c2.G/255f)*value);
	float B = c1.B/255f-((c1.B/255f-c2.B/255f)*value);
	float A = noAlpha ? c1.A/255f : c1.A/255f-((c1.A/255f-c2.A/255f)*value);
	Color ret = new Color();
	ret.R = (byte)(R*255);
	ret.G = (byte)(G*255);
	ret.B = (byte)(B*255);
	ret.A = (byte)(A*255);
	return ret;
}

private static void DrawStringShadowed(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, Color colorShadow, Vector2 vec = default(Vector2), float scale = 1f, SpriteEffects effects = SpriteEffects.None) {
	foreach (Vector2 vecOff in shadowOffset) sb.DrawString(font,text,new Vector2(pos.X+vecOff.X,pos.Y+vecOff.Y),colorShadow,0f,vec,scale,effects,0f);
	sb.DrawString(font,text,pos,color,0f,vec,scale,effects,0f);
}