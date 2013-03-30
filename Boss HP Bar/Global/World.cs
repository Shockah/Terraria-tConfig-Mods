#INCLUDE "PolygonShape.cs"
#INCLUDE "NPCInst.cs"

public const int
	INTERFACE_BAR_BOTTOM = 0,
	INTERFACE_BAR_CIRCULAR = 1;

public static Texture2D texWhite = null, texBack = null, texTop = null;
public static Texture2D[] texPhase = null;

public void Initialize(int modId) {
	if (Main.dedServ) return;
	
	texWhite = new Texture2D(Config.mainInstance.GraphicsDevice,1,1);
	texWhite.SetData(new Color[]{Color.White});
	
	switch (ModGeneric.INTERFACE_TYPE) {
		case INTERFACE_BAR_BOTTOM: {
			Color[] bgc = new Color[20];
			
			texBack = new Texture2D(Config.mainInstance.GraphicsDevice,1,bgc.Length);
			for (int i = 0; i < bgc.Length; i++) bgc[i] = Color.Lerp(Color.Silver,Color.Black,1f*i/bgc.Length/2f);
			texBack.SetData(bgc);
			
			texPhase = new Texture2D[5];
			for (int i = 0; i < 5; i++) texPhase[i] = new Texture2D(Config.mainInstance.GraphicsDevice,1,bgc.Length);
			
			for (int i = 0; i < bgc.Length; i++) bgc[i] = Color.Lerp(Color.Lime,Color.Black,1f*i/bgc.Length/2f);
			texPhase[0].SetData(bgc);
			
			for (int i = 0; i < bgc.Length; i++) bgc[i] = Color.Lerp(Color.Yellow,Color.Black,1f*i/bgc.Length/2f);
			texPhase[1].SetData(bgc);
			
			for (int i = 0; i < bgc.Length; i++) bgc[i] = Color.Lerp(Color.Orange,Color.Black,1f*i/bgc.Length/2f);
			texPhase[2].SetData(bgc);
			
			for (int i = 0; i < bgc.Length; i++) bgc[i] = Color.Lerp(Color.Red,Color.Black,1f*i/bgc.Length/2f);
			texPhase[3].SetData(bgc);
			
			for (int i = 0; i < bgc.Length; i++) bgc[i] = Color.Lerp(Color.Fuchsia,Color.Black,1f*i/bgc.Length/2f);
			texPhase[4].SetData(bgc);
		} break;
		case INTERFACE_BAR_CIRCULAR: {
			texBack = Main.goreTexture[Config.goreID["BossBar1_0"]];
			texTop = Main.goreTexture[Config.goreID["BossBar1_1"]];
			
			texPhase = new Texture2D[5];
			for (int i = 0; i < 5; i++) texPhase[i] = Main.goreTexture[Config.goreID["BossBar1_p"+i]];
		} break;
		default: break;
	}
}

public static List<NPCInst> GetActualNPCs() {
	NPCInst[] ar = new NPCInst[Main.npc.Length];
	for (int i = 0; i < ar.Length; i++) {
		NPC npc = Main.npc[i];
		if (npc == null || !npc.active || npc.life <= 0) continue;
		
		if (npc.aiStyle == 6) {
			int connected = (int)npc.ai[0];
			if (connected > 0) {
				while (true) {
					int conn = (int)Main.npc[connected].ai[0];
					if (conn > 0) connected = conn; else break;
				}
				if (ar[connected] == null) ar[connected] = new NPCInst();
				ar[connected].AddPart(npc);
			} else {
				if (ar[i] == null) ar[i] = new NPCInst();
				ar[i].AddPart(npc);
			}
		} else if (npc.realLife == -1) {
			if (ar[i] == null) ar[i] = new NPCInst();
			ar[i].AddPart(npc);
		} else {
			if (ar[npc.realLife] == null) ar[npc.realLife] = new NPCInst();
			ar[npc.realLife].AddPart(npc);
		}
	}
	
	List<NPCInst> ret = new List<NPCInst>();
	for (int i = 0; i < ar.Length; i++) if (ar[i] != null) {
		foreach (NPC npc in ar[i].parts) if (npc.type == 13) {
			ar[i].separateLife = true;
			break;
		}
		ret.Add(ar[i]);
	}
	return ret;
}
public static bool IsBoss(NPCInst npci) {
	foreach (NPC npc in npci.parts) {
		if (npc.type == 13) return true;
		if (npc.boss) return true;
	}
	return false;
}

public void RegisterOnScreenInterfaces() {
	ModGeneric.OSIBossHPBar.Register();
}
public void PostUpdate() {
	ModGeneric.OSIBossHPBar.PostUpdate();
}

public void ExternalGetBossPhase(List<NPC> parts, string name, Vector2 centerPos, int life, int lifeMax, Object[] ret) {
	switch (name) {
		case "Eye of Cthulhu": case "Retinazer": case "Spazmatism": {
			ret[0] = life <= lifeMax/2 ? 0 : 1;
			ret[1] = life == lifeMax ? 1f : 1f*(life % (lifeMax/2))/(lifeMax/2);
		} break;
	}
}