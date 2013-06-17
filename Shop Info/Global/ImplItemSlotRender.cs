public class ImplItemSlotRender : IItemSlotRender {
	public void PreDrawItemInSlot(SpriteBatch sb, Color color, Item item, Vector2 pos, float sc, ref bool letDraw) {}
	public void PostDrawItemInSlot(SpriteBatch sb, Color color, Item item, Vector2 pos, float sc, bool ranVanilla) {
		if (Main.npcShop <= 0) return;
		int val = item.value;
		
		bool shop = false;
		foreach (Item shopItem in Config.mainInstance.shop[Main.npcShop].item) {
			if (Object.ReferenceEquals(item,shopItem)) {
				shop = true;
				break;
			}
		}
		if (!shop) val /= 5;
		if (val <= 0) return;
		
		string text = FormatCoins(val*item.stack);
		DrawStringShadowed(sb,Main.fontItemStack,text,new Vector2(pos.X+(40f-Main.fontItemStack.MeasureString(text).X)*sc,pos.Y+10f*sc),GetColorForValue(val*item.stack),Color.Black,default(Vector2),sc);
	}
	
	private Color GetColorForValue(int value) {
		if (value < 100) {
			float[] ar = Main.colorShopCopper;
			return new Color((int)ar[0],(int)ar[1],(int)ar[2]);
		}
		
		value /= 100;
		if (value < 100) {
			float[] ar = Main.colorShopSilver;
			return new Color((int)ar[0],(int)ar[1],(int)ar[2]);
		}
		
		value /= 100;
		if (value < 100) {
			float[] ar = Main.colorShopGold;
			return new Color((int)ar[0],(int)ar[1],(int)ar[2]);
		}
		
		value /= 100;
		if (value < 100) {
			float[] ar = Main.colorShopPlatinum;
			return new Color((int)ar[0],(int)ar[1],(int)ar[2]);
		}
		
		return Color.White;
	}
	private string FormatCoins(int value) {
		int P = 0, G = 0, S = 0, C = 0;
		C = value % 100; value /= 100;
		S = value % 100; value /= 100;
		G = value % 100; value /= 100;
		P = value;
		
		if (P > 0) return ""+P+"p";
		if (G > 0) return ""+G+"g";
		if (S > 0) return ""+S+"s";
		return ""+C+"c";
	}
}