public class GuiItem : GuiCheat {
	#INCLUDE "ItemDef.cs"
	
	public const int xo = 16, yo = 210;
	public const int ROWS = 5, COLS = 5;
	
	private static Texture2D swap;
	private static float swapScale;
	
	public static List<ItemDef> allDefs = new List<ItemDef>();
	public static List<ItemDef> filtered = new List<ItemDef>();
	public static int scroll = 0, toSetScroll = -1;
	
	public static bool blockChange = false;
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
		
		string[] tmpStandard = new string[]{"Melee","Ranged","Magic","Head","Torso","Legs","Vanity","Ammo","Accessory","Consumable","Tile","Wall","Material","Other"};
		foreach (string key in tmpStandard) standardCategories[key] = false;
		
		if (!Main.dedServ) {
			standardTextures["Melee"] = Main.itemTexture[Config.itemDefs.byName["Excalibur"].type];
			standardTextures["Ranged"] = Main.itemTexture[Config.itemDefs.byName["Megashark"].type];
			standardTextures["Magic"] = Main.itemTexture[Config.itemDefs.byName["Rainbow Rod"].type];
			standardTextures["Ammo"] = Main.itemTexture[Config.itemDefs.byName["Cursed Arrow"].type];
			standardTextures["Head"] = Main.itemTexture[Config.itemDefs.byName["Hallowed Headgear"].type];
			standardTextures["Torso"] = Main.itemTexture[Config.itemDefs.byName["Hallowed Plate Mail"].type];
			standardTextures["Legs"] = Main.itemTexture[Config.itemDefs.byName["Hallowed Greaves"].type];
			standardTextures["Vanity"] = Main.itemTexture[Config.itemDefs.byName["Santa Hat"].type];
			standardTextures["Accessory"] = Main.itemTexture[Config.itemDefs.byName["Spectre Boots"].type];
			standardTextures["Consumable"] = Main.itemTexture[Config.itemDefs.byName["Greater Healing Potion"].type];
			standardTextures["Tile"] = Main.itemTexture[Config.itemDefs.byName["Stone Block"].type];
			standardTextures["Wall"] = Main.itemTexture[Config.itemDefs.byName["Wood Wall"].type];
			standardTextures["Material"] = Main.itemTexture[Config.itemDefs.byName["Hellstone Bar"].type];
			standardTextures["Other"] = Main.itemTexture[Config.itemDefs.byName["Mechanical Eye"].type];
		}
		
		Dictionary<string,Texture2D> customTex = new Dictionary<string,Texture2D>();
		Dictionary<string,bool> tmpVanillaItem = new Dictionary<string,bool>();
		
		foreach (KeyValuePair<string,Item> pair in Config.itemDefs.byName) {
			if (ModWorld.IsBlankItem(pair.Value)) continue;
			allDefs.Add(new ItemDef(pair.Value));
			
			string modpack = GetItemModpack(pair.Value);
			if (!modCategories.ContainsKey(modpack)) modCategories.Add(modpack,false);
			
			if (!Main.dedServ) {
				if (modpack == "") continue;
				bool vanilla = pair.Value.type <= 603;
				if (!tmpVanillaItem.ContainsKey(modpack) || (tmpVanillaItem[modpack] && !vanilla)) {
					Texture2D tex = Main.itemTexture[pair.Value.type];
					modTextures[modpack] = tex;
					customTex[modpack] = tex;
					tmpVanillaItem[modpack] = vanilla;
				}
			}
		}
		
		string[] extraItems = new string[]{
			"Gold Pickaxe","Silver Pickaxe","Copper Pickaxe",
			"Gold Broadsword","Silver Broadsword","Copper Broadsword",
			"Gold Shortsword","Silver Shortsword","Copper Shortsword",
			"Gold Hammer","Silver Hammer","Copper Hammer",
			"Gold Axe","Silver Axe","Copper Axe",
			"Gold Bow","Silver Bow","Copper Bow",
			"Blue Phasesaber","Red Phasesaber","Green Phasesaber","Purple Phasesaber","White Phasesaber","Yellow Phasesaber"
		};
		foreach (string str in extraItems) {
			Item item = new Item();
			item.SetDefaults(str);
			Main.NewText(item.name);
			if (ModWorld.IsBlankItem(item)) continue;
			allDefs.Add(new ItemDef(item));
			
			string modpack = "";
			if (!modCategories.ContainsKey(modpack)) modCategories.Add(modpack,false);
		}
		
