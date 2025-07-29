using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [HideInInspector]
    public int[] teamResources;
    public int teamNum = 2;
    public int startingResources = 20;

    private void Start()
    {
        teamResources = new int[teamNum];
        for (int i = 0; i < teamNum; i++)
        {
            teamResources[i] = startingResources;
        }
    }

    public void AddResource(int team, int amount)
    {
        if (team < 1 || team > teamNum)
        {
            Debug.LogError("Invalid team number: " + team);
            return;
        }

        teamResources[team-1] += amount;
    }

    public int GetResourceAmount(int team)
    {
        if (team < 1 || team > teamNum)
        {
            Debug.LogError("Invalid team number: " + team);
            return -1;
        }
        return teamResources[team-1];
    }
}
