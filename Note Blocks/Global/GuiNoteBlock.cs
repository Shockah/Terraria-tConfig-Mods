public class GuiNoteBlock:Interfaceable {
	public static string[] soundNames = {"G","G#","A","A#","B","C","C#","D","D#","E","F","F#"};
	
	public static void Create(int x, int y) {
		Codable.RunTileMethod(false,new Vector2(x,y),Main.tile[x,y].type,"ExternalGetNoteBlockLevel",new object[]{});
		int noteLevel = (int)Codable.customMethodReturn;
		
		Config.tileInterface = new InterfaceObj(new GuiNoteBlock(),0,24);
		Player player = Main.player[Main.myPlayer];
		Config.tileInterface.SetLocation(new Vector2(x,y));
		Main.playerInventory = true;
		
		int xo = 80, yo = 270, xx = 0, yy = 0;
		for (int i = 0; i < 24; i++) {
			Config.tileInterface.AddText(soundNames[i%12],xo+xx*48,yo+yy*48,true,noteLevel == i ? 2f : 1f);
			xx++;
			if (xx == 12) {
				xx = 0;
				yy++;
			}
		}
	}
	
	public bool CanPlaceSlot(int slot, Item mouseItem) {return false;}
	public void PlaceSlot(int slot) {}
	public bool DropSlot(int slot) {return false;}
	
	public void ButtonClicked(int num) {
		int x = (int)Config.tileInterface.sourceLocation.X, y = (int)Config.tileInterface.sourceLocation.Y;
		Codable.RunTileMethod(false,new Vector2(x,y),Main.tile[x,y].type,"ExternalSetNoteBlockLevel",new object[]{num});
		Codable.RunTileMethod(false,new Vector2(x,y),Main.tile[x,y].type,"ExternalNoteBlockPlay",new object[]{});
		Create(x,y);
	}
}