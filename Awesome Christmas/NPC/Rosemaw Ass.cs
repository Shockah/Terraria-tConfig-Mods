public class ObserverEye
{
    NPC N;
    public Color c1,c2;
    public float s1,s2;
    public int framer;
    public int del;
    public float allowed;
    Vector2 Pos;
    Vector2 Pos2; //for c1
    Vector2 Off;
    public ObserverEye(NPC Ns,Color A,Color B,float C,float D,int E,float F)
    {
        N = Ns;
        c1=A;
        c2=B;
        s1=C;
        s2=D;
        framer = E;
        del = framer;
        allowed = F;
    }

    public void Update()
    {
        del--;
        if(del == 0)
        {
            float p = (float)(Main.rand.NextDouble()*Math.PI*2f);
            Off = new Vector2((float)Math.Cos(p),(float)Math.Sin(p))*allowed*(float)Main.rand.NextDouble();
            del = framer;
        }
        Pos = Vector2.Lerp(Pos,Off,0.1f);
        int a = Player.FindClosest(N.position,N.width,N.height);
        Player P = Main.player[a];
        Vector2 PC = P.position+new Vector2(P.width,P.height)/2f;
        Vector2 NC = N.position+new Vector2(N.width,N.height)/2f;
        Vector2 TarVec = PC-(NC+Pos2+Pos);
        if(TarVec.Length() > 12.5f*s1) TarVec*=(12.5f*s1)/TarVec.Length();
        Pos2 = Vector2.Lerp(Pos2,TarVec,0.1f);
    }

    public void Draw(SpriteBatch SP)
    {
        Texture2D TEX = Main.goreTexture[Config.goreID["Observing eye"]];
        Vector2 NC = N.position+new Vector2(N.width,N.height)/2f - Main.screenPosition;
        Vector2 TC = new Vector2(TEX.Width,TEX.Height)/2f;
        SP.Draw(TEX,NC-TC*s1+Pos,new Rectangle(0,0,TEX.Width,TEX.Height),c1,0f,default(Vector2),s1,SpriteEffects.None,0f);
        SP.Draw(TEX,NC-TC*s2+Pos-Pos2,new Rectangle(0,0,TEX.Width,TEX.Height),c2,0f,default(Vector2),s2,SpriteEffects.None,0f);
    }
}

public ObserverEye ob1;
public ObserverEye ob2;
public ObserverEye ob3;

public bool FirstFrame = true; //change to true

#region Spawn NPC


public bool SpawnNPC(int x, int y, int playerID)
{
	return false;
}


#endregion


#region Initialize

public void Initialize()
{
    npc.netAlways = true;
    NPC N = npc;
}


#endregion


#region AI

public bool PreAI()
{
    NPC N = npc;
    return true;
}

public void PostAI()
{
    NPC N = npc;
}

/// ai explain
/// 0 is stage
/// 1 is owner
/// 2 is
/// 3 is
/// 

