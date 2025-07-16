using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : GridObject
{
    public int amount = 10;

    public bool Extract()
    {
        if (amount > 0)
        {
            amount--;
            return true;
        }
        Destroy(gameObject);
        return false;
    }
}
