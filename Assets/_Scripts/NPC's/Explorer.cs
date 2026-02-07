using UnityEngine;

public class Explorer : Character
{

    public override int CarryCapacity()
    {
        var baseCap = base.CarryCapacity();
        return baseCap + 3;
    }

}

