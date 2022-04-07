using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGeneration : MonoBehaviour
{
    //int to determine the number of cities on the map
    [SerializeField]
    private int numberOfCities;

    //float to set how close a city spawns to another
    [SerializeField] //Serialized so one can modify how close cities can be to eachother
    private int spawnRadius;

    [SerializeField]
    private GameObject cityPrefab;

    //Generating the cities themselves
    public void GenerateCities(int mapDepth, int mapWidth, MapData mapData)
    {
        //Want to generate as many cities as requested in the numberOfCities variable
        for (int cityCount = 0; cityCount < numberOfCities; cityCount++)
        {
            //Find a suitable spawn point for cities
            Vector3 citySpawn = ChooseCitySpawn(mapDepth, mapWidth, mapData);

            //Instantiate a city at the chosen spawn point
            GameObject city = Instantiate(cityPrefab, citySpawn, Quaternion.identity) as GameObject;
        }        
    }

    public Vector3 ChooseCitySpawn(int mapDepth, int mapWidth, MapData mapData)
    {
        bool found = false;
        int randomZIndex = 0;
        int randomXIndex = 0;

        //Iterate until find a good spawn point
        while (!found)
        {
            //Pick a random coordinate inside the map
            randomZIndex = Random.Range(0, mapDepth -22);
            randomXIndex = Random.Range(0, mapWidth -22);

            //Convert from map coordinate system to tile coordinate system and retrieve corresponding tile data
            TileCoordinate tileCoordinate = mapData.ConvertToTileCoordinate(randomZIndex, randomXIndex);
            TileData tileData = mapData.tilesData[tileCoordinate.tileZIndex, tileCoordinate.tileXIndex];

            //Ensure cities can't spawn in water (meaning they can't spawn in areas without a biome)
            if (tileData.chosenBiomes[tileCoordinate.tileZIndex, tileCoordinate.tileXIndex] != null)
            {
                Biome checkBiome = tileData.chosenBiomes[tileCoordinate.tileZIndex, tileCoordinate.tileXIndex];

                //Check radius for nearby cities


                found = true;
            }
        }

        return new Vector3(randomXIndex, 5, randomZIndex); //We use 5 as the y coordinate as the map is only to be viewed top-down, and so the perceived elevation is irrelevant. It just has to be above the map terrain.
    }
}
