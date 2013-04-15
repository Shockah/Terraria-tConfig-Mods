int level = 1;
int lastrecord=0;
public void EffectsStart(Player P,int buffIndex,int buffType,int buffTime) 
{
    lastrecord = buffTime-1;
}
public void Effects(Player P,int buffIndex,int buffType,int buffTime) 
{
    P.statDefense-=level;
    if(lastrecord <= buffTime)
    {
        lastrecord = buffTime;
        if(level < 30) level++;
    }
    Main.buffTip[buffType] = "Suffer from "+level+" reduced defense (max 30)";
}
