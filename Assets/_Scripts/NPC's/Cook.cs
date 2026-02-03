using UnityEngine;

public class Cook : Character
{

    public override int ProduceFoodUnits()
    {
        var baseUnits = base.ProduceFoodUnits();
        return baseUnits + 1;
    }


}
