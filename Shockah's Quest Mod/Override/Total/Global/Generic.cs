public static int getPlayerValue(Player player) {
	int v = ModPlayer.questsFinished/3+3;
	return (int)(v*Math.Pow(v,1.3d)*(.75d+Main.rand.NextDouble()*.5d));
}