		if (!Main.dedServ) {
			modTextures[""] = Main.npcHeadTexture[1];
		}
		
		if (!Main.dedServ) {
			Codable.RunGlobalMethod("ModGeneric","ExternalFCMSetCategoryTexture",customTex);
			foreach (KeyValuePair<string,Texture2D> pair in customTex) {
				if (pair.Value == null) continue;
				modTextures[pair.Key] = pair.Value;
			}
		}
		
		ClearFilters();
	}
	public static string GetItemModpack(Item item) {
		if (ModWorld.IsBlankItem(item)) return "";
		if (Config.itemDefs.modName.ContainsKey(item.name)) return Config.itemDefs.modName[item.name];
		return "";
	}
	
	public static void Create(int scroll = 0) {
		if (Config.tileInterface == null || !(Config.tileInterface.code is GuiItem)) {
			ClearFilters();
			Filter();
			Sort();
		}
		
		if (filtered.Count <= ROWS*COLS) scroll = 0;
		else {
			int lines = (int)Math.Ceiling(1f*filtered.Count/ROWS);
			scroll = Math.Min(Math.Max(scroll,0),lines-COLS);
		}
		
		Config.tileInterface = new InterfaceObj(new GuiItem(),ROWS*COLS+1,0);
		Player player = Main.player[Main.myPlayer];
		Config.tileInterface.SetLocation(new Vector2(player.position.X/16f,player.position.Y/16f));
		Main.playerInventory = true;
		GuiItem.scroll = scroll;
		
		int i = 0;
		for (int y = 0; y < ROWS; y++) {
			for (int x = 0; x < COLS; x++) {
				Config.tileInterface.AddItemSlot((int)(xo+(x*44)),(int)(yo+(y*44)));
				Refill(i++);
			}
		}
		Config.tileInterface.AddItemSlot(-100,-100);
	}
	public static void Refill(int slot) {
		Item item = new Item();
		try {
			item = filtered[slot+scroll*ROWS].item.CloneItem();
			item.stack = item.maxStack;
		} catch (Exception) {}
		if (ModWorld.IsBlankItem(item)) item = new Item();
		Config.tileInterface.itemSlots[slot] = item;
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
		foreach (ItemDef def in allDefs) {
			bool b = true;
			if (b) {
				if (lastSearch != null && lastSearch != "") {
					if (!def.item.name.ToLower().Contains(lastSearch)) b = false;
				}
			}
			if (b) foreach (KeyValuePair<string,bool> pair in standardCategories) {
				if (b) if (!Filter(filterMode,pair.Value,def.categories[pair.Key])) b = false;
			}
			if (b) foreach (KeyValuePair<string,bool> pair in modCategories) {
				if (b) {
					string modpack = GetItemModpack(def.item);
					if (!Filter(filterMode,pair.Value,(pair.Key == "" && def.item.type <= 603) || modpack == pair.Key)) b = false;
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
		
		tex = ModWorld.texAUp3;
		v = new Vector2(xo+COLS*44,yo);
		c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		
		tex = ModWorld.texAUp2;
		v = new Vector2(xo+COLS*44,yo+26);
		c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		
		tex = ModWorld.texAUp;
		v = new Vector2(xo+COLS*44,yo+52);
		c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		
		tex = ModWorld.texADown3;
		v = new Vector2(xo+COLS*44,yo+ROWS*44-26);
		c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		
		tex = ModWorld.texADown2;
		v = new Vector2(xo+COLS*44,yo+ROWS*44-52);
		c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		
		tex = ModWorld.texADown;
		v = new Vector2(xo+COLS*44,yo+ROWS*44-78);
		c = ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		
		int lines = (int)Math.Ceiling(1f*filtered.Count/COLS);
		int scrollMax = filtered.Count <= ROWS*COLS ? 0 : lines-ROWS;
		if (scrollMax > 0) {
			float fScroll = 1f*scroll/lines;
			float fScrollH = 1f*ROWS/lines;
			float fScrollPX = ROWS*44-164;
			sb.Draw(whiteTex,new Rectangle(xo+COLS*44+2,yo+80,20,(int)(fScrollPX+4)),Color.Silver);
			sb.Draw(whiteTex,new Rectangle(xo+COLS*44+4,(int)(yo+82+(fScroll*fScrollPX)),16,(int)Math.Max(fScrollH*fScrollPX,1)),Color.Black);
		}
		
		float xx = 0, yy = 0;
		
		PreDrawFilter(sb,ModWorld.texItemBlank,null,filterMode,ref xx,ref yy);
		xx += 1;
		PreDrawFilter(sb,ModWorld.texItemBlank,Main.cdTexture,false,ref xx,ref yy);
		
		xx = 0; yy = 1.5f;
		foreach (KeyValuePair<string,bool> pair in standardCategories) {
			PreDrawFilter(sb,ModWorld.texItemBlank,standardTextures.ContainsKey(pair.Key) ? standardTextures[pair.Key] : null,pair.Value,ref xx,ref yy);
		}
		
		if (xx == 0) {
			yy += .5f;
		} else {
			xx = 0;
			yy += 1.5f;
		}
		
		foreach (KeyValuePair<string,bool> pair in modCategories) {
			PreDrawFilter(sb,ModWorld.texItemBlank,modTextures.ContainsKey(pair.Key) ? modTextures[pair.Key] : null,pair.Value,ref xx,ref yy);
		}
		
		DrawStringShadowed(sb,Main.fontMouseText,"Items matching filters: "+filtered.Count,new Vector2(xo,yo+ROWS*44),Color.White,Color.Black);
		
		bool newStateEnter = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter);
		if (searchMode && !newStateEnter) {
			string oldSearch = search;
			search = Main.chatText;
			if (oldSearch != search) {
				lastSearch = search.ToLower();
				Filter();
				Sort();
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
				Sort();
				Create();
			}
		}
		if ((search != null && search != "") || searchMode) DrawStringShadowed(sb,Main.fontMouseText,"Search: "+search+(searchMode ? "|" : ""),new Vector2(xo,yo+ROWS*44+20),Color.White,Color.Black);
		stateEnter = newStateEnter;
	}
	public void PreDrawFilter(SpriteBatch sb, Texture2D tex, Texture2D tex2, bool value, ref float xx, ref float yy, float xxx = 0, float yyy = 0) {
		Vector2 v = new Vector2(xo+COLS*44+28+(xx+xxx)*tex.Width,yo+(yy+yyy)*tex.Height);
		Color c = value || ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height)) ? Color.White : Color.Gray;
		sb.Draw(tex,v,ModWorld.GetTexRectangle(tex),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		if (tex2 != null) {
			float scale = 1f;
			int ww = tex.Width-4, hh = tex.Height-4;
			if (tex2.Width*scale > ww) scale = 1f*ww/tex2.Width;
			if (tex2.Height*scale > hh) scale = 1f*hh/tex2.Height;
			sb.Draw(tex2,v+new Vector2(2,2)+new Vector2((ww-tex2.Width*scale)/2f,(hh-tex2.Height*scale)/2f),ModWorld.GetTexRectangle(tex2),c,0f,default(Vector2),scale,SpriteEffects.None,0f);
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
		
		v = new Vector2(xo+COLS*44,yo);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Scroll to the top");
			if (Main.mouseLeft && Main.mouseLeftRelease) toSetScroll = 0;
		}
		
		v = new Vector2(xo+COLS*44,yo+26);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Scroll "+ROWS+" lines up");
			if (Main.mouseLeft && Main.mouseLeftRelease) toSetScroll = Math.Max(scroll-ROWS,0);
		}
		
		v = new Vector2(xo+COLS*44,yo+52);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Scroll 1 line up");
			if (Main.mouseLeft && Main.mouseLeftRelease) toSetScroll = Math.Max(scroll-1,0);
		}
		
		v = new Vector2(xo+COLS*44,yo+ROWS*44-26);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Scroll to the bottom");
			if (Main.mouseLeft && Main.mouseLeftRelease) toSetScroll = (int)Math.Ceiling(1f*filtered.Count/ROWS)-ROWS;
		}
		
		v = new Vector2(xo+COLS*44,yo+ROWS*44-52);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Scroll "+ROWS+" lines down");
			if (Main.mouseLeft && Main.mouseLeftRelease) toSetScroll = scroll+ROWS;
		}
		
		v = new Vector2(xo+COLS*44,yo+ROWS*44-78);
		if (ModWorld.MouseRegion(v,new Vector2(tex.Width,tex.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Scroll 1 line down");
			if (Main.mouseLeft && Main.mouseLeftRelease) toSetScroll = scroll+1;
		}
		
		float xx = 0, yy = 0;
		if (!Main.mouseLeft) blockChange = false;
		bool oldBlockChange = blockChange;
		List<string> keys = new List<string>();
		
		if (PostDrawFilter(sb,ModWorld.texItemBlank,filterMode,(filterMode ? "Whitelist" : "Blacklist")+" mode",ref xx,ref yy)) filterMode = !filterMode;
		xx += 1;
		if (PostDrawFilter(sb,ModWorld.texItemBlank,false,"Clear",ref xx,ref yy)) ClearFilters();
		
		xx = 0; yy = 1.5f;
		keys.Clear();
		foreach (KeyValuePair<string,bool> pair in standardCategories) keys.Add(pair.Key);
		foreach (string key in keys) {
			if (PostDrawFilter(sb,ModWorld.texItemBlank,standardCategories[key],key,ref xx,ref yy)) standardCategories[key] = !standardCategories[key];
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
			if (PostDrawFilter(sb,ModWorld.texItemBlank,modCategories[key],s+" items",ref xx,ref yy)) modCategories[key] = !modCategories[key];
		}
		
		if (!oldBlockChange && blockChange) {
			Filter();
			Sort();
			Create();
		}
	}
	public bool PostDrawFilter(SpriteBatch sb, Texture2D tex, bool value, string text, ref float xx, ref float yy, float xxx = 0, float yyy = 0) {
		bool ret = false;
		
		Vector2 v = new Vector2(xo+COLS*44+28+(xx+xxx)*tex.Width,yo+(yy+yyy)*tex.Height);
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
		if (ret) blockChange = true;
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
		Item oldItem = item.CloneItem();
		if (ModWorld.IsBlankItem(item)) {
			Refill(slot);
		} else {
			if (ModWorld.IsBlankItem(Main.mouseItem)) {
				if (!ModWorld.IsBlankItem(item)) {
					item.stack = item.maxStack;
				}
			}
		}
		
		if (!ModWorld.IsBlankItem(item)) {
			if (ModWorld.IsBlankItem(oldItem)) {
				CheatNotification.Add(new CheatNotification("item|item","+"+item.stack+"x "+item.name));
			} else if (oldItem.stack != item.stack) {
				CheatNotification.Add(new CheatNotification("item|item","+"+(item.stack-oldItem.stack)+"x "+item.name));
			}
		}
		
		Main.inventoryBack5Texture = slot == Config.tileInterface.itemSlots.Length-1 ? swap : Main.inventoryBack5Texture;
		Main.inventoryScale = slot == Config.tileInterface.itemSlots.Length-1 ? swapScale : .85f;
		return true;
	}
	
	public override bool CanPlaceSlot(int slot, Item item) {
		if (ModWorld.IsBlankItem(item)) return true;
		if (ModWorld.IsBlankItem(Config.tileInterface.itemSlots[slot])) return false;
		if (item.type != Config.tileInterface.itemSlots[slot].type) return false;
		return true;
	}
	public override void PlaceSlot(int slot) {
		if (ModWorld.IsBlankItem(Config.tileInterface.itemSlots[slot]) && !ModWorld.IsBlankItem(Main.mouseItem)) {
			Player player = Main.player[Main.myPlayer];
			if (player.controlTorch) {
				Item oldItem = Main.mouseItem.CloneItem();
				Main.mouseItem = player.GetItem(player.whoAmi,Main.mouseItem);
				CheatNotification.Add(new CheatNotification("item|item","+"+(ModWorld.IsBlankItem(Main.mouseItem) ? oldItem.stack : oldItem.stack-Main.mouseItem.stack)+"x "+oldItem.name));
				if (!ModWorld.IsBlankItem(Main.mouseItem)) Main.mouseItem = new Item();
			} else CheatNotification.Add(new CheatNotification("item|item","+"+Main.mouseItem.stack+"x "+Main.mouseItem.name));
			Refill(slot);
		}
	}
	public override bool DropSlot(int slot) {
		return false;
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