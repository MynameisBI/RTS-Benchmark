using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : TeamObject
{
    public int counter = 1;
    [HideInInspector]
    public Trapper trapper;

    public void OnHit(Unit unit)
    {
        counter--;
        if (counter <= 0)
        {
            unit.TakeDamage(trapper.damage);
            Destroy(gameObject);
        }
    }
}
