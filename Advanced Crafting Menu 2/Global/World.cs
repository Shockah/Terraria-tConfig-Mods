#INCLUDE "GuiCraft.cs"
#INCLUDE "OSICrafting.cs"

public static Texture2D texButton, texAUp, texAUp2, texAUp3, texADown, texADown2, texADown3;
public static bool resetChat = false, shouldInit = true;

public void Initialize(int modId) {
	//ModWorld.modId = modId;
	if (!Main.dedServ) return;
	Init();
}
public static void Init() {
	resetChat = false;
	
	if (Main.dedServ) return;
	
	texButton = Main.goreTexture[Config.goreID["ACM_Button"]];
	
	texAUp = Main.goreTexture[Config.goreID["ACM_Arrow_Up"]];
	texAUp2 = Main.goreTexture[Config.goreID["ACM_Arrow_Up2"]];
	texAUp3 = Main.goreTexture[Config.goreID["ACM_Arrow_Up3"]];
	texADown = Main.goreTexture[Config.goreID["ACM_Arrow_Down"]];
	texADown2 = Main.goreTexture[Config.goreID["ACM_Arrow_Down2"]];
	texADown3 = Main.goreTexture[Config.goreID["ACM_Arrow_Down3"]];
}

public static bool IsBlankItem(Item item) {
	return item == null || item.type == 0 || item.name == null || item.name == "" || item.name == "Unloaded Item" || item.stack <= 0;
}

public static void MouseText(String text) {
	Main.toolTip = new Item();
	Main.buffString = "";
	Config.mainInstance.MouseText(text);
}
public static void ItemMouseText(Item item) {
	if (item == null || item.type == 0) return;
	string tip = item.name;
	Main.toolTip = item.CloneItem();
	Main.buffString = "";
	
	if (item.stack > 1) tip += " ("+item.stack+")";
	Config.mainInstance.MouseText(tip,item.rare,0);
}

public void RegisterOnScreenInterfaces() {
	OSICrafting.Register();
}
public void PostDraw(SpriteBatch sb) {
	if (Config.tileInterface != null && Config.tileInterface.code is GuiCraft) {
		((GuiCraft)Config.tileInterface.code).PostDraw(sb);
		if (((GuiCraft)Config.tileInterface.code).PretendChat()) {
			resetChat = true;
			Main.chatMode = true;
			Main.inputTextEnter = Main.chatRelease = false;
		}
	}
}

public bool PreDrawAvailableRecipes(SpriteBatch sb) {
    Dictionary<int,int> A = new Dictionary<int,int>();
    A.Add(0,0);
    Codable.RunGlobalMethod("ModWorld","AnySinisterMenus",A);
    return A[0] <= 0;
}
public static void AnySinisterMenus(Dictionary<int,int> A) {
	if (Config.tileInterface != null && Config.tileInterface.code is GuiCraft) A[0]++;
}

public static bool MouseRegion(int x, int y, int w, int h) {
	return Main.mouseX >= x && Main.mouseY >= y && Main.mouseX < x+w && Main.mouseY < y+h;
}
public static bool MouseRegion(Vector2 v1, Vector2 v2) {
	return Main.mouseX >= v1.X && Main.mouseY >= v1.Y && Main.mouseX < v1.X+v2.X && Main.mouseY < v1.Y+v2.Y;
}

public static Rectangle? GetTexRectangle(Texture2D tex) {
	return new Rectangle?(new Rectangle(0,0,tex.Width,tex.Height));
}
public static Vector2 GetTexCenter(Texture2D tex) {
	return new Vector2(tex.Width/2f,tex.Height/2f);
}