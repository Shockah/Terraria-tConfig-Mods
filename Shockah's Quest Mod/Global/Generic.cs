public static int getPlayerValue(Player player) {
	double v = player.statDefense/5+((player.statLifeMax-100)+player.statManaMax2)/20d+3d;
	return (int)(v*Math.Pow(v,1.3d)*(.75d+Main.rand.NextDouble()*.5d));
}