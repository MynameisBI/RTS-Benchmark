using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : GridObject
{
    GameManager gameManager;

    private void Awake()
    {
        team = -1;
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {

    }


}
