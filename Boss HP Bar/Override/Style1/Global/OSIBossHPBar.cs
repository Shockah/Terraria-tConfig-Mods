public class OSIBossHPBar : OnScreenInterfaceable {
	private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};
	private static DepthStencilState dss1, dss2;
	private static BlendState bs1;
	private static RenderTarget2D rt;
	private static SpriteBatch sb;
	
	public static void Register() {
		OnScreenInterface.Register(new OSIBossHPBar(),OnScreenInterface.LAYER_INTERFACE_SCREEN);
		
		dss1 = new DepthStencilState {
			StencilEnable = true,
			StencilFunction = CompareFunction.Equal,
			StencilPass = StencilOperation.Increment,
			ReferenceStencil = 0,
			DepthBufferEnable = false,
		};
		bs1 = new BlendState();
		bs1.ColorWriteChannels = ColorWriteChannels.None;
		
		dss2 = new DepthStencilState {
			StencilEnable = true,
			StencilFunction = CompareFunction.NotEqual,
			StencilPass = StencilOperation.Keep,
			ReferenceStencil = 0,
			DepthBufferEnable = false,
		};
		
		rt = new RenderTarget2D(
			Config.mainInstance.graphics.GraphicsDevice,
			1920,
			96,
			false,Config.mainInstance.graphics.GraphicsDevice.DisplayMode.Format,DepthFormat.Depth24Stencil8
		);
		sb = new SpriteBatch(Config.mainInstance.graphics.GraphicsDevice);
	}
	
	public static float LdirX(double dist, double angle) {
		return (float)(-Math.Cos((angle+180)*Math.PI/180f)*dist);
	}
	public static float LdirY(double dist, double angle) {
		return (float)(Math.Sin((angle+180)*Math.PI/180)*dist);
	}
	
	public static void PostUpdate() {
		int xx = 4;
		
		List<ModWorld.NPCInst> list = ModWorld.GetActualNPCs();
		foreach (ModWorld.NPCInst npci in list) {
			if (!ModWorld.IsBoss(npci)) continue;
			var gd = Config.mainInstance.graphics.GraphicsDevice;
			
			Player p = Main.player[Main.myPlayer];
			if (Vector2.Distance(p.position+new Vector2(p.width/2f,p.height/2f),npci.GetCenterPos()) > 2*Math.Sqrt(Math.Pow(Main.screenWidth,2)+Math.Pow(Main.screenHeight,2))) continue;
			
			gd.SetRenderTarget(rt);
			
			float hpPercent = 1f*npci.GetLife()/npci.GetLifeMax();
			int phase = -1;
			
			Object[] ret = new Object[]{null,null};
			if (Codable.RunGlobalMethod("ModWorld","ExternalGetBossPhase",npci.parts,npci.GetName(),npci.GetCenterPos(),npci.GetLife(),npci.GetLifeMax(),ret)) {
				if (ret[0] != null && ret[1] != null) {
					phase = (int)ret[0];
					hpPercent = (float)ret[1];
				}
			}
			int angle = (int)(hpPercent*270);
			
			sb.Begin(SpriteSortMode.Immediate,bs1,null,dss1,null);
			gd.Clear(ClearOptions.Target | ClearOptions.Stencil,Color.Transparent,0,0);
			VertexPositionColorTexture[] pointList = new VertexPositionColorTexture[Math.Max(angle/2+1,3)];
			pointList[0] = new VertexPositionColorTexture(new Vector3(48,48,0),Color.White,default(Vector2));
			for (int i = 1; i <= 2; i++) pointList[pointList.Length-i] = new VertexPositionColorTexture(new Vector3(48+LdirX(48,270),48+LdirY(48,270),0),Color.White,default(Vector2));
			for (int i = 0; i < angle/2; i++) pointList[i+1] = new VertexPositionColorTexture(new Vector3(48+LdirX(48,270-i*2),48+LdirY(48,270-i*2),0),Color.White,default(Vector2));
			new ModWorld.PolygonShape(Config.mainInstance.graphics.GraphicsDevice,pointList).Draw();;
			sb.End();
			
			sb.Begin(SpriteSortMode.Immediate,BlendState.NonPremultiplied,null,dss2,null);
			sb.Draw(phase == -1 ? ModWorld.texTop : ModWorld.texPhase[phase],default(Vector2),Color.White);
			sb.End();
			gd.SetRenderTarget(null);
			
			string s = ""+npci.GetLife()+"/"+npci.GetLifeMax();
			xx += (int)(Math.Max(Main.fontMouseText.MeasureString(s).X,Main.fontMouseText.MeasureString(npci.GetName()).X)+68);
		}
	}
	
	public void DrawOnScreen(SpriteBatch sb, double layer) {
		int xx = 4, yy = Main.screenHeight-100;
		
		List<ModWorld.NPCInst> list = ModWorld.GetActualNPCs();
		foreach (ModWorld.NPCInst npci in list) {
			if (!ModWorld.IsBoss(npci)) continue;
			
			Player p = Main.player[Main.myPlayer];
			if (Vector2.Distance(p.position+new Vector2(p.width/2f,p.height/2f),npci.GetCenterPos()) > 2*Math.Sqrt(Math.Pow(Main.screenWidth,2)+Math.Pow(Main.screenHeight,2))) continue;
			
			sb.Draw(ModWorld.texBack,new Vector2(xx,yy),Color.White);
			
			int phase = -1;
			Object[] ret = new Object[]{null,null};
			if (Codable.RunGlobalMethod("ModWorld","ExternalGetBossPhase",npci.parts,npci.GetName(),npci.GetCenterPos(),npci.GetLife(),npci.GetLifeMax(),ret)) if (ret[0] != null && ret[1] != null) phase = (int)ret[0];
			if (phase > 0) sb.Draw(ModWorld.texPhase[phase-1],new Vector2(xx,yy),Color.White);
			
			sb.Draw((Texture2D)rt,new Vector2(xx,yy),Color.White);
			string s = ""+npci.GetLife()+"/"+npci.GetLifeMax();
			DrawStringShadowed(sb,Main.fontMouseText,s,new Vector2(xx+52,yy+52),Color.White,Color.Black);
			DrawStringShadowed(sb,Main.fontMouseText,npci.GetName(),new Vector2(xx+52,yy+72),Color.White,Color.Black);
			
			xx += (int)(Math.Max(Main.fontMouseText.MeasureString(s).X,Main.fontMouseText.MeasureString(npci.GetName()).X)+68);
		}
	}
	
	private static void DrawStringShadowed(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, Color colorShadow, Vector2 origin = default(Vector2), float scale = 1f, SpriteEffects effects = SpriteEffects.None) {
		foreach (Vector2 vecOff in shadowOffset) sb.DrawString(font,text,new Vector2(pos.X+vecOff.X,pos.Y+vecOff.Y),colorShadow,0f,origin,scale,effects,0f);
		sb.DrawString(font,text,pos,color,0f,origin,scale,effects,0f);
	}
}