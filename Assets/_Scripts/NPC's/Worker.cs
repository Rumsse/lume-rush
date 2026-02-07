using UnityEngine;

public class Worker : Character
{
    public override int CraftItemQuality()
    {
        var baseQuality = base.CraftItemQuality();
        var boosted = baseQuality + 1;
        if (boosted > 10) boosted = 10;
        return boosted;
    }


}

