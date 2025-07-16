using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Unit
{
    protected new void Awake()
    {
        
    }

    protected new void Start()
    {
        base.Start();
    }

    protected new void Update()
    {

    }

    private void OnDestroy()
    {
        gameManager.RemoveBuilding(this);
    }
}
