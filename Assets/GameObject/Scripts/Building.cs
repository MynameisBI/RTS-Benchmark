using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : TeamObject
{
    private new void OnDestroy()
    {
        base.OnDestroy();

        gameManager.RemoveBuilding(this);
    }
}
