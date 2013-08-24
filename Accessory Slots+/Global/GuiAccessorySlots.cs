public class GuiAccessorySlots : Interfaceable {
	private static Texture2D swap;
	private static float swapScale;
	
	public bool PreDrawSlot(SpriteBatch sb, int slot) {
		int eslots = Settings.GetInt("slots");
		
		if (slot == 0) {
			swap = Main.inventoryBack5Texture;
			swapScale = Main.inventoryScale;
			for (int i = 0; i < eslots; i++) ModWorld.gui.itemSlots[i] = ModPlayer.accessories[i];
		}
		
		if (slot != eslots) ModWorld.gui.slotLocation[slot].X = Main.screenWidth-139-slot/3*48;
		
		Main.inventoryBack5Texture = slot == eslots ? swap : Main.inventoryBack3Texture;
		Main.inventoryScale = slot == eslots ? swapScale : .85f;
		
		if (slot == eslots) {
			for (int i = 0; i < eslots; i++) ModPlayer.accessories[i] = ModWorld.gui.itemSlots[i];
		}
		return true;
	}
	
	public bool CanPlaceSlot(int slot, Item item) {
		if (slot >= Settings.GetInt("slots")) return false;
		if (item == null || item.type == 0 || item.name == null || item.name == "" || item.stack <= 0) return true;
		if (!item.accessory) return false;
		if (item.RunMethod("CanEquip",Main.player[Main.myPlayer],-slot-1) && !(bool)Codable.customMethodReturn) return false;
		if (item.RunMethod("AccCheck",Main.player[Main.myPlayer],-slot-1) && !(bool)Codable.customMethodReturn) return false;
		return true;
	}
	public void PlaceSlot(int slot) {
		if (Main.mouseItem.type != 0) Main.mouseItem.RunMethod("OnUnequip",Main.player[Main.myPlayer],-slot-1);
		ModWorld.gui.itemSlots[slot].RunMethod("OnEquip",Main.player[Main.myPlayer],-slot-1);
		
		MemoryStream ms = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(ms);
		
		bw.Write((byte)Main.myPlayer);
		bw.Write((byte)slot);
		ItemSave(bw,ModWorld.gui.itemSlots[slot]);
		
		byte[] data = ms.ToArray();
		object[] toSend = new object[data.Length];
		for (int i = 0; i < data.Length; i++) toSend[i] = data[i];
		NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_ITEM,-1,-1,toSend);
	}
	public bool DropSlot(int slot) {
		return false;
	}
	public void SlotRightClicked(int slot) {}
	
	public void ButtonClicked(int num) {}
}