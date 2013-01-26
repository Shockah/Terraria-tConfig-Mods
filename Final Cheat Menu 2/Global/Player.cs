public const float noclipMoveSpeed = 32f;

public void Initialize() {
	ModWorld.Init();
}

public void UpdatePlayer(Player player) {
	if (player == null || !player.active || player.name == "") return;
	if (ModWorld.enabledNoclip[player.whoAmi]) {
		player.position -= player.velocity;
		player.velocity = default(Vector2);
		player.gravControl = false;
		player.baseGravity = 0f;
		
		if (player.controlLeft) player.position.X -= noclipMoveSpeed;
		if (player.controlRight) player.position.X += noclipMoveSpeed;
		if (player.controlUp) player.position.Y -= noclipMoveSpeed;
		if (player.controlDown) player.position.Y += noclipMoveSpeed;
	}
	if (ModWorld.enabledGodmode[player.whoAmi]) {
		player.immune = true;
		player.immuneAlpha = 0;
	}
	if (ModWorld.enabledAllLight) {
		Lighting.addLight((int)(Main.player[Main.myPlayer].position.X/16),(int)(Main.player[Main.myPlayer].position.X/16),1f,1f,1f);
	}
	
	if (player.whoAmi != Main.myPlayer) return;
	ModWorld.CheatNotification.Update();
}