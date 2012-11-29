public class Category {
	public readonly string title;
	public bool toggled = false;
	
	public List<ModPlayer.Achievement> achievements = new List<ModPlayer.Achievement>();
	public List<Category> categories = new List<Category>();
	
	public Category(string title) {
		this.title = title;
	}
	
	public override bool Equals(Object other) {
		if (other == null) return false;
		if (other is Category) {
			Category c = (Category)other;
			return title == c.title;
		} else if (other is String) {
			String s = (String)other;
			return title == s;
		}
		return false;
	}
	public override int GetHashCode() {
		return title.GetHashCode();
	}
	
	public void Draw(SpriteBatch sb, int xx, ref int yy, int ww) {
		if (!Visible()) return;
		
		if (Main.mouseLeft && Main.mouseLeftRelease && ModWorld.GuiAchievements.MouseIn(sb.GraphicsDevice.ScissorRectangle,new Rectangle(xx,yy,ww,42))) {
			toggled = !toggled;
			if (toggled) GuiAchievements.setScroll = yy+GuiAchievements.scroll-120;
		}
		
		Color white = new Color(255,255,255,191);
		Color limish = new Color(136,255,0,191);
		bool achieved = Achieved();
		sb.Draw(ModWorld.Notifier.frame1,new Rectangle(xx+1,yy,ww-2,42),achieved ? limish : white);
		sb.Draw(ModWorld.Notifier.frame2,new Rectangle(xx,yy,1,42),achieved ? limish : white);
		sb.Draw(ModWorld.Notifier.frame2,new Rectangle(xx+ww-1,yy,1,42),achieved ? limish : white);
		
		ModWorld.DrawStringShadowed(sb,Main.fontMouseText,title,new Vector2(xx+8,yy+10),Color.White,Color.Black);
		string value = ""+Value(true)+"/"+Value(false);
		ModWorld.DrawStringShadowed(sb,Main.fontMouseText,value,new Vector2(xx+ww-12-Main.fontMouseText.MeasureString(value).X,yy+10),Color.White,Color.Black);
		
		yy += 42;
		
		if (toggled) {
			for (int i = 0; i < categories.Count; i++) categories[i].Draw(sb,xx+24,ref yy,ww-48);
			for (int i = 0; i < achievements.Count; i++) {
				int parents = 0;
				if (i != 0) for (int j = i-1; j >= 0; j--) if (achievements[j+1].parent != null && achievements[j+1].parent == achievements[j].apiName) parents++; else break;
				achievements[i].Draw(sb,xx+24+(parents*24),ref yy,ww-48-(parents*48));
			}
		}
	}
	
	public bool Visible() {
		if (achievements.Count != 0) return true;
		if (categories.Count == 0) return false;
		for (int i = 0; i < categories.Count; i++) if (categories[i].Visible()) return true;
		return false;
	}
	public bool Achieved() {
		for (int i = 0; i < achievements.Count; i++) if (!achievements[i].achieved) return false;
		for (int i = 0; i < categories.Count; i++) if (!categories[i].Achieved()) return false;
		return true;
	}
	public int Value(bool achieved) {
		int value = 0;
		for (int i = 0; i < achievements.Count; i++) {
			ModPlayer.Achievement ac = achievements[i];
			if (achieved) {if (ac.achieved) value += ac.value;
			} else {if (!ac.hidden || ac.achieved) value += ac.value;}
		}
		for (int i = 0; i < categories.Count; i++) value += categories[i].Value(achieved);
		return value;
	}
}