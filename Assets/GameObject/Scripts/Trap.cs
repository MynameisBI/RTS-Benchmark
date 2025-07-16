using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : GridObject
{
    public int damage = 10;
    public int counter = 1;

    public void OnHit(Unit unit)
    {
        counter--;
        if (counter <= 0)
        {
            Destroy(gameObject);
        }
    }
}
