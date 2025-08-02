using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamObject : GridObject
{
    [Header("Team Object Properties")]
    public int team;

    protected GameManager gameManager;

    protected void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
}
