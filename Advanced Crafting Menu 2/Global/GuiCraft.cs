#INCLUDE "Container.cs"
#INCLUDE "ContainerRecipe.cs"
#INCLUDE "Util.cs"
#INCLUDE "RecipeDef.cs"

public class GuiCraft : Interfaceable {
	private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};
	public static Microsoft.Xna.Framework.Input.MouseState? state = null, stateOld = null;
	public static Texture2D whiteTex = null;
	
	static GuiCraft() {
		if (Main.dedServ) return;
		
		whiteTex = new Texture2D(Config.mainInstance.GraphicsDevice,1,1);
		whiteTex.SetData(new Color[]{Color.White});
	}
	
	public static void DrawStringShadowed(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, Color colorShadow, Vector2 origin = default(Vector2), float scale = 1f, SpriteEffects effects = SpriteEffects.None) {
		if (text == null) return;
		foreach (Vector2 vecOff in shadowOffset) sb.DrawString(font,text,new Vector2(pos.X+vecOff.X,pos.Y+vecOff.Y),colorShadow,0f,origin,scale,effects,0f);
		sb.DrawString(font,text,pos,color,0f,origin,scale,effects,0f);
	}
	
	public const int xo = 16, yo = 210;
	public const int ROWS = 5, COLS = 3;
	
	public static Container container;
	
	public static List<RecipeDef> allDefs = new List<RecipeDef>();
	public static List<RecipeDef> filtered = new List<RecipeDef>();
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
		
		for (int i = 0; i < Main.numAvailableRecipes; i++) {
			Recipe recipe = Main.recipe[Main.availableRecipe[i]];
			if (ModWorld.IsBlankItem(recipe.createItem)) continue;
			
			allDefs.Add(new RecipeDef(recipe));
			
			string modpack = GetItemModpack(recipe.createItem);
			if (!modCategories.ContainsKey(modpack)) modCategories.Add(modpack,false);
			
			if (!Main.dedServ) {
				if (modpack == "") continue;
				bool vanilla = recipe.createItem.type <= 603;
				if (!tmpVanillaItem.ContainsKey(modpack) || (tmpVanillaItem[modpack] && !vanilla)) {
					Texture2D tex = Main.itemTexture[recipe.createItem.type];
					modTextures[modpack] = tex;
					customTex[modpack] = tex;
					tmpVanillaItem[modpack] = vanilla;
				}
			}
		}
		
		if (!Main.dedServ) {
			modTextures[""] = Main.npcHeadTexture[1];
		}
		
		if (!Main.dedServ) {
			Codable.RunGlobalMethod("ModGeneric","ExternalACMSetCategoryTexture",customTex);
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
		if (Config.tileInterface == null || !(Config.tileInterface.code is GuiCraft)) {
			ClearFilters();
			Filter();
			Sort();
		}
		
		if (filtered.Count <= ROWS*COLS) scroll = 0;
		else {
			int lines = (int)Math.Ceiling(1f*filtered.Count/COLS);
			scroll = Math.Min(Math.Max(scroll,0),lines-ROWS);
		}
		
		Config.tileInterface = new InterfaceObj(new GuiCraft(),0,0);
		Player player = Main.player[Main.myPlayer];
		Config.tileInterface.SetLocation(new Vector2(player.position.X/16f,player.position.Y/16f));
		Main.playerInventory = true;
		GuiCraft.scroll = scroll;
		
		container = new ContainerRecipe(new Vector2(xo,yo),ROWS,COLS,filtered);
		for (int y = 0; y < ROWS; y++) for (int x = 0; x < COLS; x++) Refill(y*COLS+x);
	}
	public static void Refill(int slot) {
		Item item = new Item();
		try {
			item = filtered[slot+scroll*COLS].recipe.createItem;
		} catch (Exception) {}
		if (ModWorld.IsBlankItem(item)) item = new Item();
		
		container.slots[slot].item = item;
		((ContainerRecipe.SlotRecipe)container.slots[slot]).recipe = IsBlankItem(item) ? null : filtered[slot+scroll*COLS].recipe;
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
		foreach (RecipeDef def in allDefs) {
			bool b = true;
			if (b) {
				if (lastSearch != null && lastSearch != "") {
					if (!def.recipe.createItem.name.ToLower().Contains(lastSearch)) b = false;
				}
			}
			if (b) foreach (KeyValuePair<string,bool> pair in standardCategories) {
				if (b) if (!Filter(filterMode,pair.Value,def.categories[pair.Key])) b = false;
			}
			if (b) foreach (KeyValuePair<string,bool> pair in modCategories) {
				if (b) {
					string modpack = GetItemModpack(def.recipe.createItem);
					if (!Filter(filterMode,pair.Value,(pair.Key == "" && def.recipe.createItem.type <= 603) || modpack == pair.Key)) b = false;
				}
			}
			if (b) {
				b = false;
				for (int i = 0; i < Main.numAvailableRecipes; i++) {
					Recipe recipe = Main.recipe[Main.availableRecipe[i]];
					if (recipe.createItem.type == def.recipe.createItem.type && recipe.createItem.name == def.recipe.createItem.name) {
						b = true;
						break;
					}
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
	
	public void PreDrawInterface(SpriteBatch sb) {
		int COLS = GuiCraft.COLS*2;
		
		container.HandlePreDrawInterface(sb);
		
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
		
		int lines = (int)Math.Ceiling(1f*filtered.Count/GuiCraft.COLS);
		int scrollMax = filtered.Count <= ROWS*GuiCraft.COLS ? 0 : lines-ROWS;
		if (scrollMax > 0) {
			float fScroll = 1f*scroll/lines;
			float fScrollH = 1f*ROWS/lines;
			float fScrollPX = ROWS*44-164;
			sb.Draw(whiteTex,new Rectangle(xo+COLS*44+2,yo+80,20,(int)(fScrollPX+4)),Color.Silver);
			sb.Draw(whiteTex,new Rectangle(xo+COLS*44+4,(int)(yo+82+(fScroll*fScrollPX)),16,(int)Math.Max(fScrollH*fScrollPX,1)),Color.Black);
		}
		
		float xx = 0, yy = 0;
		
		PreDrawFilter(sb,ModWorld.texButton,null,filterMode,ref xx,ref yy);
		xx += 1;
		PreDrawFilter(sb,ModWorld.texButton,Main.cdTexture,false,ref xx,ref yy);
		
		xx = 0; yy = 1.5f;
		foreach (KeyValuePair<string,bool> pair in standardCategories) {
			PreDrawFilter(sb,ModWorld.texButton,standardTextures.ContainsKey(pair.Key) ? standardTextures[pair.Key] : null,pair.Value,ref xx,ref yy);
		}
		
		if (xx == 0) {
			yy += .5f;
		} else {
			xx = 0;
			yy += 1.5f;
		}
		
		foreach (KeyValuePair<string,bool> pair in modCategories) {
			PreDrawFilter(sb,ModWorld.texButton,modTextures.ContainsKey(pair.Key) ? modTextures[pair.Key] : null,pair.Value,ref xx,ref yy);
		}
		
		DrawStringShadowed(sb,Main.fontMouseText,"Recipes matching filters: "+filtered.Count,new Vector2(xo+COLS*44+32+ModWorld.texButton.Width*3,yo+(Main.craftGuide || Main.ForceGuideMenu ? 50 : 0)+44),Color.White,Color.Black);
		
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
		if ((search != null && search != "") || searchMode) DrawStringShadowed(sb,Main.fontMouseText,"Search: "+search+(searchMode ? "|" : ""),new Vector2(xo+COLS*44+32+ModWorld.texButton.Width*3,yo+(Main.craftGuide || Main.ForceGuideMenu ? 50 : 0)+44+20),Color.White,Color.Black);
		stateEnter = newStateEnter;
	}
	public void PreDrawFilter(SpriteBatch sb, Texture2D tex, Texture2D tex2, bool value, ref float xx, ref float yy, float xxx = 0, float yyy = 0) {
		int COLS = GuiCraft.COLS*2;
		
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
	
	public void PostDraw(SpriteBatch sb) {
		int COLS = GuiCraft.COLS*2;
		
		container.HandlePostDraw(sb);
		
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
		
		if (PostDrawFilter(sb,ModWorld.texButton,filterMode,(filterMode ? "Whitelist" : "Blacklist")+" mode",ref xx,ref yy)) filterMode = !filterMode;
		xx += 1;
		if (PostDrawFilter(sb,ModWorld.texButton,false,"Clear",ref xx,ref yy)) ClearFilters();
		
		xx = 0; yy = 1.5f;
		keys.Clear();
		foreach (KeyValuePair<string,bool> pair in standardCategories) keys.Add(pair.Key);
		foreach (string key in keys) {
			if (PostDrawFilter(sb,ModWorld.texButton,standardCategories[key],key,ref xx,ref yy)) standardCategories[key] = !standardCategories[key];
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
			if (PostDrawFilter(sb,ModWorld.texButton,modCategories[key],s+" items",ref xx,ref yy)) modCategories[key] = !modCategories[key];
		}
		
		if (!oldBlockChange && blockChange) {
			Filter();
			Sort();
			Create();
		}
	}
	public bool PostDrawFilter(SpriteBatch sb, Texture2D tex, bool value, string text, ref float xx, ref float yy, float xxx = 0, float yyy = 0) {
		int COLS = GuiCraft.COLS*2;
		
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
	
	public bool PretendChat() {
		return searchMode;
	}
	
	public bool CanPlaceSlot(int slot, Item item) {return false;}
	public void PlaceSlot(int slot) {}
	public bool DropSlot(int slot) {return false;}
	public void ButtonClicked(int num) {}
}