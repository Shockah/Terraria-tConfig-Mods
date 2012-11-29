#INCLUDE "FactoryGen.cs"

public void ModifyWorld() {
	GenerateFactory(Main.maxTilesX/2);
}

public static void GenerateFactory(int baseX) {
	int baseY = 0;
	while (true) {
		if (baseY >= Main.maxTilesY/2) break;
		if (Main.tile[baseX,baseY].active) {
			bool isOk = true;
			for (int i = 1; i <= 10; i++) if (!Main.tile[baseX-i,baseY].active) {
				isOk = false;
				break;
			}
			for (int i = 1; i <= 10; i++) if (!Main.tile[baseX+i,baseY].active) {
				isOk = false;
				break;
			}
			
			if (isOk) {
				new FactoryGen().Generate(baseX,baseY);
				break;
			}
		}
		baseY++;
	}
}

public static void SetTile(int x, int y, int tile) {
	Main.tile[x,y].active = true;
	Main.tile[x,y].type = (ushort)tile;
	Codable.InitTile(new Vector2(x,y),tile);
}
public static void ClearTile(int x, int y) {
	Main.tile[x,y].active = false;
}

public static void SetTileRegion(int x1, int y1, int x2, int y2, int tile) {
	int xMin = Math.Min(x1,x2), xMax = Math.Max(x1,x2), yMin = Math.Min(y1,y2), yMax = Math.Max(y1,y2);
	for (int y = yMin; y <= yMax; y++) for (int x = xMin; x <= xMax; x++) SetTile(x,y,tile);
}
public static void ClearTileRegion(int x1, int y1, int x2, int y2) {
	int xMin = Math.Min(x1,x2), xMax = Math.Max(x1,x2), yMin = Math.Min(y1,y2), yMax = Math.Max(y1,y2);
	for (int y = yMin; y <= yMax; y++) for (int x = xMin; x <= xMax; x++) ClearTile(x,y);
}

public static void SetWall(int x, int y, int wall) {
	Main.tile[x,y].wall = (byte)wall;
}
public static void ClearWall(int x, int y) {
	Main.tile[x,y].wall = 0;
}

public static void SetWallRegion(int x1, int y1, int x2, int y2, int wall) {
	int xMin = Math.Min(x1,x2), xMax = Math.Max(x1,x2), yMin = Math.Min(y1,y2), yMax = Math.Max(y1,y2);
	for (int y = yMin; y <= yMax; y++) for (int x = xMin; x <= xMax; x++) SetWall(x,y,wall);
}
public static void ClearWallRegion(int x1, int y1, int x2, int y2) {
	int xMin = Math.Min(x1,x2), xMax = Math.Max(x1,x2), yMin = Math.Min(y1,y2), yMax = Math.Max(y1,y2);
	for (int y = yMin; y <= yMax; y++) for (int x = xMin; x <= xMax; x++) ClearWall(x,y);
}

public static void FillWall(int x, int y, int wall) {
	List<Vector2> toFill = new List<Vector2>(), passed = new List<Vector2>();
	int fillingBlock = Main.tile[x,y].active ? Main.tile[x,y].type : -1;
	toFill.Add(new Vector2(x,y));
	
	while (toFill.Count != 0) {
		Vector2 v = toFill[0];
		toFill.RemoveAt(0);
		if (passed.Contains(v)) continue;
		
		x = (int)Math.Round(v.X);
		y = (int)Math.Round(v.Y);
		if ((Main.tile[x,y].active ? Main.tile[x,y].type : -1) == fillingBlock) {
			SetWall(x,y,wall);
			toFill.Add(new Vector2(x-1,y));
			toFill.Add(new Vector2(x+1,y));
			toFill.Add(new Vector2(x,y-1));
			toFill.Add(new Vector2(x,y+1));
			passed.Add(v);
		}
	}
}