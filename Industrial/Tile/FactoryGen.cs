public class FactoryGen {
#INCLUDE "FactoryRoom.cs"
	
	public List<FactoryRoom> rooms = new List<FactoryRoom>();
	
	public void Generate(int baseX, int baseY) {
		Vector2[] allowed;
		int count;
		
		allowed = new Vector2[]{new Vector2(0,1)};
		count = WorldGen.genRand.Next(11)+5;
		while (count-- > 0) RandomRoom(allowed);
		
		allowed = new Vector2[]{new Vector2(0,1),new Vector2(-1,0),new Vector2(1,0)};
		count = WorldGen.genRand.Next(21)+10;
		while (count-- > 0) RandomRoom(allowed);
		
		allowed = new Vector2[]{new Vector2(0,-1),new Vector2(0,1),new Vector2(-1,0),new Vector2(1,0)};
		count = WorldGen.genRand.Next(41)+20;
		while (count-- > 0) RandomRoom(allowed);
		
		CreateRooms(baseX,baseY);
		CreateCorridors(baseX,baseY);
	}
	
	public void CreateRooms(int baseX, int baseY) {
		baseX -= rooms[0].w/2-rooms[0].x;
		baseY -= rooms[0].h+rooms[0].y;
		
		for (int i = 0; i < rooms.Count; i++) {
			FactoryRoom room = rooms[i];
			int xx = baseX+room.gridX*FactoryRoom.GRID_X, yy = baseY+room.gridY*FactoryRoom.GRID_Y;
			
			SetTileRegion(xx,yy,xx+FactoryRoom.GRID_X-1,yy+FactoryRoom.GRID_Y-1,Config.tileDefs.ID["Factory Brick"]);
			ClearTileRegion(xx+room.x+3,yy+room.y+3,xx+room.x+room.w-4,yy+room.y+room.h-4);
			//FillWall(xx+room.x+1,yy+room.y+1,Config.wallDefs.ID["Factory Brick Wall"]);
		}
	}
	public void CreateCorridors(int baseX, int baseY) {
		baseX -= rooms[0].w/2-rooms[0].x;
		baseY -= rooms[0].h+rooms[0].y;
		
		int zeroX, zeroY;
		FactoryRoom[,] map = ListToArray(rooms,zeroX,zeroY);
		
		//TODO
	}
	
	public FactoryRoom[,] ListToArray(List<FactoryRoom> rooms, out int firstX, out int firstY) {
		int minX = 999999, minY = 999999, maxX = -999999, maxY = -999999;
		foreach (FactoryRoom room in rooms) {
			if (room.gridX < minX) minX = room.gridX;
			if (room.gridY < minY) minY = room.gridY;
			if (room.gridX > maxX) maxX = room.gridX;
			if (room.gridY > maxY) maxY = room.gridY;
		}
		
		FactoryRoom[,] ret = new FactoryRoom[maxX-minX+1,maxY-minY+1];
		foreach (FactoryRoom room in rooms) ret[room.gridX+minX,room.gridY+minY] = room;
		firstX = -minX;
		firstY = -minY;
		return ret;
	}
	
	public FactoryRoom RandomRoom(Vector2[] allowed) {
		while (true) {
			if (rooms.Count == 0) {
				rooms.Add(new FactoryRoom(0,0));
				return rooms[0];
			} else {
				if (allowed == null || allowed.Length == 0) return null;
				FactoryRoom room = rooms.Count == 1 ? rooms[0] : rooms[WorldGen.genRand.Next(rooms.Count)];
				Vector2 v = allowed.Length == 1 ? allowed[0] : allowed[WorldGen.genRand.Next(allowed.Length)];
				
				int xx = (int)Math.Round(room.gridX+v.X), yy = (int)Math.Round(room.gridY+v.Y);
				for (int i = 0; i < rooms.Count; i++) {
					FactoryRoom room2 = rooms[i];
					if (xx == room2.gridX && yy == room2.gridY) goto continueWhile;
				}
				
				FactoryRoom roomRet = new FactoryRoom(xx,yy);
				rooms.Add(roomRet);
				roomRet.ConnectOneRoom(rooms);
				return roomRet;
			}
			continueWhile: {}
		}
	}
}