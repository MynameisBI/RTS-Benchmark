using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : TeamObject
{
    private void OnDestroy()
    {
        gameManager.RemoveBuilding(this);
    }
}
