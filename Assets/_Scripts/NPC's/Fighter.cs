using UnityEngine;

public class Fighter : Character
{
    public override int AttackDamage()
    {
        var baseDmg = base.AttackDamage();
        return baseDmg + 5;
    }
}