public void AI()
{
    NPC N = npc;
    Player PT = Main.player[N.target];
    Vector2 NC = N.position+new Vector2(N.width/2,N.height/2);
    Vector2 PTC = PT.position+new Vector2(PT.width/2,PT.height/2);

    NPC M = Main.npc[(int)N.ai[1]];
    Vector2 MC = M.position+new Vector2(M.width/2f,M.height/2f);
    N.spriteDirection = -1;
    N.displayName = M.displayName;
    
    //if(FirstFrame)
    //{
    //    if(!Main.dedServ) ob1 = new ObserverEye(N,Color.White,Color.Indigo,0.4f,0.5f,160,60f);
    //    if(!Main.dedServ) ob2 = new ObserverEye(N,Color.LightCoral,Color.Black,0.7f,1f,140,57f);
    //    if(!Main.dedServ) ob3 = new ObserverEye(N,Color.DeepPink,Color.DarkViolet,0.5f,0.8f,90,90f);
    //    FirstFrame = false;
    //}

    if(!M.active || M.type != Config.npcDefs.byName["Rosemaw"].type)
    {
		N.life = -1;
		N.HitEffect(0, 10.0);
		N.active = false;
    }
    N.realLife = (int)N.ai[1];
    N.position = MC-(NC-N.position)-new Vector2(0,M.height);
    
    if(N.ai[0] < 1)
    {
        if(M.ai[0] >= 2)
        {
            N.ai[0] = 1;
        }
    }
    if(N.ai[0] < 2)
    {
        if(M.ai[0] >= 3)
        {
            N.ai[0] = 2;
            
        }
    }
    if(N.ai[0] < 3 && M.ai[0] >= 3)
    {
        N.ai[0] = 3;
        if(Main.netMode == 1) return;
        int babies = 15; //a number dividable by 300 is preferable
        for(int i = 0; i < babies; i++) 
        {
            int poor = NPC.NewNPC((int)NC.X,(int)NC.Y,"Rosemaw Spiderling");
            NPC Z = Main.npc[poor];
            Z.velocity = new Vector2((float)Math.Cos(i),(float)Math.Sin(i))*5f;
            Z.ai[0] = 300f*(float)Config.syncedRand.NextDouble();
            Z.ai[3] = 300f*(float)Config.syncedRand.NextDouble();
        }
    }
    if(N.ai[0] > 0) N.ai[2]++;
    if(N.ai[0] > 0 && N.ai[2] == 480)
    {
        N.ai[2] = 0;
        if(Main.netMode != 1)
        {
            for(int i = 0; i < Main.player.Length; i++)
            {
                Player Z = Main.player[i];
                if(!Z.active) continue;
                if(Z.dead) continue;
                Vector2 ZC = new Vector2(Z.width,Z.height)/2f +Z.position;
                if(Vector2.Distance(ZC,NC) < 400f && i != N.target) continue;
                
                Vector2 SP = ZC + new Vector2((200f+300f*(float)Config.syncedRand.NextDouble())*(((float)Config.syncedRand.Next(2) - 0.5f)*2f),-600f);
                Vector2 Velo = (ZC-SP);
                Velo.Normalize();
                Velo*=7f;
                Projectile.NewProjectile(SP.X,SP.Y,Velo.X,Velo.Y,"Big Icicle",30,2f,Main.myPlayer);
                for(int d = 0; d < 3; d++)
                {
                    SP = ZC + new Vector2((200f+300f*(float)Config.syncedRand.NextDouble())*(((float)Config.syncedRand.Next(2) - 0.5f)*2f),-600f);
                    Velo = (ZC-SP);
                    Velo.Normalize();
                    Velo*=7f;
                    Velo+=new Vector2(Config.syncedRand.Next(-40,41)*0.1f,Config.syncedRand.Next(-40,41)*0.1f);
                    Projectile.NewProjectile(SP.X,SP.Y,Velo.X,Velo.Y,"Small Icicle",30,2f,Main.myPlayer);
                
                }
            }
        }
    }
    if(N.ai[0] == 3) N.ai[3]++;
    if(N.ai[0] == 3 && N.ai[3] == 1200)
    {
        N.ai[3] = 0;
        if(Main.netMode == 1) return;
        int babies = 3; //a number dividable by 300 is preferable
        for(int i = 0; i < babies; i++) 
        {
            int poor = NPC.NewNPC((int)NC.X,(int)NC.Y,"Rosemaw Spiderling");
            NPC Z = Main.npc[poor];
            Z.velocity = new Vector2((float)Math.Cos(i),(float)Math.Sin(i))*5f;
            Z.ai[0] = 300f*(float)Config.syncedRand.NextDouble();
            Z.ai[3] = 300f*(float)Config.syncedRand.NextDouble();
        }
    }
}


#endregion

#region Find Frame


public void FindFrame(int currentFrame)
{
    NPC N = npc;
    NPC M = Main.npc[(int)N.ai[1]];
    Vector2 NC = N.position+new Vector2(N.width/2,N.height/2);

    if (N.velocity.X < 0)
    {
    N.direction = -1;
    N.spriteDirection = -1;
    }
    else
    {
    N.direction = 1;
    N.spriteDirection = 1;
    }

    int num = Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type];
    int num2 = (int)N.ai[0];
    if(num2 > 2) num2 = 2; if(num2 < 0) num2 = 0;
    N.frame.Y =  num * (num2/2);
    #region frame switching
    //N.frameCounter += 1.0;
    //if (N.frameCounter >= 4.0)
    //{
    //    N.frame.Y = N.frame.Y + num;
    //    N.frameCounter = 0.0;
    //}
    //if (N.frame.Y >= num * Main.npcFrameCount[N.type])
    //{
    //    N.frame.Y = 0;
    //}
    #endregion
}


#endregion


#region Dealt NPC


public void DealtNPC(double DMG, Player P)
{

}


#endregion


#region NPC Loot


public void NPCLoot()
{
}


#endregion


#region postdraw
//public void PostDraw(SpriteBatch SP)
//{
//    if(!Main.gamePaused)
//    {
//        ob1.Update();
//        ob2.Update();
//        ob3.Update();
//    }
//    ob1.Draw(SP);
//    ob2.Draw(SP);
//    ob3.Draw(SP);
//}
#endregion