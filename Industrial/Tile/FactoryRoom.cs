public class FactoryRoom : IEquatable<FactoryRoom> {
	public const int
		ROOM_STANDARD = 0,
		ROOM_ASSEMBLY = 1,
		ROOM_POWERPLANT = 2,
		ROOM_SAFE = 3,
		ROOM_LAB = 4,
		ROOM_MAIN_CONTROL_ROOM = 5;
	public const int
		GRID_X = 50, GRID_Y = 40,
		ROOM_MIN_W = 25, ROOM_MIN_H = 20,
		ROOM_ADD_W = 20, ROOM_ADD_H = 16;
	
	public readonly int gridX, gridY;
	public int type, x, y, w, h;
	public bool mergeU = false, mergeD = false, mergeL = false, mergeR = false;
	public bool connectU = false, connectD = false, connectL = false, connectR = false;
	
	public FactoryRoom(int gridX, int gridY) : this(gridX,gridY,ROOM_STANDARD) {}
	public FactoryRoom(int gridX, int gridY, int type) {
		this.gridX = gridX;
		this.gridY = gridY;
		this.type = type;
		
		w = ROOM_MIN_W+WorldGen.genRand.Next(ROOM_ADD_W+1);
		h = ROOM_MIN_H+WorldGen.genRand.Next(ROOM_ADD_H+1);
		x = (GRID_X-w)/2+(w%2 == 0 ? 0 : WorldGen.genRand.Next(2));
		y = (GRID_Y-h)/2+(h%2 == 0 ? 0 : WorldGen.genRand.Next(2));
	}
	
	public bool Equals(FactoryRoom other) {
		return gridX == other.gridX && gridY == other.gridY;
	}
	
	public List<FactoryRoom> GetOnSides(List<FactoryRoom> rooms) {
		List<FactoryRoom> ret = new List<FactoryRoom>();
		for (int i = 0; i < rooms.Count; i++) {
			FactoryRoom room = rooms[i];
			if (Math.Abs(room.gridX-gridX)+Math.Abs(room.gridY-gridY) == 1) {
				ret.Add(room);
				if (ret.Count == 4) break;
			}
		}
		return ret;
	}
	
	public List<FactoryRoom> GetConnected(List<FactoryRoom> rooms) {
		List<FactoryRoom> ret = new List<FactoryRoom>();
		foreach (FactoryRoom room in GetOnSides(rooms)) {
			if (connectU && gridY-room.gridY == -1) ret.Add(room);
			else if (connectD && gridY-room.gridY == 1) ret.Add(room);
			else if (connectL && gridX-room.gridX == -1) ret.Add(room);
			else if (connectR && gridX-room.gridX == 1) ret.Add(room);
		}
		return ret;
	}
	public List<FactoryRoom> GetMerged(List<FactoryRoom> rooms) {
		List<FactoryRoom> ret = new List<FactoryRoom>();
		foreach (FactoryRoom room in GetOnSides(rooms)) {
			if (mergeU && gridY-room.gridY == -1) ret.Add(room);
			else if (mergeD && gridY-room.gridY == 1) ret.Add(room);
			else if (mergeL && gridX-room.gridX == -1) ret.Add(room);
			else if (mergeR && gridX-room.gridX == 1) ret.Add(room);
		}
		return ret;
	}
	
	public bool ConnectOneRoom(List<FactoryRoom> rooms) {
		if (connectU && connectD && connectL && connectR) return false;
		
		List<FactoryRoom> onSides = GetOnSides(rooms);
		List<FactoryRoom> connected = GetConnected(rooms);
		foreach (FactoryRoom room in connected) onSides.Remove(room);
		
		if (onSides.Count == 0) return false;
		FactoryRoom roomC = onSides.Count == 1 ? onSides[0] : onSides[WorldGen.genRand.Next(onSides.Count)];
		
		if (gridY-roomC.gridY == -1) {
			connectU = true;
			roomC.connectD = true;
		} else if (gridY-roomC.gridY == 1) {
			connectD = true;
			roomC.connectU = true;
		} else if (gridX-roomC.gridX == -1) {
			connectL = true;
			roomC.connectR = true;
		} else if (gridX-roomC.gridX == 1) {
			connectR = true;
			roomC.connectL = true;
		}
		return true;
	}
	public bool MergeOneRoom(List<FactoryRoom> rooms) {
		if (mergeU && mergeD && mergeL && mergeR) return false;
		
		List<FactoryRoom> onSides = GetOnSides(rooms);
		List<FactoryRoom> merged = GetMerged(rooms);
		foreach (FactoryRoom room in merged) onSides.Remove(room);
		
		if (onSides.Count == 0) return false;
		FactoryRoom roomM = onSides.Count == 1 ? onSides[0] : onSides[WorldGen.genRand.Next(onSides.Count)];
		
		if (gridY-roomM.gridY == -1) {
			mergeU = true;
			roomM.mergeD = true;
		} else if (gridY-roomM.gridY == 1) {
			mergeD = true;
			roomM.mergeU = true;
		} else if (gridX-roomM.gridX == -1) {
			mergeL = true;
			roomM.mergeR = true;
		} else if (gridX-roomM.gridX == 1) {
			mergeR = true;
			roomM.mergeL = true;
		}
		return true;
	}
}