#INCLUDE "Category.cs"

public class GuiAchievements {
	private static RasterizerState _rasterizerState = new RasterizerState(){ScissorTestEnable = true};
	private static Texture2D whiteTex = null;
	private static int scrollDragY = -1;
	
	public static bool visible = false;
	public static int scroll, setScroll;
	public static List<ModPlayer.Achievement> achievements;
	public static List<Category> categories;
	
	public static Microsoft.Xna.Framework.Input.MouseState? state = null, stateOld = null;
	
	static GuiAchievements() {
		if (Main.dedServ) return;
		
		whiteTex = new Texture2D(Config.mainInstance.GraphicsDevice,1,1);
		whiteTex.SetData(new Color[]{Color.White});
	}
	
	public static void Toggle() {
		visible = !visible;
		if (visible) {
			scroll = 0;
			setScroll = -1;
			UpdateAchievementList();
			state = null;
			stateOld = null;
		}
	}
	public static void Draw(SpriteBatch sb) {
		if (!visible) return;
		if (Config.tileInterface != null || Config.npcInterface != null || Main.npcShop != 0 || Main.signBubble || (Main.npcChatText != null && Main.npcChatText != "")) {
			Toggle();
			return;
		}
		
		sb.End();
		
		int guiX = 80, guiY = 120, guiW = Main.screenWidth-guiX*2, guiH = Main.screenHeight-guiY*2-40;
		
		sb.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,null,null,_rasterizerState);
		sb.GraphicsDevice.ScissorRectangle = new Rectangle(guiX,guiY,guiW+1,guiH);
		
		int yStart = guiY-scroll, yy = yStart;
		for (int i = 0; i < categories.Count; i++) categories[i].Draw(sb,guiX,ref yy,guiW-32);
		for (int i = 0; i < achievements.Count; i++) achievements[i].Draw(sb,guiX,ref yy,guiW-32);
		
		sb.End();
		sb.Begin(SpriteSortMode.Immediate,BlendState.NonPremultiplied);
		
		int hMax = yy-yStart, scrollMax = hMax-guiH;
		if (scrollMax < 0) scrollMax = 0;
		if (scroll < 0) scroll = 0;
		if (scroll > scrollMax) scroll = scrollMax;
		
		float sliderY = scrollMax == 0 ? 0f : 1f*scroll/scrollMax, sliderH = scrollMax == 0 ? 1f : 1f*guiH/hMax;
		Rectangle rectBlack = new Rectangle(guiX+guiW-24,guiY,24,guiH); sb.Draw(whiteTex,rectBlack,new Color(1f,1f,1f,.75f));
		Rectangle rectWhite = new Rectangle(guiX+guiW-22,(int)(guiY+2+(guiH-4-(guiH-4)*sliderH)*sliderY),20,(int)((guiH-4)*sliderH)); sb.Draw(whiteTex,rectWhite,new Color(0f,0f,0f,.75f));
		
