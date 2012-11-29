private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};

public static void ExternalInitAchievementsDelegates(
	Action<string,string,string,string,string,int,Texture2D,bool> AddAchievement,
	Action<string[],int> ConfigNetMode,
	Action<string[],int> ConfigDifficulty,
	Action<string[],int> ConfigHardmode,
	Action<string,object,Func<object,object,string>> ConfigProgress,
	Func<string,bool> GetAchieved,
	Func<string,Texture2D,bool> Achieve,
	Action<int,string> AchievePlayer,
	Action<string> AchieveAllPlayers,
	Func<string,object[]> GetProgress,
	Func<string,object,Texture2D,bool> SetProgress,
	Func<string,object,Texture2D,bool> Progress,
	Action<int,string,object> ProgressPlayer,
	Action<string,object> ProgressAllPlayers,
	Func<string,object[]> GetAchievementInfo)
{
	ModPlayer.ExternalInitAchievementsDelegates(AddAchievement,ConfigNetMode,ConfigDifficulty,ConfigHardmode,ConfigProgress,GetAchieved,Achieve,AchievePlayer,AchieveAllPlayers,GetProgress,SetProgress,Progress,ProgressPlayer,ProgressAllPlayers,GetAchievementInfo);
}

public static void PreDrawInterface(SpriteBatch sb) {
	if (Config.tileInterface != null && Config.tileInterface.code is ModPlayer.GuiQuests) {
		Player player = Main.player[Main.myPlayer];
		Config.tileInterface.SetLocation(new Vector2(player.position.X/16,player.position.Y/16));
		ModPlayer.GuiQuests.Draw(sb);
	}
}
public static void PostDraw(SpriteBatch sb) {
	if (!Main.playerInventory) {
		int yoff = 0;
		for (int i = 0; i < ModPlayer.quest.Length; i++) {
			ModPlayer.Quest q = ModPlayer.quest[i];
			if (q == null) continue;
			DrawStringShadowed(sb,Main.fontMouseText,q.questText(),new Vector2(16f,Main.screenHeight-24+yoff),Color.White,Color.Black);
			yoff -= 24;
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
	if (Config.tileInterface != null && Config.tileInterface.code is ModPlayer.GuiQuests) A[0]++;
}

public static void DrawStringShadowed(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, Color colorShadow, Vector2 origin = default(Vector2), float scale = 1f, SpriteEffects effects = SpriteEffects.None) {
	foreach (Vector2 vecOff in shadowOffset) sb.DrawString(font,text,new Vector2(pos.X+vecOff.X,pos.Y+vecOff.Y),colorShadow,0f,origin,scale,effects,0f);
	sb.DrawString(font,text,pos,color,0f,origin,scale,effects,0f);
}