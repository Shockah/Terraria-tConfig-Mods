public class Util {
	public static float LdirX(double dist, double angle) {
		return (float)(-Math.Cos((angle+180)*Math.PI/180f)*dist);
	}
	public static float LdirY(double dist, double angle) {
		return (float)(Math.Sin((angle+180)*Math.PI/180f)*dist);
	}
	
	public static float Direction(Vector2 v1, Vector2 v2) {
		return (float)(Math.Atan2(v1.Y-v2.Y,v2.X-v1.X)*(180f/Math.PI));
	}

	public static Vector2 Vector(float dist, float angle) {
		return new Vector2(LdirX(dist,angle),LdirY(dist,angle));
	}
}