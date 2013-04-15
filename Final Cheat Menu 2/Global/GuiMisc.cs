public class GuiMisc : GuiCheat {
	public const int xo = 16, yo = 210;
	
	public static string[] moonPhases = new string[]{"Full","Waning Gibbous","Last Quarter","Waning Crescent","New","Waxing Crescent","First Quarter","Waxing Gibbous"};
	public static bool blockChange = false;
	public static float clockDragAngle = -1;
	public static bool changedTime = false;
	
	public static void Init() {}
	
	public static void Create() {
		clockDragAngle = -1;
		
		Config.tileInterface = new InterfaceObj(new GuiMisc(),0,0);
		Player player = Main.player[Main.myPlayer];
		Config.tileInterface.SetLocation(new Vector2(player.position.X/16f,player.position.Y/16f));
		Main.playerInventory = true;
	}
	
	public static int GetAbsoluteTime() {
		int ret = (int)Main.time;
		if (!Main.dayTime) ret += 54000;
		ret -= 27000;
		while (ret < 0) ret += 86400;
		while (ret >= 86400) ret -= 86400;
		return ret;
	}
	public static List<int> GetTime(int time = -1) {
		if (time < 0) time = GetAbsoluteTime();
		
		int h = 0, m = 0, s = 0;
		s = time % 60; time /= 60;
		m = time % 60; time /= 60;
		h = time % 24;
		
		List<int> ret = new List<int>();
		ret.Add(h);
		ret.Add(m);
		ret.Add(s);
		return ret;
	}
	public static List<float> GetTimeAngles(int time = -1) {
		if (time < 0) time = GetAbsoluteTime();
		
		List<float> ret = new List<float>();
		ret.Add((time%(60*60*12))*360f/(60*60*12));
		ret.Add((time%(60*60))*360f/(60*60));
		return ret;
	}
	public static string GetTimeText(bool system24h, int time = -1) {
		List<int> l = GetTime(time);
		
		if (system24h) {
			string ret = "", s;
			
			s = ""+l[0];
			while (s.Length < 2) s = "0"+s;
			ret += s;
			
			s = ""+l[1];
			while (s.Length < 2) s = "0"+s;
			ret += ":"+s;
			
			return ret;
		} else {
			string ret = "", s;
			bool am = false;
			
			if (l[0] >= 12 && l[0] < 24) am = true;
			l[0] %= 12;
			if (l[0] == 0) l[0] = 12;
			
			s = ""+l[0];
			//while (s.Length < 2) s += "0";
			ret += s;
			
			s = ""+l[1];
			while (s.Length < 2) s = "0"+s;
			ret += ":"+s;
			
			ret += " "+(am ? "AM" : "PM");
			
			return ret;
		}
	}
	
	public override void PreDrawInterface(SpriteBatch sb) {
		Vector2 v;
		
		v = new Vector2(xo,yo)+new Vector2(texClock.Width/2,texClock.Height/2);
		if (clockDragAngle >= 0) {
			Main.player[Main.myPlayer].mouseInterface = true;
			float clockNewAngle = Util.Direction(v,new Vector2(Main.mouseX,Main.mouseY));
			float diff = BetterAngle(clockDragAngle,clockNewAngle);
			
			int timeDiff = (int)(86400/2/360f*diff);
			Main.time -= timeDiff;
			while (Main.time < 0 || Main.time > (Main.dayTime ? 54000 : 86400-54000)) {
				if (Main.time < 0) {
					Main.time += Main.dayTime ? 86400-54000 : 54000;
					if (Main.dayTime) Main.moonPhase--;
					if (Main.moonPhase < 0) Main.moonPhase += moonPhases.Length;
					Main.dayTime = !Main.dayTime;
				} else {
					Main.time -= Main.dayTime ? 54000 : 86400-54000;
					if (!Main.dayTime) {
						Main.moonPhase++;
						Main.bloodMoon = false;
					}
					if (Main.moonPhase >= moonPhases.Length) Main.moonPhase -= moonPhases.Length;
					Main.dayTime = !Main.dayTime;
				}
			}
			changedTime = true;
			clockDragAngle = blockChange ? clockNewAngle : -1;
		} else {
			if (changedTime) {
				NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SET_TIME,-1,-1,(int)Main.time,Main.dayTime,(byte)Main.moonPhase,Main.bloodMoon,Main.hardMode,Main.dayRate);
				CheatNotification.Add(new CheatNotification("time|time","Time: "+GetTimeText(false),30));
				changedTime = false;
			}
		}
		
		List<float> angles = GetTimeAngles();
		v = new Vector2(texClock.Width/2,texClock.Height/2);
		sb.Draw(texClock,new Vector2(xo,yo),GetTexRectangle(texClock),Color.White,0f,default(Vector2),1f,SpriteEffects.None,0f);
		sb.Draw(texClock3,new Vector2(xo,yo)+v,GetTexRectangle(texClock),Color.White,(float)((angles[1]*Math.PI)/180f),v,1f,SpriteEffects.None,0f);
		sb.Draw(texClock2,new Vector2(xo,yo)+v,GetTexRectangle(texClock),Color.White,(float)((angles[0]*Math.PI)/180f),v,1f,SpriteEffects.None,0f);
		
		string timeText = GetTimeText(false);
		Vector2 measure = Main.fontMouseText.MeasureString(timeText);
		DrawStringShadowed(sb,Main.fontMouseText,timeText,new Vector2(xo,yo)+v-measure/2,Color.White,Color.Black);
		
		v = new Vector2(xo+texClock.Width,yo);
		PreDrawFilter(sb,texNPCBlank2,Main.npcTexture[53],3,v+new Vector2(texNPCBlank2.Width*0,texNPCBlank2.Height*0),Main.bloodMoon);
		PreDrawFilter(sb,texNPCBlank2,Main.npcTexture[75],4,v+new Vector2(texNPCBlank2.Width*1,texNPCBlank2.Height*0),Main.hardMode);
		PreDrawFilter(sb,texNPCBlank2,Main.buffTexture[18],v+new Vector2(texNPCBlank2.Width*3,texNPCBlank2.Height*0),ModWorld.enabledNoclip[Main.myPlayer]);
		PreDrawFilter(sb,texNPCBlank2,Main.buffTexture[10],v+new Vector2(texNPCBlank2.Width*4,texNPCBlank2.Height*0),ModWorld.enabledGodmode[Main.myPlayer]);
		PreDrawFilter(sb,texNPCBlank2,Main.buffTexture[27],v+new Vector2(texNPCBlank2.Width*5,texNPCBlank2.Height*0),ModWorld.enabledAllLight);
		for (int i = 0; i < moonPhases.Length; i++) PreDrawFilter(sb,texNPCBlank2,Main.moonTexture,8,i,v+new Vector2(texNPCBlank2.Width*i,texNPCBlank2.Height*1.5f),Main.moonPhase == i);
		
		v += new Vector2(0,texNPCBlank2.Height*3);
		PreDrawFilter(sb,texALeft3,null,v+new Vector2(texALeft.Width*0,0),false);
		PreDrawFilter(sb,texALeft2,null,v+new Vector2(texALeft.Width*1,0),false);
		PreDrawFilter(sb,texALeft,null,v+new Vector2(texALeft.Width*2,0),false);
		
		string dayRateText = ""+Main.dayRate;
		measure = Main.fontMouseText.MeasureString(dayRateText);
		DrawStringShadowed(sb,Main.fontMouseText,dayRateText,v+new Vector2(texALeft.Width*4,0)+new Vector2(-measure.X/2f,2f),Color.White,Color.Black);
		
		PreDrawFilter(sb,texARight,null,v+new Vector2(texALeft.Width*5,0),false);
		PreDrawFilter(sb,texARight2,null,v+new Vector2(texALeft.Width*6,0),false);
		PreDrawFilter(sb,texARight3,null,v+new Vector2(texALeft.Width*7,0),false);
		
		v += new Vector2(0,texALeft.Height*2);
		PreDrawFilter(sb,texALeft3,null,v+new Vector2(texALeft.Width*0,0),false);
		PreDrawFilter(sb,texALeft2,null,v+new Vector2(texALeft.Width*1,0),false);
		PreDrawFilter(sb,texALeft,null,v+new Vector2(texALeft.Width*2,0),false);
		string hpText = ""+Main.player[Main.myPlayer].statLife;
		measure = Main.fontMouseText.MeasureString(hpText);
		DrawStringShadowed(sb,Main.fontMouseText,hpText,v+new Vector2(texALeft.Width*4,0)+new Vector2(-measure.X/2f,2f),Color.White,Color.Black);
		PreDrawFilter(sb,texARight,null,v+new Vector2(texALeft.Width*5,0),false);
		PreDrawFilter(sb,texARight2,null,v+new Vector2(texALeft.Width*6,0),false);
		PreDrawFilter(sb,texARight3,null,v+new Vector2(texALeft.Width*7,0),false);
		
		v += new Vector2(0,texALeft.Height);
		PreDrawFilter(sb,texALeft3,null,v+new Vector2(texALeft.Width*0,0),false);
		PreDrawFilter(sb,texALeft2,null,v+new Vector2(texALeft.Width*1,0),false);
		PreDrawFilter(sb,texALeft,null,v+new Vector2(texALeft.Width*2,0),false);
		hpText = ""+Main.player[Main.myPlayer].statLifeMax;
		measure = Main.fontMouseText.MeasureString(hpText);
		DrawStringShadowed(sb,Main.fontMouseText,hpText,v+new Vector2(texALeft.Width*4,0)+new Vector2(-measure.X/2f,2f),Color.White,Color.Black);
		PreDrawFilter(sb,texARight,null,v+new Vector2(texALeft.Width*5,0),false);
		PreDrawFilter(sb,texARight2,null,v+new Vector2(texALeft.Width*6,0),false);
		PreDrawFilter(sb,texARight3,null,v+new Vector2(texALeft.Width*7,0),false);
		
		v += new Vector2(0,texALeft.Height*1.5f);
		PreDrawFilter(sb,texALeft3,null,v+new Vector2(texALeft.Width*0,0),false);
		PreDrawFilter(sb,texALeft2,null,v+new Vector2(texALeft.Width*1,0),false);
		PreDrawFilter(sb,texALeft,null,v+new Vector2(texALeft.Width*2,0),false);
		string mpText = ""+Main.player[Main.myPlayer].statMana;
		measure = Main.fontMouseText.MeasureString(mpText);
		DrawStringShadowed(sb,Main.fontMouseText,mpText,v+new Vector2(texALeft.Width*4,0)+new Vector2(-measure.X/2f,2f),Color.White,Color.Black);
		PreDrawFilter(sb,texARight,null,v+new Vector2(texALeft.Width*5,0),false);
		PreDrawFilter(sb,texARight2,null,v+new Vector2(texALeft.Width*6,0),false);
		PreDrawFilter(sb,texARight3,null,v+new Vector2(texALeft.Width*7,0),false);
		
		v += new Vector2(0,texALeft.Height);
		PreDrawFilter(sb,texALeft3,null,v+new Vector2(texALeft.Width*0,0),false);
		PreDrawFilter(sb,texALeft2,null,v+new Vector2(texALeft.Width*1,0),false);
		PreDrawFilter(sb,texALeft,null,v+new Vector2(texALeft.Width*2,0),false);
		mpText = ""+Main.player[Main.myPlayer].statManaMax;
		measure = Main.fontMouseText.MeasureString(mpText);
		DrawStringShadowed(sb,Main.fontMouseText,mpText,v+new Vector2(texALeft.Width*4,0)+new Vector2(-measure.X/2f,2f),Color.White,Color.Black);
		PreDrawFilter(sb,texARight,null,v+new Vector2(texALeft.Width*5,0),false);
		PreDrawFilter(sb,texARight2,null,v+new Vector2(texALeft.Width*6,0),false);
		PreDrawFilter(sb,texARight3,null,v+new Vector2(texALeft.Width*7,0),false);
	}
	public void PreDrawFilter(SpriteBatch sb, Texture2D tex, Texture2D tex2, Vector2 v, bool value) {
		PreDrawFilter(sb,tex,tex2,1,0,v,value);
	}
	public void PreDrawFilter(SpriteBatch sb, Texture2D tex, Texture2D tex2, int frameCount, Vector2 v, bool value) {
		PreDrawFilter(sb,tex,tex2,frameCount,0,v,value);
	}
	public void PreDrawFilter(SpriteBatch sb, Texture2D tex, Texture2D tex2, int frameCount, int frame, Vector2 v, bool value) {
		Color c = value || ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		if (tex2 != null) {
			float scale = 1f;
			int ww = tex.Width-4, hh = tex.Height-4;
			if (tex2.Width*scale > ww) scale = 1f*ww/tex2.Width;
			if (tex2.Height/frameCount*scale > hh) scale = 1f*hh/(tex2.Height/frameCount);
			sb.Draw(tex2,v+new Vector2(2,2)+new Vector2((ww-tex2.Width*scale)/2f,(hh-tex2.Height/frameCount*scale)/2f),new Rectangle?(new Rectangle(0,tex2.Height/frameCount*frame,tex2.Width,tex2.Height/frameCount)),c,0f,default(Vector2),scale,SpriteEffects.None,0f);
		}
	}
	
	public float BetterAngle(float angle1, float angle2) {
		while (angle1 >= 360f) angle1 -= 360f;
		while (angle1 < 0f) angle1 += 360f;
		while (angle2 >= 360f) angle2 -= 360f;
		while (angle2 < 0f) angle2 += 360f;
		
		if (angle2 <= angle1+180f) {
			return angle1 > angle2+180f ? 360f-angle1+angle2 : angle2-angle1;
		} else return angle2-angle1-360f;
	}
	
	public override void PostDraw(SpriteBatch sb) {
		Vector2 v;
		
		if (!Main.mouseLeft) blockChange = false;
		if (clockDragAngle >= 0) Main.player[Main.myPlayer].mouseInterface = true;
		bool oldBlockChange = blockChange;
		
		stateOld = state;
		state = Microsoft.Xna.Framework.Input.Mouse.GetState();
		
		v = new Vector2(xo,yo)+new Vector2(texClock.Width/2,texClock.Height/2);
		if (ModWorld.MouseRegionCircle(v,texClock.Width/2)) {
			Main.player[Main.myPlayer].mouseInterface = true;
			if (stateOld.HasValue && state.HasValue) {
				int mouseScrollDiff = (state.Value.ScrollWheelValue-stateOld.Value.ScrollWheelValue)/120;
				
				Main.time -= 3600*mouseScrollDiff;
				while (Main.time < 0 || Main.time > (Main.dayTime ? 54000 : 86400-54000)) {
					if (Main.time < 0) {
						Main.time += Main.dayTime ? 86400-54000 : 54000;
						if (Main.dayTime) Main.moonPhase--;
						if (Main.moonPhase < 0) Main.moonPhase += moonPhases.Length;
						Main.dayTime = !Main.dayTime;
					} else {
						Main.time -= Main.dayTime ? 54000 : 86400-54000;
						if (!Main.dayTime) {
							Main.moonPhase++;
							Main.bloodMoon = false;
						}
						if (Main.moonPhase >= moonPhases.Length) Main.moonPhase -= moonPhases.Length;
						Main.dayTime = !Main.dayTime;
					}
				}
				if (mouseScrollDiff != 0) {
					NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SET_TIME,-1,-1,(int)Main.time,Main.dayTime,(byte)Main.moonPhase,Main.bloodMoon,Main.hardMode,Main.dayRate);
					CheatNotification.Add(new CheatNotification("time|time","Time: "+GetTimeText(false),30));
				}
			}
			if (Main.mouseLeft && Main.mouseLeftRelease && PressStuff()) clockDragAngle = Util.Direction(v,new Vector2(Main.mouseX,Main.mouseY));
		}
		
		bool b = false;
		v = new Vector2(xo+texClock.Width,yo);
		if (PostDrawFilter(sb,texNPCBlank2,v+new Vector2(texNPCBlank2.Width*0,texNPCBlank2.Height*0),Main.bloodMoon,"Blood Moon: "+(Main.bloodMoon ? "On" : "Off"))) {
			Main.bloodMoon = !Main.bloodMoon;
			CheatNotification.Add(new CheatNotification("time|bloodmoon","Blood Moon "+(Main.bloodMoon ? "on" : "off")));
			b = true;
		}
		if (PostDrawFilter(sb,texNPCBlank2,v+new Vector2(texNPCBlank2.Width*1,texNPCBlank2.Height*0),Main.hardMode,"Hardmode: "+(Main.hardMode ? "On" : "Off"))) {
			Main.hardMode = !Main.hardMode;
			CheatNotification.Add(new CheatNotification("time|hardmode","Hardmode "+(Main.hardMode ? "on" : "off")));
			b = true;
		}
		if (PostDrawFilter(sb,texNPCBlank2,v+new Vector2(texNPCBlank2.Width*3,texNPCBlank2.Height*0),ModWorld.enabledNoclip[Main.myPlayer],"Noclip: "+(ModWorld.enabledNoclip[Main.myPlayer] ? "On" : "Off"))) {
			ModWorld.enabledNoclip[Main.myPlayer] = !ModWorld.enabledNoclip[Main.myPlayer];
			CheatNotification.Add(new CheatNotification("misc|noclip","Noclip "+(ModWorld.enabledNoclip[Main.myPlayer] ? "on" : "off")));
			NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SWITCH_NOCLIP,-1,-1,(byte)Main.myPlayer);
		}
		if (PostDrawFilter(sb,texNPCBlank2,v+new Vector2(texNPCBlank2.Width*4,texNPCBlank2.Height*0),ModWorld.enabledGodmode[Main.myPlayer],"Godmode: "+(ModWorld.enabledGodmode[Main.myPlayer] ? "On" : "Off"))) {
			ModWorld.enabledGodmode[Main.myPlayer] = !ModWorld.enabledGodmode[Main.myPlayer];
			CheatNotification.Add(new CheatNotification("misc|godmode","Godmode "+(ModWorld.enabledGodmode[Main.myPlayer] ? "on" : "off")));
			NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SWITCH_GODMODE,-1,-1,(byte)Main.myPlayer);
		}
		if (PostDrawFilter(sb,texNPCBlank2,v+new Vector2(texNPCBlank2.Width*5,texNPCBlank2.Height*0),ModWorld.enabledAllLight,"Lighting Up: "+(ModWorld.enabledAllLight ? "On" : "Off"))) {
			ModWorld.enabledAllLight = !ModWorld.enabledAllLight;
			CheatNotification.Add(new CheatNotification("misc|alllight","Lighting Up "+(ModWorld.enabledAllLight ? "on" : "off")));
		}
		for (int i = 0; i < moonPhases.Length; i++) if (PostDrawFilter(sb,texNPCBlank2,v+new Vector2(texNPCBlank2.Width*i,texNPCBlank2.Height*1.5f),Main.moonPhase == i,moonPhases[i]+" Moon")) {
			Main.moonPhase = i;
			CheatNotification.Add(new CheatNotification("time|moon",moonPhases[i]+" Moon"));
			b = true;
		}
		
		if (b) NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SET_TIME,-1,-1,(int)Main.time,Main.dayTime,(byte)Main.moonPhase,Main.bloodMoon,Main.hardMode,Main.dayRate);
		
		if (ModWorld.MouseRegion(v+new Vector2(0,texNPCBlank2.Height*1.5f),new Vector2(texNPCBlank2.Width*moonPhases.Length,texNPCBlank2.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			if (stateOld.HasValue && state.HasValue) {
				int mouseScrollDiff = (state.Value.ScrollWheelValue-stateOld.Value.ScrollWheelValue)/120;
				Main.moonPhase -= mouseScrollDiff;
				while (Main.moonPhase < 0) Main.moonPhase += moonPhases.Length;
				while (Main.moonPhase >= moonPhases.Length) Main.moonPhase -= moonPhases.Length;
				if (mouseScrollDiff != 0) {
					NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SET_TIME,-1,-1,(int)Main.time,Main.dayTime,(byte)Main.moonPhase,Main.bloodMoon,Main.hardMode,Main.dayRate);
					CheatNotification.Add(new CheatNotification("time|moon",moonPhases[Main.moonPhase]+" Moon",30));
				}
			}
		}
		
		Player p = Main.player[Main.myPlayer];
		int oldI, max;
		
		v += new Vector2(0,texNPCBlank2.Height*3);
		b = false;
		oldI = Main.dayRate;
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*0,texALeft.Height*0),false,"-25 day rate")) {
			Main.dayRate -= 25;
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*1,texALeft.Height*0),false,"-5 day rate")) {
			Main.dayRate -= 5;
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*2,texALeft.Height*0),false,"-1 day rate")) {
			Main.dayRate--;
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*5,texALeft.Height*0),false,"+1 day rate")) {
			Main.dayRate++;
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*6,texALeft.Height*0),false,"+5 day rate")) {
			Main.dayRate += 5;
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*7,texALeft.Height*0),false,"+25 day rate")) {
			Main.dayRate += 25;
			b = true;
		}
		if (b) CheatNotification.Add(new CheatNotification("time|rate","Day rate: "+Main.dayRate,30));
		if (ModWorld.MouseRegion(v,new Vector2(texALeft.Width*8,texALeft.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			if (stateOld.HasValue && state.HasValue) {
				int mouseScrollDiff = (state.Value.ScrollWheelValue-stateOld.Value.ScrollWheelValue)/120;
				Main.dayRate -= mouseScrollDiff;
				if (mouseScrollDiff != 0) CheatNotification.Add(new CheatNotification("time|rate","Day rate: "+Main.dayRate,30));
			}
		}
		if (oldI != Main.dayRate) NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SET_TIME,-1,-1,(int)Main.time,Main.dayTime,(byte)Main.moonPhase,Main.bloodMoon,Main.hardMode,Main.dayRate);
		
		v += new Vector2(0,texALeft.Height*2);
		b = false;
		oldI = p.statLife;
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*0,texALeft.Height*0),false,"1 HP")) {
			p.statLife = 1;
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*1,texALeft.Height*0),false,"-20 HP")) {
			p.statLife = Math.Max(p.statLife-20,1);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*2,texALeft.Height*0),false,"-1 HP")) {
			p.statLife = Math.Max(p.statLife-1,1);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*5,texALeft.Height*0),false,"+1 HP")) {
			p.statLife = Math.Min(p.statLife+1,p.statLifeMax2);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*6,texALeft.Height*0),false,"+20 HP")) {
			p.statLife = Math.Min(p.statLife+20,p.statLifeMax2);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*7,texALeft.Height*0),false,""+p.statLifeMax2+" HP")) {
			p.statLife = p.statLifeMax2;
			b = true;
		}
		if (b) CheatNotification.Add(new CheatNotification("hp|current","HP: "+p.statLife,30));
		if (ModWorld.MouseRegion(v,new Vector2(texALeft.Width*8,texALeft.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			if (stateOld.HasValue && state.HasValue) {
				int mouseScrollDiff = (state.Value.ScrollWheelValue-stateOld.Value.ScrollWheelValue)/120;
				p.statLife = Math.Min(Math.Max(p.statLife-mouseScrollDiff*20,1),p.statLifeMax2);
				if (mouseScrollDiff != 0) CheatNotification.Add(new CheatNotification("hp|current","HP: "+p.statLife,30));
			}
		}
		if (oldI != p.statLife) NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_STATS,-1,-1,(byte)p.whoAmi,p.statLife,p.statLifeMax,p.statMana,p.statManaMax);
		
		v += new Vector2(0,texALeft.Height);
		b = false;
		oldI = p.statLifeMax;
		max = Codable.RunGlobalMethod("ModWorld","ExternalGetMaxHealth") ? (int)Codable.customMethodReturn : 400;
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*0,texALeft.Height*0),false,"20 max HP")) {
			p.statLifeMax = 20;
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*1,texALeft.Height*0),false,"-20 max HP")) {
			p.statLifeMax = Math.Max(p.statLifeMax-20,20);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*2,texALeft.Height*0),false,"-1 max HP")) {
			p.statLifeMax = Math.Max(p.statLifeMax-1,20);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*5,texALeft.Height*0),false,"+1 max HP")) {
			p.statLifeMax = Math.Min(p.statLifeMax+1,max);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*6,texALeft.Height*0),false,"+20 max HP")) {
			p.statLifeMax = Math.Min(p.statLifeMax+20,max);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*7,texALeft.Height*0),false,""+max+" max HP")) {
			p.statLifeMax = max;
			b = true;
		}
		if (b) CheatNotification.Add(new CheatNotification("hp|max","HP Max: "+p.statLifeMax,30));
		if (ModWorld.MouseRegion(v,new Vector2(texALeft.Width*8,texALeft.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			if (stateOld.HasValue && state.HasValue) {
				int mouseScrollDiff = (state.Value.ScrollWheelValue-stateOld.Value.ScrollWheelValue)/120;
				p.statLifeMax = Math.Min(Math.Max(p.statLifeMax-mouseScrollDiff*20,20),max);
				if (mouseScrollDiff != 0) CheatNotification.Add(new CheatNotification("hp|max","HP Max: "+p.statLifeMax,30));
			}
		}
		if (oldI != p.statLifeMax) NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_STATS,-1,-1,(byte)p.whoAmi,p.statLife,p.statLifeMax,p.statMana,p.statManaMax);
		
		v += new Vector2(0,texALeft.Height*1.5f);
		b = false;
		oldI = p.statMana;
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*0,texALeft.Height*0),false,"0 MP")) {
			p.statMana = 0;
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*1,texALeft.Height*0),false,"-20 MP")) {
			p.statMana = Math.Max(p.statMana-20,0);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*2,texALeft.Height*0),false,"-1 MP")) {
			p.statMana = Math.Max(p.statMana-1,0);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*5,texALeft.Height*0),false,"+1 MP")) {
			p.statMana = Math.Min(p.statMana+1,p.statManaMax2);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*6,texALeft.Height*0),false,"+20 MP")) {
			p.statMana = Math.Min(p.statMana+20,p.statManaMax2);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*7,texALeft.Height*0),false,""+p.statManaMax2+" MP")) {
			p.statMana = p.statManaMax2;
			b = true;
		}
		if (b) CheatNotification.Add(new CheatNotification("mp|current","MP: "+p.statMana,30));
		if (ModWorld.MouseRegion(v,new Vector2(texALeft.Width*8,texALeft.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			if (stateOld.HasValue && state.HasValue) {
				int mouseScrollDiff = (state.Value.ScrollWheelValue-stateOld.Value.ScrollWheelValue)/120;
				p.statMana = Math.Min(Math.Max(p.statMana-mouseScrollDiff*20,0),p.statManaMax2);
				if (mouseScrollDiff != 0) CheatNotification.Add(new CheatNotification("mp|current","MP: "+p.statMana,30));
			}
		}
		if (oldI != p.statMana) NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_STATS,-1,-1,(byte)p.whoAmi,p.statLife,p.statLifeMax,p.statMana,p.statManaMax);
		
		v += new Vector2(0,texALeft.Height);
		b = false;
		oldI = p.statManaMax;
		max = Codable.RunGlobalMethod("ModWorld","ExternalGetMaxMana") ? (int)Codable.customMethodReturn : 200;
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*0,texALeft.Height*0),false,"0 max MP")) {
			p.statManaMax = 20;
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*1,texALeft.Height*0),false,"-20 max MP")) {
			p.statManaMax = Math.Max(p.statManaMax-20,0);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*2,texALeft.Height*0),false,"-1 max MP")) {
			p.statManaMax = Math.Max(p.statManaMax-1,0);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*5,texALeft.Height*0),false,"+1 max MP")) {
			p.statManaMax = Math.Min(p.statManaMax+1,max);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*6,texALeft.Height*0),false,"+20 max MP")) {
			p.statManaMax = Math.Min(p.statManaMax+20,max);
			b = true;
		}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*7,texALeft.Height*0),false,""+max+" max MP")) {
			p.statManaMax = max;
			b = true;
		}
		if (b) CheatNotification.Add(new CheatNotification("mp|max","MP Max: "+p.statManaMax,30));
		if (ModWorld.MouseRegion(v,new Vector2(texALeft.Width*8,texALeft.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			if (stateOld.HasValue && state.HasValue) {
				int mouseScrollDiff = (state.Value.ScrollWheelValue-stateOld.Value.ScrollWheelValue)/120;
				p.statManaMax = Math.Min(Math.Max(p.statManaMax-mouseScrollDiff*20,0),max);
				if (mouseScrollDiff != 0) CheatNotification.Add(new CheatNotification("mp|max","MP Max: "+p.statManaMax,30));
			}
		}
		if (oldI != p.statManaMax) NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_STATS,-1,-1,(byte)p.whoAmi,p.statLife,p.statLifeMax,p.statMana,p.statManaMax);
	}
	public bool PostDrawFilter(SpriteBatch sb, Texture2D tex, Vector2 v, bool value, string text) {
		bool ret = false;
		
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText(text);
			if (Main.mouseLeft && Main.mouseLeftRelease) ret = true;
		}
		
		if (blockChange) return false;
		if (ret) blockChange = true;
		return ret;
	}
	
	public bool PressStuff() {
		if (blockChange) return false;
		blockChange = true;
		return true;
	}
	
	public override bool CanPlaceSlot(int slot, Item item) {
		return true;
	}
	public override void PlaceSlot(int slot) {}
	public override bool DropSlot(int slot) {
		return false;
	}
	
	public override void ButtonClicked(int num) {}
}