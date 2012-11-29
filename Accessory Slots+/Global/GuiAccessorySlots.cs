public class GuiAccessorySlots:Interfaceable {
	private static Texture2D swap;
	private static float swapScale;
	
	public bool PreDrawSlot(SpriteBatch sb, int slot) {
		if (slot == 0) {
			swap = Main.inventoryBack5Texture;
			swapScale = Main.inventoryScale;
		}
		
		Main.inventoryBack5Texture = slot == ModGeneric.extraSlots ? swap : Main.inventoryBack3Texture;
		Main.inventoryScale = slot == ModGeneric.extraSlots ? swapScale : .85f;
		return true;
	}
	
	public bool CanPlaceSlot(int slot, Item item) {
		if (slot >= ModGeneric.extraSlots) return false;
		if (item == null || item.type == 0) return true;
		if (item.stack != 1 || item.maxStack != 1 || !item.accessory) return false;
		if (item.RunMethod("CanEquip",new object[]{Main.player[Main.myPlayer],-slot-1}) && !(bool)Codable.customMethodReturn) return false;
		if (item.RunMethod("AccCheck",new object[]{Main.player[Main.myPlayer],-slot-1}) && !(bool)Codable.customMethodReturn) return false;
		return true;
	}
	public void PlaceSlot(int slot) {
		if (Main.mouseItem.type != 0) Main.mouseItem.RunMethod("OnUnequip",new object[]{Main.player[Main.myPlayer],-slot-1});
		ModWorld.slots.itemSlots[slot].RunMethod("OnEquip",new object[]{Main.player[Main.myPlayer],-slot-1});
		ModWorld.SendItemData(Main.myPlayer,ModWorld.ExternalGetAccessorySlots(),-1,Main.myPlayer);
	}
	public bool DropSlot(int slot) {
		return false;
	}
	public void SlotRightClicked(int slot) {}
	
	public void ButtonClicked(int num) {}
}