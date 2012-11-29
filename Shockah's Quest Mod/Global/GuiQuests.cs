public class GuiQuests:Interfaceable {
	public static ModPlayer.Quest[] quests = new ModPlayer.Quest[ModPlayer.quest.Length];
	public static bool[] taken = new bool[ModPlayer.quest.Length];
	private static Texture2D swap = Main.inventoryBack5Texture;
	
	public static void Create() {
		const int off = 44;
		int xo = 80, yo = 208;
		
		Config.tileInterface = new InterfaceObj(new GuiQuests(),ModPlayer.quest.Length+1,ModPlayer.quest.Length+1);
		Player player = Main.player[Main.myPlayer];
		Config.tileInterface.SetLocation(new Vector2(player.position.X/16,player.position.Y/16));
		Main.playerInventory = true;
		
		for (int i = 0; i < ModPlayer.quest.Length; i++) {
			quests[i] = ModPlayer.quest[i] ?? ModPlayer.Quest.getRandomQuest(Main.player[Main.myPlayer]);
			taken[i] = ModPlayer.quest[i] != null;
			Config.tileInterface.AddItemSlot(xo,yo+(i*off));
			Config.tileInterface.AddText(quests[i].questTypeName()+": "+quests[i].questText(),xo+48,yo+10+(i*off),true);
			Config.tileInterface.itemSlots[i].SetDefaults(quests[i].reward.itemName);
			Config.tileInterface.itemSlots[i].stack = quests[i].reward.amount;
		}
		Config.tileInterface.AddText("Total quests completed: "+ModPlayer.questsFinished,xo,yo+6+(5*off),false);
		Config.tileInterface.AddItemSlot(-100,-100);
	}
	
	public static void Draw(SpriteBatch sb) {}
	
	public bool PreDrawSlot(SpriteBatch sb, int slot) {
		if (slot == ModPlayer.quest.Length) {
			Main.inventoryBack5Texture = swap;
			/*for (int i = 0; i < ModPlayer.quest.Length; i++) {
				int diff = quests[i].reward.amount-Config.tileInterface.itemSlots[i].stack;
				Main.mouseItem.stack -= diff;
				Config.tileInterface.itemSlots[i].stack += diff;
			}*/
		} else {
			Main.inventoryBack5Texture = taken[slot] ? Main.inventoryBack3Texture : swap;
		}
		return true;
	}
	
	public bool CanPlaceSlot(int slot, Item mouseItem) {
		return false;
	}
	public void PlaceSlot(int slot) {}
	public bool DropSlot(int slot) {
		return false;
	}
	public void SlotRightClicked(int slot) {}
	
	public void ButtonClicked(int num) {
		ModPlayer.quest[num] = taken[num] ? null : quests[num];
		Create();
	}
}