		if (scrollDragY == -1) {
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				if (MouseIn(rectBlack)) {
					if (MouseIn(rectWhite)) scrollDragY = Main.mouseY-rectWhite.Y; else {
						scrollDragY = rectWhite.Height/2;
						scroll = (int)(1f*(Main.mouseY-scrollDragY-(guiY+2))/(guiH-4)*hMax);
					}
				}
			}
		} else {
			scroll = (int)(1f*(Main.mouseY-scrollDragY-(guiY+2))/(guiH-4)*hMax);
			if (!Main.mouseLeft) scrollDragY = -1;
		}
		
		if (setScroll >= 0) {
			scroll = setScroll;
			setScroll = -1;
		}
		
		stateOld = state;
		state = Microsoft.Xna.Framework.Input.Mouse.GetState();
		if (stateOld.HasValue && state.HasValue) {
			int mouseScrollDiff = (state.Value.ScrollWheelValue-stateOld.Value.ScrollWheelValue)/120;
			scroll -= mouseScrollDiff*56;
			Main.player[Main.myPlayer].selectedItem += mouseScrollDiff;
			while (Main.player[Main.myPlayer].selectedItem < 0) Main.player[Main.myPlayer].selectedItem += 10;
			while (Main.player[Main.myPlayer].selectedItem > 9) Main.player[Main.myPlayer].selectedItem -= 10;
		}
		
		if (scroll < 0) scroll = 0;
		if (scroll > scrollMax) scroll = scrollMax;
		
		int acAchieved = 0, acTotal = 0, acHidden = 0, acLocked = 0, acPoints = 0, acPointsTotal = 0;
		UpdateCounters(ref acAchieved,ref acTotal,ref acHidden,ref acLocked,ref acPoints,ref acPointsTotal);
		
		ModWorld.DrawStringShadowed(sb,Main.fontMouseText,"Achieved: "+acAchieved+"/"+acTotal+(acHidden+acLocked != 0 ? " ("+(acHidden > 0 ? "+"+acHidden+" hidden" : "")+(acLocked > 0 ? (acHidden > 0 ? ", " : "")+"+"+acLocked+" locked" : "")+")" : ""),new Vector2(guiX,guiY+guiH+10),Color.White,Color.Black);
		ModWorld.DrawStringShadowed(sb,Main.fontMouseText,"Total points: "+acPoints+"/"+acPointsTotal,new Vector2(guiX+guiW-Main.fontMouseText.MeasureString("Total points: "+acPoints+"/"+acPointsTotal).X,guiY+guiH+10),Color.White,Color.Black);
	}
	
	public static bool MouseIn(Rectangle rect) {
		return Main.mouseX >= rect.X && Main.mouseY >= rect.Y && Main.mouseX < rect.X+rect.Width && Main.mouseY < rect.Y+rect.Height;
	}
	public static bool MouseIn(Rectangle clip, Rectangle rect) {
		if (Main.mouseX < clip.X || Main.mouseY < clip.Y || Main.mouseX >= clip.X+clip.Width || Main.mouseY >= clip.Y+clip.Height) return false;
		return MouseIn(rect);
	}
	
	public static void UpdateAchievementList() {
		List<ModPlayer.Achievement> list = new List<ModPlayer.Achievement>(ModPlayer.achievements);
		for (int i = 0; i < list.Count; i++) {
			ModPlayer.Achievement ac = list[i];
			if (!ac.CheckDifficulty() || ((ac.hidden || !ac.CheckNetMode() || !ac.CheckHardMode()) && !ac.achieved) || (ac.parent != null && !ModPlayer.ExternalGetAchieved(ac.parent))) list.RemoveAt(i--);
			ac.sub.Clear();
		}
		
		achievements = new List<ModPlayer.Achievement>();
		List<Category> cat = new List<Category>();
		for (int i = 0; i < list.Count; i++) {
			ModPlayer.Achievement ac = list[i];
			
			bool b = true;
			if (ac.parent != null) {
				foreach (ModPlayer.Achievement ac2 in list) if (ac2.apiName == ac.parent) {
					ac2.sub.Add(ac);
					b = false;
					break;
				}
			}
			
			if (b) {
				if (ac.category != null && ac.category != "") {
					string[] spl = ac.category.Replace("->",""+(char)1).Split((char)1);
					List<Category> current = cat;
					Category catCurrent = null;
					for (int j = 0; j < spl.Length; j++) {
						if (!current.Contains(new Category(spl[j]))) current.Add(new Category(spl[j]));
						for (int k = 0; k < current.Count; k++) if (current[k].Equals(spl[j])) {
							catCurrent = current[k];
							current = catCurrent.categories;
							goto L;
						}
						L: {}
					}
					catCurrent.achievements.Add(ac);
				} else achievements.Add(ac);
			}
		}
		
		categories = SortCategories(cat);
	}
	public static void UpdateCounters(ref int acAchieved, ref int acTotal, ref int acHidden, ref int acLocked, ref int acPoints, ref int acPointsTotal) {
		for (int i = 0; i < ModPlayer.achievements.Count; i++) {
			ModPlayer.Achievement ac = ModPlayer.achievements[i];
			if (ac.category == null || ac.category == "" || !ac.CheckDifficulty() || (!ac.CheckNetMode() && !ac.achieved)) continue;
			if ((ac.parent != null && !ModPlayer.ExternalGetAchieved(ac.parent)) || !ac.CheckHardMode()) acLocked++;
			else if (ac.achieved) {
				acTotal++;
				acAchieved++;
				acPoints += ac.value;
				acPointsTotal += ac.value;
			} else {
				if (!ac.CheckNetMode()) continue;
				if (ac.hidden) acHidden++; else {
					acTotal++;
					acPointsTotal += ac.value;
				}
			}
		}
	}
	public static List<Category> SortCategories(List<Category> list) {
		List<Category> ret = new List<Category>();
		while (list.Count != 0) {
			Category c = null;
			int cIndex = -1;
			
			for (int i = 0; i < list.Count; i++) if (c == null || list[i].title == "Terraria" || String.Compare(list[i].title,c.title) < 0) {
				c = list[i];
				cIndex = i;
			}
			
			ret.Add(c);
			list.Remove(c);
			c.categories = SortCategories(c.categories);
			c.achievements = SortAchievements(c.achievements);
		}
		
		return ret;
	}
	public static List<ModPlayer.Achievement> SortAchievements(List<ModPlayer.Achievement> list) {
		List<ModPlayer.Achievement> ret = new List<ModPlayer.Achievement>();
		while (list.Count != 0) {
			for (int i = 0; i < list.Count; i++) if (list[i].parent != null) {
				for (int j = 0; j < ret.Count; j++) if (ret[j].apiName == list[i].parent) {
					ret.Insert(j+1,list[i]);
					list.RemoveAt(i);
					goto L;
				}
			}
			
			ModPlayer.Achievement ac = null;
			int acIndex = -1;
			
			for (int i = 0; i < list.Count; i++) if (Object.ReferenceEquals(list[i].Compare(ret,ac),list[i])) {
				ac = list[i];
				acIndex = i;
			}
			
			ret.Add(ac);
			list.Remove(ac);
			
			L: {}
		}
		
		foreach (ModPlayer.Achievement ac in ret) if (ac.sub.Count > 0) ac.sub = SortAchievements(ac.sub);
		return ret;
	}
}