public bool CanUse(Player P,int i)
{
    P.statLife-=3;
    if(P.statLife < 1) P.statLife = 1;
    return true;
}
public void DamageNPC(Player P, NPC N, ref int dmg, ref float kb)
{
    if(Main.rand.Next(10) != 0) return;
    dmg*=5;
}