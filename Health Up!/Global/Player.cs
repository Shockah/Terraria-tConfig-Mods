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
	ConfigProgress("TERRARIA_MAXHEALTH",400*Settings.GetInt("multLife"),null);
	ConfigProgress("TERRARIA_MAXMANA",200*Settings.GetInt("multMana"),null);
}