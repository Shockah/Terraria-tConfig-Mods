public class GuiBuff : GuiCheat {
	#INCLUDE "BuffDef.cs"
	
	public const int xo = 16, yo = 258;
	public const int ROWS = 6, COLS = 3, WW = 160, HH = 40;
	
	public static BuffDef[] slots = new BuffDef[ROWS*COLS];
	public static List<BuffDef> allDefs = new List<BuffDef>();
	public static List<BuffDef> filtered = new List<BuffDef>();
	public static int scroll = 0, toSetScroll = -1;
	
	public static bool blockChange = false, refresh = false, alreadyScrolling = false;
	public static Dictionary<string,bool> standardCategories = new Dictionary<string,bool>();
	public static Dictionary<string,Texture2D> standardTextures = new Dictionary<string,Texture2D>();
	public static Dictionary<string,bool> modCategories = new Dictionary<string,bool>();
	public static Dictionary<string,Texture2D> modTextures = new Dictionary<string,Texture2D>();
	
	public static bool filterMode = true; //false - blacklist; true - whitelist
	public static bool searchMode = false;
	public static string search = "", lastSearch = "";
	
	public static bool stateEnter = false;
	
	public static int setBuffTime;
	
	public static void Init() {
		allDefs.Clear();
		standardCategories.Clear();
		standardTextures.Clear();
		modCategories.Clear();
		modTextures.Clear();
		
		stateEnter = false;
		setBuffTime = 60*60*5;
		
		string[] tmpStandard = new string[]{"Buff","Debuff","No timer"};
		foreach (string key in tmpStandard) standardCategories[key] = false;
		
		if (!Main.dedServ) {
			standardTextures["Buff"] = texBuffPos;
			standardTextures["Debuff"] = texBuffNeg;
			standardTextures["No timer"] = Main.buffTexture[34];
		}
		
		Dictionary<string,Texture2D> customTex = new Dictionary<string,Texture2D>();
		Dictionary<string,bool> tmpVanillaBuff = new Dictionary<string,bool>();
		
		for (int i = 1; i <= 40; i++) {
			KeyValuePair<string,int> pair = new KeyValuePair<string,int>(Main.buffName[i],i);
			if (pair.Value < 0) continue;
			allDefs.Add(new BuffDef(pair.Value));
			
			string modpack = GetBuffModpack(pair.Value);
			if (!modCategories.ContainsKey(modpack)) modCategories.Add(modpack,false);
			
			if (!Main.dedServ) {
				if (modpack == "") continue;
				bool vanilla = pair.Value <= 40;
				if (!tmpVanillaBuff.ContainsKey(modpack) || (tmpVanillaBuff[modpack] && !vanilla)) {
					Texture2D tex = Main.buffTexture[pair.Value];
					modTextures[modpack] = tex;
					customTex[modpack] = tex;
					tmpVanillaBuff[modpack] = vanilla;
				}
			}
		}
		foreach (KeyValuePair<string,int> pair in Config.buffDefs.ID.dict) {
			if (pair.Value < 0) continue;
			allDefs.Add(new BuffDef(pair.Value));
			
			string modpack = GetBuffModpack(pair.Value);
			if (!modCategories.ContainsKey(modpack)) modCategories.Add(modpack,false);
			
			if (!Main.dedServ) {
				if (modpack == "") continue;
				bool vanilla = pair.Value <= 40;
				if (!tmpVanillaBuff.ContainsKey(modpack) || (tmpVanillaBuff[modpack] && !vanilla)) {
					Texture2D tex = Main.buffTexture[pair.Value];
					modTextures[modpack] = tex;
					customTex[modpack] = tex;
					tmpVanillaBuff[modpack] = vanilla;
				}
			}
		}
		
		if (!Main.dedServ) {
			modTextures[""] = Main.buffTexture[4];
		}
		
		if (!Main.dedServ) {
			Codable.RunGlobalMethod("ModGeneric","ExternalFCMSetBuffCategoryTexture",customTex);
			foreach (KeyValuePair<string,Texture2D> pair in customTex) {
				if (pair.Value == null) continue;
				modTextures[pair.Key] = pair.Value;
			}
		}
		
		ClearFilters();
	}
	public static string GetBuffModpack(int buffID) {
		if (buffID < 0) return "";
		if (Config.buffDefs.modName.ContainsKey(buffID)) return Config.buffDefs.modName[buffID];
		return "";
	}
	
	public static void Create(int scroll = 0) {
		if (Config.tileInterface == null || !(Config.tileInterface.code is GuiBuff)) {
			ClearFilters();
			Filter();
			Sort();
		}
		
		int lines = (int)Math.Ceiling(1f*filtered.Count/ROWS);
		scroll = filtered.Count <= ROWS*COLS ? 0 : Math.Min(Math.Max(scroll,0),lines-COLS);
		
		Config.tileInterface = new InterfaceObj(new GuiBuff(),0,0);
		Player player = Main.player[Main.myPlayer];
		Config.tileInterface.SetLocation(new Vector2(player.position.X/16f,player.position.Y/16f));
		Main.playerInventory = true;
		GuiBuff.scroll = scroll;
		
		int i = 0;
		for (int x = 0; x < COLS; x++) for (int y = 0; y < ROWS; y++) Refill(i++);
	}
	public static void Refill(int slot) {
		try {
			slots[slot] = filtered[slot+scroll*ROWS];
		} catch (Exception) {
			slots[slot] = null;
		}
	}
	public static void ClearFilters() {
		List<string> keys = new List<string>();
		
		keys.Clear();
		foreach (KeyValuePair<string,bool> pair in standardCategories) keys.Add(pair.Key);
		foreach (string key in keys) standardCategories[key] = false;
		
		keys.Clear();
		foreach (KeyValuePair<string,bool> pair in modCategories) keys.Add(pair.Key);
		foreach (string key in keys) modCategories[key] = false;
		
		filterMode = true;
		
		searchMode = false;
		search = "";
		lastSearch = "";
	}
	public static void Filter() {
		filtered.Clear();
		foreach (BuffDef def in allDefs) {
			bool b = true;
			if (b) {
				if (lastSearch != null && lastSearch != "") {
					if (!Main.buffName[def.buffID].ToLower().Contains(lastSearch)) b = false;
				}
			}
			if (b) foreach (KeyValuePair<string,bool> pair in standardCategories) {
				if (b) if (!Filter(filterMode,pair.Value,def.categories[pair.Key])) b = false;
			}
			if (b) foreach (KeyValuePair<string,bool> pair in modCategories) {
				if (b) {
					string modpack = GetBuffModpack(def.buffID);
					if (!Filter(filterMode,pair.Value,(pair.Key == "" && def.buffID <= 40) || modpack == pair.Key)) b = false;
				}
			}
			if (b) filtered.Add(def);
		}
	}
	public static bool Filter(bool mode, bool filter, bool value) {
		return filter ? value == mode : true;
	}
	public static void Sort() {
		
	}
	
	public override void PreDrawInterface(SpriteBatch sb) {
		Texture2D tex, tex2;
		Color c;
		Vector2 v, measure;
		
		tex = ModWorld.texALeft3;
		v = new Vector2(xo,yo+ROWS*HH);
		c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		
		tex = ModWorld.texALeft2;
		v = new Vector2(xo+26,yo+ROWS*HH);
		c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		
		tex = ModWorld.texALeft;
		v = new Vector2(xo+52,yo+ROWS*HH);
		c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		
		tex = ModWorld.texARight3;
		v = new Vector2(xo+COLS*WW-24,yo+ROWS*HH);
		c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		
		tex = ModWorld.texARight2;
		v = new Vector2(xo+COLS*WW-50,yo+ROWS*HH);
		c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		
		tex = ModWorld.texARight;
		v = new Vector2(xo+COLS*WW-76,yo+ROWS*HH);
		c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		
		int lines = (int)Math.Ceiling(1f*filtered.Count/ROWS);
		int scrollMax = filtered.Count <= ROWS*COLS ? 0 : lines;
		if (scrollMax > 0) {
			float fScroll = 1f*scroll/lines;
			float fScrollW = 1f*COLS/lines;
			float fScrollPX = COLS*WW-164;
			sb.Draw(whiteTex,new Rectangle(xo+80,yo+ROWS*HH+2,(int)(fScrollPX+4),20),Color.Silver);
			sb.Draw(whiteTex,new Rectangle((int)(xo+82+fScroll*fScrollPX),yo+ROWS*HH+4,(int)(fScrollW*fScrollPX),16),Color.Black);
		}
		
		int i = 0;
		tex = ModWorld.texBuffBlank;
		for (int x = 0; x < COLS; x++) for (int y = 0; y < ROWS; y++) {
			BuffDef def = slots[i++];
			if (def == null) continue;
			
			int enabledOn = -1;
			for (int j = 0; j < Main.player[Main.myPlayer].buffType.Length; j++) {
				if (Main.player[Main.myPlayer].buffType[j] == def.buffID && Main.player[Main.myPlayer].buffTime[j] > 0) {
					enabledOn = j;
					break;
				}
			}
			
			v = new Vector2(xo+x*WW,yo+y*HH);
			c = enabledOn != -1 || ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
			sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
			
			tex2 = Main.buffTexture[def.buffID];
			float scale = 1f;
			int ww = 32, hh = 32;
			if (tex2.Width*scale > ww) scale = 1f*ww/tex2.Width;
			if (tex2.Height*scale > hh) scale = 1f*hh/tex2.Height;
			sb.Draw(tex2,v+new Vector2(4,4)+new Vector2((ww-tex2.Width*scale)/2f,(hh-tex2.Height*scale)/2f),ModWorld.GetTexRectangle(tex2),c,0f,default(Vector2),scale,SpriteEffects.None,0f);
			
			if (enabledOn != -1 && !Main.buffDontDisplayTime[def.buffID]) {
				string timeText = "0 s";
				if (Main.player[Main.myPlayer].buffTime[enabledOn]/60 >= 60) timeText = System.Math.Round((double)(Main.player[Main.myPlayer].buffTime[enabledOn]/60)/60.0)+" m";
				else timeText = System.Math.Round((double)Main.player[Main.myPlayer].buffTime[enabledOn]/60.0)+" s";
				
				DrawStringShadowed(sb,Main.fontMouseText,Main.buffName[def.buffID],v+new Vector2(40,4),Color.White,Color.Black,default(Vector2),.75f);
				DrawStringShadowed(sb,Main.fontMouseText,timeText,v+new Vector2(40,20),Color.White,Color.Black,default(Vector2),.75f);
			} else DrawStringShadowed(sb,Main.fontMouseText,Main.buffName[def.buffID],v+new Vector2(40,12),Color.White,Color.Black,default(Vector2),.75f);
		}
		
		float xx = 0, yy = 0;
		
		PreDrawFilter(sb,ModWorld.texNPCBlank2,null,1,filterMode,ref xx,ref yy);
		xx += 1;
		PreDrawFilter(sb,ModWorld.texNPCBlank2,Main.cdTexture,1,false,ref xx,ref yy);
		
		xx = 0; yy = 1.5f;
		foreach (KeyValuePair<string,bool> pair in standardCategories) {
			PreDrawFilter(sb,ModWorld.texNPCBlank2,standardTextures.ContainsKey(pair.Key) ? standardTextures[pair.Key] : null,1,pair.Value,ref xx,ref yy);
		}
		
		if (xx == 0) {
			yy += .5f;
		} else {
			xx = 0;
			yy += 1.5f;
		}
		
		foreach (KeyValuePair<string,bool> pair in modCategories) {
			PreDrawFilter(sb,ModWorld.texNPCBlank2,modTextures.ContainsKey(pair.Key) ? modTextures[pair.Key] : null,1,pair.Value,ref xx,ref yy);
		}
		
		DrawStringShadowed(sb,Main.fontMouseText,"Buffs matching filters: "+filtered.Count,new Vector2(xo,yo-26),Color.White,Color.Black);
		
		v = new Vector2(xo+COLS*WW-56-8*texALeft.Width,yo-28);
		PreDrawFilter(sb,texALeft3,null,v+new Vector2(texALeft.Width*0,0),false);
		PreDrawFilter(sb,texALeft2,null,v+new Vector2(texALeft.Width*1,0),false);
		PreDrawFilter(sb,texALeft,null,v+new Vector2(texALeft.Width*2,0),false);
		
		int timerS = (setBuffTime/60)%60;
		int timerM = setBuffTime/60/60;
		string timerText = ""+timerM+":"+(timerS < 10 ? "0" : "")+timerS;
		measure = Main.fontMouseText.MeasureString(timerText);
		DrawStringShadowed(sb,Main.fontMouseText,timerText,v+new Vector2(texALeft.Width*4,0)+new Vector2(-measure.X/2f,2f),Color.White,Color.Black);
		
		PreDrawFilter(sb,texARight,null,v+new Vector2(texALeft.Width*5,0),false);
		PreDrawFilter(sb,texARight2,null,v+new Vector2(texALeft.Width*6,0),false);
		PreDrawFilter(sb,texARight3,null,v+new Vector2(texALeft.Width*7,0),false);
		
		bool newStateEnter = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter);
		if (searchMode && !newStateEnter) {
			string oldSearch = search;
			search = Main.chatText;
			if (oldSearch != search) {
				lastSearch = search.ToLower();
				Filter();
				Create();
			}
		}
		if ((newStateEnter && !stateEnter) && !Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) && !Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt)) {
			searchMode = !searchMode;
			if (searchMode) {
				Main.clrInput();
				Main.chatText = search = "";
			} else {
				searchMode = false;
				lastSearch = search.ToLower();
				Filter();
				Create();
			}
		}
		if ((search != null && search != "") || searchMode) DrawStringShadowed(sb,Main.fontMouseText,"Search: "+search+(searchMode ? "|" : ""),new Vector2(xo,yo+ROWS*HH+26),Color.White,Color.Black);
		stateEnter = newStateEnter;
	}
	public void PreDrawFilter(SpriteBatch sb, Texture2D tex, Texture2D tex2, Vector2 v, bool value) {
		PreDrawFilter(sb,tex,tex2,1,v,value);
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
	public void PreDrawFilter(SpriteBatch sb, Texture2D tex, Texture2D tex2, int frameCount, bool value, ref float xx, ref float yy, float xxx = 0, float yyy = 0) {
		Vector2 v = new Vector2(xo+COLS*WW+4+(xx+xxx)*tex.Width,yo+(yy+yyy)*tex.Height);
		Color c = value || ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		if (tex2 != null) {
			float scale = 1f;
			int ww = tex.Width-4, hh = tex.Height-4;
			if (tex2.Width*scale > ww) scale = 1f*ww/tex2.Width;
			if (tex2.Height/frameCount*scale > hh) scale = 1f*hh/(tex2.Height/frameCount);
			sb.Draw(tex2,v+new Vector2(2,2)+new Vector2((ww-tex2.Width*scale)/2f,(hh-tex2.Height/frameCount*scale)/2f),new Rectangle?(new Rectangle(0,0,tex2.Width,tex2.Height/frameCount)),c,0f,default(Vector2),scale,SpriteEffects.None,0f);
		}
		
		xx += 1;
		if (xx >= 3) {
			xx -= 3;
			yy += 1;
		}
	}
	
	public override void PostDraw(SpriteBatch sb) {
		Texture2D tex;
		Vector2 v;
		tex = ModWorld.texAUp2;
		alreadyScrolling = false;
		
		stateOld = state;
		state = Microsoft.Xna.Framework.Input.Mouse.GetState();
		
		v = new Vector2(xo,yo+ROWS*HH);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Scroll to the beginning");
			if (Main.mouseLeft && Main.mouseLeftRelease) toSetScroll = 0;
		}
		
		v = new Vector2(xo+26,yo+ROWS*HH);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Previous page");
			if (Main.mouseLeft && Main.mouseLeftRelease) toSetScroll = Math.Max(scroll-COLS,0);
		}
		
		v = new Vector2(xo+52,yo+ROWS*HH);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Previous column");
			if (Main.mouseLeft && Main.mouseLeftRelease) toSetScroll = Math.Max(scroll-1,0);
		}
		
		v = new Vector2(xo+COLS*WW-24,yo+ROWS*HH);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Scroll to the end");
			if (Main.mouseLeft && Main.mouseLeftRelease) toSetScroll = (int)Math.Ceiling(1f*filtered.Count/COLS)-COLS;
		}
		
		v = new Vector2(xo+COLS*WW-50,yo+ROWS*HH);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Next page");
			if (Main.mouseLeft && Main.mouseLeftRelease) toSetScroll = scroll+COLS;
		}
		
		v = new Vector2(xo+COLS*WW-76,yo+ROWS*HH);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Next column");
			if (Main.mouseLeft && Main.mouseLeftRelease) toSetScroll = scroll+1;
		}
		
		float xx = 0, yy = 0;
		if (!Main.mouseLeft) blockChange = false;
		bool oldBlockChange = blockChange;
		List<string> keys = new List<string>();
		
		int i = 0;
		tex = ModWorld.texBuffBlank;
		for (int x = 0; x < COLS; x++) for (int y = 0; y < ROWS; y++) {
			BuffDef def = slots[i++];
			if (def == null) continue;
			
			int enabledOn = -1;
			for (int j = 0; j < Main.player[Main.myPlayer].buffType.Length; j++) {
				if (Main.player[Main.myPlayer].buffType[j] == def.buffID && Main.player[Main.myPlayer].buffTime[j] > 0) {
					enabledOn = j;
					break;
				}
			}
			
			v = new Vector2(xo+x*WW,yo+y*HH);
			if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
				Main.player[Main.myPlayer].mouseInterface = true;
				if (Main.mouseLeft && Main.mouseLeftRelease && !blockChange) {
					Main.player[Main.myPlayer].AddBuff(def.buffID,setBuffTime);
					blockChange = true;
					refresh = false;
					
					string timeText = "0 s";
					if (setBuffTime >= 60) timeText = System.Math.Round((double)(setBuffTime/60)/60.0)+" m";
					else timeText = System.Math.Round((double)setBuffTime/60.0)+" s";
					CheatNotification.Add(new CheatNotification("buff|on","Activated buff: "+Main.buffName[def.buffID]+" | Time: "+timeText));
				}
				if (Main.mouseRight && Main.mouseRightRelease && !blockChange) {
					DelBuff(def.buffID);
					blockChange = true;
					refresh = false;
					
					CheatNotification.Add(new CheatNotification("buff|off","Deactivated buff: "+Main.buffName[def.buffID]));
				}
			}
		}
		
		if (PostDrawFilter(sb,ModWorld.texNPCBlank2,filterMode,(filterMode ? "Whitelist" : "Blacklist")+" mode",ref xx,ref yy)) filterMode = !filterMode;
		xx += 1;
		if (PostDrawFilter(sb,ModWorld.texNPCBlank2,false,"Clear",ref xx,ref yy)) ClearFilters();
		
		xx = 0; yy = 1.5f;
		keys.Clear();
		foreach (KeyValuePair<string,bool> pair in standardCategories) keys.Add(pair.Key);
		foreach (string key in keys) {
			if (PostDrawFilter(sb,ModWorld.texNPCBlank2,standardCategories[key],key,ref xx,ref yy)) standardCategories[key] = !standardCategories[key];
		}
		
		if (xx == 0) {
			yy += .5f;
		} else {
			xx = 0;
			yy += 1.5f;
		}
		
		keys.Clear();
		foreach (KeyValuePair<string,bool> pair in modCategories) keys.Add(pair.Key);
		foreach (string key in keys) {
			string s = key == "" ? "Vanilla" : key;
			if (PostDrawFilter(sb,ModWorld.texNPCBlank2,modCategories[key],s+" buffs",ref xx,ref yy)) modCategories[key] = !modCategories[key];
		}
		
		v = new Vector2(xo+COLS*WW-56-8*texALeft.Width,yo-28);
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*0,texALeft.Height*0),false,"-5 minutes")) {setBuffTime -= 60*60*5; refresh = false;}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*1,texALeft.Height*0),false,"-30 seconds")) {setBuffTime -= 60*30; refresh = false;}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*2,texALeft.Height*0),false,"-1 second")) {setBuffTime -= 60; refresh = false;}
		
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*5,texALeft.Height*0),false,"+1 second")) {setBuffTime += 60; refresh = false;}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*6,texALeft.Height*0),false,"+30 seconds")) {setBuffTime += 60*30; refresh = false;}
		if (PostDrawFilter(sb,texALeft,v+new Vector2(texALeft.Width*7,texALeft.Height*0),false,"+5 minutes")) {setBuffTime += 60*60*5; refresh = false;}
		
		if (ModWorld.MouseRegion(v,new Vector2(texALeft.Width*8,texALeft.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			if (stateOld.HasValue && state.HasValue) {
				int mouseScrollDiff = (state.Value.ScrollWheelValue-stateOld.Value.ScrollWheelValue)/120;
				setBuffTime -= mouseScrollDiff*60*60;
				alreadyScrolling = true;
			}
		}
		setBuffTime = Math.Min(Math.Max(setBuffTime,0),60*60*60);
		
		if (!alreadyScrolling) {
			if (stateOld.HasValue && state.HasValue) {
				int mouseScrollDiff = (state.Value.ScrollWheelValue-stateOld.Value.ScrollWheelValue)/120;
				if (mouseScrollDiff != 0) {
					toSetScroll = toSetScroll == -1 ? scroll-mouseScrollDiff : toSetScroll-mouseScrollDiff;
					if (toSetScroll < 0) toSetScroll = 0;
				}
			}
			
			if (toSetScroll != -1) {
				Create(toSetScroll);
				toSetScroll = -1;
			}
		}
		
		if (!oldBlockChange && blockChange && refresh) {
			Filter();
			Sort();
			Create();
			refresh = false;
		}
	}
	public bool PostDrawFilter(SpriteBatch sb, Texture2D tex, bool value, string text, ref float xx, ref float yy, float xxx = 0, float yyy = 0) {
		bool ret = false;
		
		Vector2 v = new Vector2(xo+COLS*WW+4+(xx+xxx)*tex.Width,yo+(yy+yyy)*tex.Height);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText(text);
			if (Main.mouseLeft && Main.mouseLeftRelease) ret = true;
		}
		
		xx += 1;
		if (xx >= 3) {
			xx -= 3;
			yy += 1;
		}
		
		if (blockChange) return false;
		if (ret) {
			blockChange = true;
			refresh = true;
		}
		return ret;
	}
	public bool PostDrawFilter(SpriteBatch sb, Texture2D tex, Vector2 v, bool value, string text) {
		bool ret = false;
		
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText(text);
			if (Main.mouseLeft && Main.mouseLeftRelease) ret = true;
		}
		
		if (blockChange) return false;
		if (ret) {
			blockChange = true;
			refresh = true;
		}
		return ret;
	}
	
	public void DelBuff(int buffID) {
		for (int i = 0; i < Main.player[Main.myPlayer].buffType.Length; i++) {
			if (Main.player[Main.myPlayer].buffType[i] == buffID && Main.player[Main.myPlayer].buffTime[i] > 0) {
				Main.player[Main.myPlayer].DelBuff(i);
				return;
			}
		}
	}
	public override bool PretendChat() {
		return searchMode;
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