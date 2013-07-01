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
	ModWorld.ExternalInitAchievementsDelegates(AddAchievement,ConfigNetMode,ConfigDifficulty,ConfigHardmode,ConfigProgress,GetAchieved,Achieve,AchievePlayer,AchieveAllPlayers,GetProgress,SetProgress,Progress,ProgressPlayer,ProgressAllPlayers,GetAchievementInfo);
}

public void UpdatePlayer(Player player) {
	if (Main.netMode == 2) return;
	if (player.whoAmi != Main.myPlayer) return;
	
	Item itemSelected = player.inventory[player.selectedItem];
	if (itemSelected.type > 0 && itemSelected.stack > 0 && (itemSelected.name == "Bottle" || itemSelected.name == "Jar") && Main.rand.Next(250) == 0) {
		ModWorld.EffectFirefly close = null;
		double closeDist = -1;
		foreach (ModWorld.Effect e in ModWorld.GetAllOfType(typeof(ModWorld.EffectFirefly))) {
			ModWorld.EffectFirefly firefly = (ModWorld.EffectFirefly)e;
			double d = Vector2.Distance(player.position,firefly.GetDrawPos());
			if (close == null || d < closeDist) {
				close = firefly;
				closeDist = d;
			}
		}
		
		if (close != null && closeDist <= 64) {
			for (int i = 0; i < 40; i++) {
				Item item = player.inventory[i];
				if (item.type <= 0 || item.stack <= 0) {
					item.SetDefaults("Firefly in a "+itemSelected.name);
					item.stack = 1;
					item.RunMethod("ExternalSetFirefly",close);
					
					close.Destroy();
					itemSelected.stack--;
					if (itemSelected.stack <= 0) itemSelected.type = 0;
					close.OnCatch(player);
					
					if (Main.netMode != 1) return;
					using (MemoryStream ms = new MemoryStream())
					using (BinaryWriter bw = new BinaryWriter(ms)) {
						bw.Write((byte)player.whoAmi);
						bw.Write(close.id);
						ModWorld.NetworkHelper.Send(ModWorld.NetworkHelper.FLYCATCH,ms);
					}
					return;
				}
			}
		}
	}
}