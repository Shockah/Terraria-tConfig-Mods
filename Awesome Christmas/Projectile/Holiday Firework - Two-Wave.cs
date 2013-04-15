public ModWorld.Firework firework;

public float dist = 0;

public void Initialize()
{
	firework = new ModWorld.FireworkTwoWave(BitConverter.ToInt32(BitConverter.GetBytes(projectile.velocity.X),0));
    projectile.timeLeft = 10000;
	dist = (30+Main.rand.Next(15))*8f;
}
public void AI()
{
    projectile.velocity.X = 0;
}

public bool PreDraw(SpriteBatch sb) {
	if (projectile.lastPosition != default(Vector2)) {
		float dist2 = Math.Abs(projectile.lastPosition.Y-projectile.position.Y);
		dist -= dist2;
	}
	if (dist > 0) return true;
	return firework.DrawProjectile(sb,projectile);
}
public void PostDraw(SpriteBatch sb) {
	if (dist > 0) return;
	firework.Draw(sb,projectile);
}

public void PostKill() {
    firework.PostKill(projectile);
}