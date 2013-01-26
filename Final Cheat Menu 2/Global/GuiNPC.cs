public class GuiNPC : GuiCheat {
	#INCLUDE "NPCDef.cs"
	
	public const int xo = 16, yo = 258;
	public const int ROWS = 10, COLS = 3, WW = 160, HH = 24;
	
	public static NPCDef[] slots = new NPCDef[ROWS*COLS];
	public static List<NPCDef> allDefs = new List<NPCDef>();
	public static List<NPCDef> filtered = new List<NPCDef>();
	public static int scroll = 0, toSetScroll = -1;
	
	public static bool blockChange = false, refresh = false;
	public static Dictionary<string,bool> standardCategories = new Dictionary<string,bool>();
	public static Dictionary<string,Texture2D> standardTextures = new Dictionary<string,Texture2D>();
	public static Dictionary<string,bool> modCategories = new Dictionary<string,bool>();
	public static Dictionary<string,Texture2D> modTextures = new Dictionary<string,Texture2D>();
	public static Dictionary<string,int> modCount = new Dictionary<string,int>();
	
	public static Texture2D spawnCircle = null;
	public static NPCDef prepareSpawn = null;
	public static bool keepMouseInterface = false;
	public static Vector2 startSpawn = new Vector2(-1,-1);
	
	public static bool filterMode = true; //false - blacklist; true - whitelist
	public static bool searchMode = false;
	public static string search = "", lastSearch = "";
	
	public static bool stateEnter = false;
	
	public static void Init() {
		allDefs.Clear();
		standardCategories.Clear();
		standardTextures.Clear();
		modCategories.Clear();
		modTextures.Clear();
		modCount.Clear();
		
		stateEnter = false;
		
		string[] tmpStandard = new string[]{"Town","Friendly","Hostile","Boss"};
		foreach (string key in tmpStandard) standardCategories[key] = false;
		
		if (!Main.dedServ) {
			standardTextures["Town"] = Main.chatTexture;
			standardTextures["Friendly"] = Main.buffTexture[40];
			standardTextures["Hostile"] = Main.buffTexture[37];
			standardTextures["Boss"] = Main.npcTexture[68];
		}
		
		Dictionary<string,Texture2D> customTex = new Dictionary<string,Texture2D>();
		Dictionary<string,int> customCount = new Dictionary<string,int>();
		Dictionary<string,bool> tmpVanillaNPC = new Dictionary<string,bool>();
		
		foreach (KeyValuePair<string,NPC> pair in Config.npcDefs.byName) {
			if (pair.Value.type <= 0 || pair.Value.name == "") continue;
			allDefs.Add(new NPCDef(pair.Value));
			
			string modpack = GetNPCModpack(pair.Value);
			if (!modCategories.ContainsKey(modpack)) modCategories.Add(modpack,false);
			
			if (!Main.dedServ) {
				if (modpack == "") continue;
				bool vanilla = pair.Value.type <= 146;
				if (!tmpVanillaNPC.ContainsKey(modpack) || (tmpVanillaNPC[modpack] && !vanilla)) {
					Texture2D tex = pair.Value.townNPC ? Main.npcHeadTexture[NPC.TypeToNum(pair.Value.type)] : Main.npcTexture[pair.Value.type];
					modTextures[modpack] = tex;
					customTex[modpack] = tex;
					customCount[modpack] = pair.Value.townNPC ? 1 : Main.npcFrameCount[pair.Value.type];
					tmpVanillaNPC[modpack] = vanilla;
				}
			}
		}
		
		if (!Main.dedServ) {
			modTextures[""] = Main.npcHeadTexture[1];
		}
		
		if (!Main.dedServ) {
			Codable.RunGlobalMethod("ModGeneric","ExternalFCMSetNPCCategoryTexture",customTex,customCount);
			foreach (KeyValuePair<string,Texture2D> pair in customTex) {
				if (pair.Value == null) continue;
				int count = customCount[pair.Key];
				if (count <= 0) continue;
				
				modTextures[pair.Key] = pair.Value;
				modCount[pair.Key] = count;
			}
		}
		
		ClearFilters();
	}
	public static string GetNPCModpack(NPC npc) {
		if (npc.type <= 0 || npc.name == "") return "";
		if (Config.npcDefs.modName.ContainsKey(npc.name)) return Config.npcDefs.modName[npc.name];
		return "";
	}
	
	public static void Create(int scroll = 0) {
		prepareSpawn = null;
		keepMouseInterface = false;
		spawnCircle = null;
		startSpawn = new Vector2(-1,-1);
		
		if (Config.tileInterface == null || !(Config.tileInterface.code is GuiNPC)) {
			ClearFilters();
			Filter();
			Sort();
		}
		
		int lines = (int)Math.Ceiling(1f*filtered.Count/ROWS);
		scroll = filtered.Count <= ROWS*COLS ? 0 : Math.Min(Math.Max(scroll,0),lines-COLS);
		
		Config.tileInterface = new InterfaceObj(new GuiNPC(),0,0);
		Player player = Main.player[Main.myPlayer];
		Config.tileInterface.SetLocation(new Vector2(player.position.X/16f,player.position.Y/16f));
		Main.playerInventory = true;
		GuiNPC.scroll = scroll;
		
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
		foreach (NPCDef def in allDefs) {
			bool b = true;
			if (b) {
				if (lastSearch != null && lastSearch != "") {
					if (!def.npc.name.ToLower().Contains(lastSearch)) b = false;
				}
			}
			if (b) foreach (KeyValuePair<string,bool> pair in standardCategories) {
				if (b) if (!Filter(filterMode,pair.Value,def.categories[pair.Key])) b = false;
			}
			if (b) foreach (KeyValuePair<string,bool> pair in modCategories) {
				if (b) {
					string modpack = GetNPCModpack(def.npc);
					if (!Filter(filterMode,pair.Value,(pair.Key == "" && def.npc.type <= 146) || modpack == pair.Key)) b = false;
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
		Texture2D tex;
		Color c;
		Vector2 v;
		
		stateOld = state;
		state = Microsoft.Xna.Framework.Input.Mouse.GetState();
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
		
		if (startSpawn.X != -1 && startSpawn.Y != -1) {
			sb.End();
			sb.Begin(SpriteSortMode.Immediate,BlendState.Additive);
			
			Vector2 curPos = Main.screenPosition+new Vector2(Main.mouseX,Main.mouseY);
			float radius = (float)Math.Min(Math.Sqrt(Math.Pow(startSpawn.X-curPos.X,2)+Math.Pow(startSpawn.Y-curPos.Y,2)),200d*Math.Sqrt(10d/Math.PI));
			float field = (float)(Math.PI*Math.Pow(radius,2));
			int amount = (int)Math.Max(1,field/2000);
			if (spawnCircle == null || spawnCircle.Width != ((int)radius)*2+2) spawnCircle = CreateCircleTexture((int)radius);
			sb.Draw(spawnCircle,startSpawn-Main.screenPosition-new Vector2(spawnCircle.Width/2,spawnCircle.Height/2),ModWorld.GetTexRectangle(spawnCircle),Color.White,0f,default(Vector2),1f,SpriteEffects.None,0f);
			
			sb.End();
			sb.Begin();
			
			if (amount > 1) DrawStringShadowed(sb,Main.fontMouseText,""+amount,startSpawn-Main.screenPosition,Color.White,Color.Black,Main.fontMouseText.MeasureString(""+amount)/2);
		}
		
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
		tex = ModWorld.texNPCBlank;
		for (int x = 0; x < COLS; x++) for (int y = 0; y < ROWS; y++) {
			NPCDef def = slots[i++];
			if (def == null) continue;
			v = new Vector2(xo+x*WW,yo+y*HH);
			c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) || (prepareSpawn != null && prepareSpawn.npc.name == def.npc.name) ? Color.White : Color.Gray;
			sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
			DrawStringShadowed(sb,Main.fontMouseText,def.npc.name,v+new Vector2(6,4),Color.White,Color.Black,default(Vector2),.75f);
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
			PreDrawFilter(sb,ModWorld.texNPCBlank2,modTextures.ContainsKey(pair.Key) ? modTextures[pair.Key] : null,modCount.ContainsKey(pair.Key) ? modCount[pair.Key] : 1,pair.Value,ref xx,ref yy);
		}
		
		DrawStringShadowed(sb,Main.fontMouseText,"NPCs matching filters: "+filtered.Count,new Vector2(xo,yo-26),Color.White,Color.Black);
		PreDrawFilter(sb,texNPCBlank2,Main.npcTexture[53],3,new Vector2(xo+COLS*WW-88,yo-36),ModWorld.enabledNoSpawns);
		
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
		tex = ModWorld.texNPCBlank;
		for (int x = 0; x < COLS; x++) for (int y = 0; y < ROWS; y++) {
			NPCDef def = slots[i++];
			if (def == null) continue;
			v = new Vector2(xo+x*WW,yo+y*HH);
			if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
				Main.player[Main.myPlayer].mouseInterface = true;
				if (Main.mouseLeft && Main.mouseLeftRelease && !blockChange) {
					prepareSpawn = def;
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
		
		if (PostDrawFilter(sb,ModWorld.texNPCBlank2,new Vector2(xo+COLS*WW-88,yo-36),ModWorld.enabledNoSpawns,"Disable spawns: "+(ModWorld.enabledNoSpawns ? "On" : "Off"))) {
			ModWorld.enabledNoSpawns = !ModWorld.enabledNoSpawns;
			NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SWITCH_NOSPAWNS,-1,-1,(byte)Main.myPlayer);
			refresh = false;
			CheatNotification.Add(new CheatNotification("npc|spawns","NPC spawning "+(ModWorld.enabledNoSpawns ? "on" : "off")));
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
			if (PostDrawFilter(sb,ModWorld.texNPCBlank2,modCategories[key],s+" NPCs",ref xx,ref yy)) modCategories[key] = !modCategories[key];
		}
		
		if (!oldBlockChange && !blockChange && prepareSpawn != null && !Main.player[Main.myPlayer].mouseInterface && Main.mouseLeft && Main.mouseLeftRelease) {
			Main.player[Main.myPlayer].mouseInterface = true;
			keepMouseInterface = true;
			startSpawn = Main.screenPosition+new Vector2(Main.mouseX,Main.mouseY);
			return;
		}
		if (prepareSpawn != null) Main.player[Main.myPlayer].mouseInterface = true;
		if (keepMouseInterface) {
			Main.player[Main.myPlayer].mouseInterface = true;
			if (!Main.mouseLeft) {
				keepMouseInterface = false;
				
				Vector2 curPos = Main.screenPosition+new Vector2(Main.mouseX,Main.mouseY);
				float radius = (float)Math.Min(Math.Sqrt(Math.Pow(startSpawn.X-curPos.X,2)+Math.Pow(startSpawn.Y-curPos.Y,2)),200d*Math.Sqrt(10d/Math.PI));
				float field = (float)(Math.PI*Math.Pow(radius,2));
				int amount = (int)Math.Max(1,field/2000);
				
				if (Main.netMode == 0) {
					Random rnd = new Random();
					for (i = 0; i < amount; i++) {
						v = amount == 1 ? startSpawn : startSpawn+Util.Vector((float)(radius*rnd.NextDouble()),(float)(rnd.NextDouble()*360d));
						NPC npc = Main.npc[NPC.NewNPC((int)v.X,(int)v.Y,prepareSpawn.npc.name,0)];
						if (prepareSpawn.npc.netID < 0) npc.SetDefaults(prepareSpawn.npc.name);
					}
				} else {
					NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SPAWN_NPC,-1,-1,startSpawn.X,startSpawn.Y,radius,prepareSpawn.npc.name,prepareSpawn.npc.netID);
					CheatNotification.Add(new CheatNotification("npc|spawn","Spawning: "+amount+"x "+prepareSpawn.npc.name));
				}
				
				prepareSpawn = null;
				startSpawn = new Vector2(-1,-1);
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
		if (ret) blockChange = true;
		return ret;
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