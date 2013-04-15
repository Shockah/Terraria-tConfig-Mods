public class GuiPrefix : GuiCheat {
	#INCLUDE "PrefixDef.cs"
	
	public const int xo = 16, yo = 258;
	public const int ROWS = 10, COLS = 3, WW = 160, HH = 24;
	
	private static Texture2D swap;
	private static float swapScale;
	
	public static PrefixDef[] slots = new PrefixDef[ROWS*COLS];
	public static List<PrefixDef> allDefs = new List<PrefixDef>();
	public static List<PrefixDef> filtered = new List<PrefixDef>();
	public static int scroll = 0, toSetScroll = -1;
	
	public static bool blockChange = false, refresh = false;
	public static Dictionary<string,bool> standardCategories = new Dictionary<string,bool>();
	public static Dictionary<string,Texture2D> standardTextures = new Dictionary<string,Texture2D>();
	public static Dictionary<string,bool> modCategories = new Dictionary<string,bool>();
	public static Dictionary<string,Texture2D> modTextures = new Dictionary<string,Texture2D>();
	
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
		
		stateEnter = false;
		
		string[] tmpStandard = new string[]{"Positive","Negative"};
		foreach (string key in tmpStandard) standardCategories[key] = false;
		
		if (!Main.dedServ) {
			standardTextures["Positive"] = texBuffPos;
			standardTextures["Negative"] = texBuffNeg;
		}
		
		Dictionary<string,Texture2D> customTex = new Dictionary<string,Texture2D>();
		
		for (int i = 0; i < Prefix.prefixes.Count; i++) {
			Prefix prefix = Prefix.prefixes[i];
			allDefs.Add(new PrefixDef(i,prefix));
			
			string modpack = GetPrefixModpack(prefix);
			if (!modCategories.ContainsKey(modpack)) modCategories.Add(modpack,false);
		}
		
		if (!Main.dedServ) {
			modTextures[""] = Main.buffTexture[4];
			Codable.RunGlobalMethod("ModGeneric","ExternalFCMSetPrefixCategoryTexture",customTex);
			foreach (KeyValuePair<string,Texture2D> pair in customTex) {
				if (pair.Value == null) continue;
				modTextures[pair.Key] = pair.Value;
			}
		}
		
		ClearFilters();
	}
	public static string GetPrefixModpack(Prefix prefix) {
		if (prefix == null) return "";
		return prefix.modname;
	}
	
	public static void Create(bool preserveOldItems) {
		Create(0,preserveOldItems);
	}
	public static void Create(int scroll = 0, bool preserveOldItems = false) {
		if (Config.tileInterface == null || !(Config.tileInterface.code is GuiPrefix)) {
			ClearFilters();
			Filter();
			Sort();
		}
		
		int lines = (int)Math.Ceiling(1f*filtered.Count/ROWS);
		scroll = filtered.Count <= ROWS*COLS ? 0 : Math.Min(Math.Max(scroll,0),lines-COLS);
		
		Item preserve = Config.tileInterface != null && Config.tileInterface.code is GuiPrefix ? Config.tileInterface.itemSlots[0] : new Item();
		
		Config.tileInterface = new InterfaceObj(new GuiPrefix(),2,0);
		Player player = Main.player[Main.myPlayer];
		Config.tileInterface.SetLocation(new Vector2(player.position.X/16f,player.position.Y/16f));
		Main.playerInventory = true;
		GuiPrefix.scroll = scroll;
		
		Config.tileInterface.AddItemSlot(xo+COLS*WW+4,yo+ROWS*HH-48);
		Config.tileInterface.AddItemSlot(-100,-100);
		
		if (preserveOldItems) Config.tileInterface.itemSlots[0] = preserve;
		
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
		foreach (PrefixDef def in allDefs) {
			bool b = true;
			if (b) if (def == null || def.prefix == null) b = false;
			if (b) {
				if (lastSearch != null && lastSearch != "") {
					if (!def.prefix.affix.ToLower().Contains(lastSearch)) b = false;
				}
			}
			if (b) foreach (KeyValuePair<string,bool> pair in standardCategories) {
				if (b) if (!Filter(filterMode,pair.Value,def.categories[pair.Key])) b = false;
			}
			if (b) foreach (KeyValuePair<string,bool> pair in modCategories) {
				if (b) {
					string modpack = GetPrefixModpack(def.prefix);
					if (!Filter(filterMode,pair.Value,modpack == pair.Key)) b = false;
				}
			}
			if (b && Config.tileInterface != null && Config.tileInterface.code is GuiPrefix && !ModWorld.IsBlankItem(Config.tileInterface.itemSlots[0])) if (!def.prefix.Check(Config.tileInterface.itemSlots[0])) b = false;
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
			PrefixDef def = slots[i++];
			if (def == null) continue;
			
			bool enabled = false;
			if (!ModWorld.IsBlankItem(Config.tileInterface.itemSlots[0])) enabled = Config.tileInterface.itemSlots[0].prefix == def.prefixID;
			
			v = new Vector2(xo+x*WW,yo+y*HH);
			c = enabled || ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
			sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
			DrawStringShadowed(sb,Main.fontMouseText,def.prefix.affix,v+new Vector2(6,2),Color.White,Color.Black,default(Vector2),.75f);
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
		
		DrawStringShadowed(sb,Main.fontMouseText,"Prefixes matching filters: "+filtered.Count,new Vector2(xo,yo-26),Color.White,Color.Black);
		
		bool newStateEnter = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter);
		if (searchMode && !newStateEnter) {
			string oldSearch = search;
			search = Main.chatText;
			if (oldSearch != search) {
				lastSearch = search.ToLower();
				Filter();
				Create(true);
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
				Create(true);
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
		
		stateOld = state;
		state = Microsoft.Xna.Framework.Input.Mouse.GetState();
		refresh = false;
		
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
			PrefixDef def = slots[i++];
			if (def == null) continue;
			
			v = new Vector2(xo+x*WW,yo+y*HH);
			if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
				Main.player[Main.myPlayer].mouseInterface = true;
				
				if (def.prefix.weight > 0) {
					Item item = ModWorld.IsBlankItem(Config.tileInterface.itemSlots[0]) ? (def.showcaseItem == null ? null : def.showcaseItem.CloneItem()) : Config.tileInterface.itemSlots[0].CloneItem();
					if (item != null) {
						item.SetDefaults(item.name);
						if (def.prefixID > 0) item.Prefix(def.prefixID);
						ItemMouseText(item);
					}
				}
				
				if (Main.mouseLeft && Main.mouseLeftRelease && !blockChange) {
					int oldPrefix = Config.tileInterface.itemSlots[0].prefix;
					Item oldItem = Config.tileInterface.itemSlots[0].CloneItem();
					
					Config.tileInterface.itemSlots[0].SetDefaults(Config.tileInterface.itemSlots[0].name);
					if (def.prefixID > 0) Config.tileInterface.itemSlots[0].Prefix(def.prefixID);
					blockChange = true;
					refresh = false;
					
					Item newItem = Config.tileInterface.itemSlots[0].CloneItem();
					if (oldPrefix != def.prefixID) CheatNotification.Add(new CheatNotification("prefix",oldItem.AffixName()+" -> "+newItem.AffixName()));
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
			if (PostDrawFilter(sb,ModWorld.texNPCBlank2,modCategories[key],s+" prefixes",ref xx,ref yy)) modCategories[key] = !modCategories[key];
		}
		
		if (stateOld.HasValue && state.HasValue) {
			int mouseScrollDiff = (state.Value.ScrollWheelValue-stateOld.Value.ScrollWheelValue)/120;
			if (mouseScrollDiff != 0) {
				toSetScroll = toSetScroll == -1 ? scroll-mouseScrollDiff : toSetScroll-mouseScrollDiff;
				if (toSetScroll < 0) toSetScroll = 0;
			}
		}
		
		if (toSetScroll != -1) {
			Create(toSetScroll,true);
			toSetScroll = -1;
		}
		
		if (!oldBlockChange && blockChange && refresh) {
			Filter();
			Sort();
			Create(true);
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
	
	public override bool PretendChat() {
		return searchMode;
	}
	
	public bool PreDrawSlot(SpriteBatch sb, int slot) {
		if (slot == 0) {
			swap = Main.inventoryBack5Texture;
			swapScale = Main.inventoryScale;
		}
		
		Item item = Config.tileInterface.itemSlots[slot];
		if (ModWorld.IsBlankItem(item)) {
			Refill(slot);
		}
		
		Main.inventoryBack5Texture = slot == Config.tileInterface.itemSlots.Length-1 ? swap : Main.inventoryBack5Texture;
		Main.inventoryScale = slot == Config.tileInterface.itemSlots.Length-1 ? swapScale : .85f;
		return true;
	}
	
	public override bool CanPlaceSlot(int slot, Item item) {
		return true;
	}
	public override void PlaceSlot(int slot) {
		Filter();
		Sort();
		Create(true);
	}
	public override bool DropSlot(int slot) {
		return true;
	}
	
	public override void ButtonClicked(int num) {}
	
	public void SlotRightClicked(int slot) {
		Item[] chest = Config.tileInterface.itemSlots;
		if (Main.stackSplit <= 1 && Main.mouseRight && chest[slot].maxStack > 1 && (Main.mouseItem.IsTheSameAs(chest[slot]) || Main.mouseItem.type == 0) && (Main.mouseItem.stack < Main.mouseItem.maxStack || Main.mouseItem.type == 0)) {
			if (Main.mouseItem.type == 0) {
				Main.mouseItem = (Item)chest[slot].Clone();
				Main.mouseItem.stack = 0;
			}
			Main.mouseItem.stack++;
			chest[slot].stack--;
			if (chest[slot].stack <= 0) chest[slot] = new Item();
			Recipe.FindRecipes();
			Main.soundInstanceMenuTick.Stop();
			Main.soundInstanceMenuTick = Main.soundMenuTick.CreateInstance();
			Main.PlaySound(12,-1,-1,1);
			if (Main.stackSplit == 0) Main.stackSplit = 15;
			else Main.stackSplit = Main.stackDelay;
		}
	}
}