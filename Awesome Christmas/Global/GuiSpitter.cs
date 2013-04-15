public class GuiSpitter : Interfaceable {
	private static Texture2D swap;
	private static float swapScale;
	
	public readonly int slots;
	
	public static void Create(Vector2 v, List<Item> items, int slots) {
		int xo = 73, yo = 210;
		
		if (items == null) items = new List<Item>();
		
		Config.tileInterface = new InterfaceObj(new GuiSpitter(slots),slots+1,slots/5);
		Player player = Main.player[Main.myPlayer];
		Config.tileInterface.SetLocation(v);
		Main.playerInventory = true;
		
		for (int y = 0; y < 4; y++) {
			for (int x = 0; x < 5; x++) {
				if (x+y*5 >= slots) goto L;
				Config.tileInterface.AddItemSlot((int)(xo+(x*50)),(int)(yo+(y*50)));
			}
		}
		L: {}
		for (int i = 0; i < items.Count; i++) Config.tileInterface.itemSlots[i] = items[i];
		for (int i = 0; i < slots/5; i++) Config.tileInterface.AddText("Color #"+(i+1),xo+5*50,yo+i*50+10,false);
		Config.tileInterface.AddItemSlot(-100,-100);
	}
	
	public GuiSpitter(int slots) : base() {
		this.slots = slots;
	}
	
	public bool PreDrawSlot(SpriteBatch sb, int slot) {
		if (slot == 0) {
			swap = Main.inventoryBack5Texture;
			swapScale = Main.inventoryScale;
		}
		
		Main.inventoryBack5Texture = slot == slots ? swap : Main.inventoryBack5Texture;
		Main.inventoryScale = slot == slots ? swapScale : .85f;
		return true;
	}
	
	public bool CanPlaceSlot(int slot, Item item) {
		return true;
	}
	public void PlaceSlot(int slot) {}
	public bool DropSlot(int slot) {
		return false;
	}
	
	public void ButtonClicked(int num) {}
}