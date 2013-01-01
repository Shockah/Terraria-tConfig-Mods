public class GuiNoteBlock : Interfaceable {
	public static string[] soundNames = new string[]{"G","G#","A","A#","B","C","C#","D","D#","E","F","F#"};
	
	public static void Create(int x, int y) {
		Create(x,y,ModWorld.GetNoteLevel(new Vector2(x,y)));
	}
	public static void Create(int x, int y, int noteLevel) {
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
		
		if (Main.netMode == 0) {
			ModWorld.SetNoteLevel(new Vector2(x,y),num);
		} else {
			NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SETLEVEL,-1,-1,x,y,(byte)num);
		}
		Codable.RunTileMethod(false,new Vector2(x,y),Main.tile[x,y].type,"ExternalNoteBlockPlay");
		
		Create(x,y,num);
	}
}