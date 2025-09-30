using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/*
 * This is the basis for a procedual map generator
 *  Probably not going to use this
 *  Interesting to maybe do.
 */
public class MapManager : MonoBehaviour
{
    [SerializeField] private int[,] map;
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private bool testing = true;

    [SerializeField] Dictionary<int, GameObject> tiletable = new Dictionary<int, GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Start()
    {
        map = null;
        NetworkManager.Singleton.OnServerStarted += DetermineTesting;

    }
    void DetermineTesting()
    {
        Debug.Log("Hello");
        if (!NetworkManager.Singleton.IsServer) return;
        
        if (testing)
        {
            MakeTestingMap();
            Debug.Log("Hello2");
        }
        else
        {
            MakeMap();
        }

        SpawnTiles();
    }

    void MakeTestingMap()
    {
        //If theres already a map generated, don't change (something probably went wrong)
        if (map != null) return;

        sizeX = 10;
        sizeY = 10;
        map = new int[sizeX, sizeY];
        map[8, 1] = 1;
        map[8, 2] = 1;
        map[8, 3] = 1;
        map[8, 4] = 1;
        map[8, 5] = 1;
        map[8, 6] = 1;
        map[8, 7] = 1;

        map[1, 5] = 1;
        map[2, 3] = 1;
        map[2, 8] = 1;
        map[3, 2] = 1;
        map[3, 9] = 1;
    }

    
    void MakeMap()
    {
        //If theres already a map generated, don't change (something probably went wrong)
        if (map != null) return;
        map = new int[sizeX, sizeY];

        //Either read from file or hardcode the map.

        ////Realistically, i should learn how to use the tilemap
    }

    void SpawnTiles()
    {
        for (int i = 0; i < sizeX; i++)
        {
            for(int j = 0; j < sizeY; j++)
            {

                //Very Very Very basic, if 1, place tile, if anything else, place nothing.
                //Use dictionary when this gets more expansive
                if (map[i,j] == 1)
                {
                    Vector2 Location = new Vector2(i,j);
                    Location.x += transform.position.x;
                    Location.y += transform.position.y;
                    GameObject wall = Instantiate(wallPrefab, Location, Quaternion.identity);

                    wall.GetComponent<NetworkObject>().Spawn(true);
                    Debug.Log("wtf");


                }
            }
        }
    }
}
