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

    private List<GameObject> cities;

    //Generating the cities themselves
    public void GenerateCities(int mapDepth, int mapWidth, MapData mapData)
    {
        cities = new List<GameObject>();
        
        //Want to generate as many cities as requested in the numberOfCities variable
        for (int cityCount = 0; cityCount < numberOfCities; cityCount++)
        {
            //Find a suitable spawn point for cities
            Vector3 citySpawn = ChooseCitySpawn(mapDepth, mapWidth, mapData);

            //Instantiate a city at the chosen spawn point
            GameObject city = Instantiate(cityPrefab, citySpawn, Quaternion.identity) as GameObject;

            //Name the city and display the name

            city.GetComponentInChildren<TextMesh>().text = GenerateCityName(citySpawn, mapData);

            //Add the city to the list of cities
            cities.Add(city);
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
            randomZIndex = Random.Range(0, mapDepth - (mapDepth / 5));
            randomXIndex = Random.Range(0, mapWidth - (mapWidth / 5));

            //Convert from map coordinate system to tile coordinate system and retrieve corresponding tile data
            TileCoordinate tileCoordinate = mapData.ConvertToTileCoordinate(randomZIndex, randomXIndex);
            TileData tileData = mapData.tilesData[tileCoordinate.tileZIndex, tileCoordinate.tileXIndex];

            //Ensure cities can't spawn in water (meaning they can't spawn in areas without a biome)
            if (tileData.chosenBiomes[tileCoordinate.tileZIndex, tileCoordinate.tileXIndex] != null)
            {
                Biome checkBiome = tileData.chosenBiomes[tileCoordinate.tileZIndex, tileCoordinate.tileXIndex];

                Vector3 checkPoint = new Vector3(randomXIndex, 5, randomZIndex);

                //Check radius for nearby cities
                if (cities.Count > 0)
                {
                    foreach(GameObject city in cities)
                    {
                        if (Mathf.Abs(Vector3.Distance(checkPoint, city.transform.position)) >= spawnRadius)
                        {
                            found = true;
                        }

                        else
                        {
                            found = false;
                            break;
                        }
                    }
                }

                else
                {
                    found = true;
                }
            }
        }

        return new Vector3(randomXIndex, 5, randomZIndex); //We use 5 as the y coordinate as the map is only to be viewed top-down, and so the perceived elevation is irrelevant. It just has to be above the map terrain.
    }

    public string GenerateCityName(Vector3 citySpawn, MapData mapData)
    {
        //Arrays for prefixes, middles, and suffixes of city names
        string[] savannaPrefix = new string[] { "Sa", "Sav", "San", "Su", "Za", "Zu" };
        string[] grasslandPrefix = new string[] { "Al", "An", "Ad" };
        string[] desertPrefix = new string[] { "Dak", "Das", "Dol" };
        string[] tundraPrefix = new string[] { "Ske", "Skja", "Sek" };
        string[] borealPrefix = new string[] { "Tor", "Tav", "Tul", "Tol" };
        string[] rainPrefix = new string[] { "Bra", "Ban", "Bol" };

        string[] middleWord = new string[] { "an", "ad", "va", "dar", "da", "la", "len", "liv", "ver", "vil", "bad", "cad", "dav" };

        string[] savannaSuffix = new string[] { "a", "ah", "o", "ya", "da" };
        string[] grasslandSuffix = new string[] { "ren", "id", "ds" };
        string[] desertSuffix = new string[] { "ba", "ca", "vo" };
        string[] tundraSuffix = new string[] { "a", "e", "oe" };
        string[] borealSuffix = new string[] { "on", "en", "ak", "adh" };
        string[] rainSuffix = new string[] { "go", "ga" };

        
        //Identify the biome the city is located in
        TileCoordinate tileCoordinate = mapData.ConvertToTileCoordinate((int)citySpawn.z, (int)citySpawn.x);
        TileData tileData = mapData.tilesData[tileCoordinate.tileZIndex, tileCoordinate.tileXIndex];

        Biome cityBiome = tileData.chosenBiomes[tileCoordinate.tileZIndex, tileCoordinate.tileXIndex];

        //Create the name
        string cityPrefix = "";
        string cityMiddle = "";
        string citySuffix = "";

        switch (cityBiome.name) //Check the biome name for the coordinate, then generate the prefix and suffix accordingly
        {
            case "Savanna":
                cityPrefix = savannaPrefix[Random.Range(0, savannaPrefix.Length)];
                citySuffix = savannaSuffix[Random.Range(0, savannaSuffix.Length)];
                break;

            case "Desert":
                cityPrefix = desertPrefix[Random.Range(0, desertPrefix.Length)];
                citySuffix = desertSuffix[Random.Range(0, desertSuffix.Length)];
                break;

            case "Grassland":
                cityPrefix = grasslandPrefix[Random.Range(0, grasslandPrefix.Length)];
                citySuffix = grasslandSuffix[Random.Range(0, grasslandSuffix.Length)];
                break;

            case "Tundra":
                cityPrefix = tundraPrefix[Random.Range(0, tundraPrefix.Length)];
                citySuffix = tundraSuffix[Random.Range(0, tundraSuffix.Length)];
                break;

            case "Boreal Forest":
                cityPrefix = borealPrefix[Random.Range(0, borealPrefix.Length)];
                citySuffix = borealSuffix[Random.Range(0, borealSuffix.Length)];
                break;

            case "Tropical Rainforest":
                cityPrefix = rainPrefix[Random.Range(0, rainPrefix.Length)];
                citySuffix = rainSuffix[Random.Range(0, rainSuffix.Length)];
                break;
        }

        cityMiddle = middleWord[Random.Range(0, middleWord.Length)];

        string cityName = cityPrefix + cityMiddle + citySuffix;

        return cityName;
    }
}
