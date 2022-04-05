using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverGeneration : MonoBehaviour
{
    [SerializeField]
    private int numberOfRivers;

    [SerializeField]
    private float heightThreshold;

    [SerializeField]
    private Color riverColour;

    public void GenerateRivers(int mapDepth, int mapWidth, MapData mapData)
    {
        for (int riverIndex = 0; riverIndex < numberOfRivers; riverIndex++)
        {
            //Choose the origin point for the river
            Vector3 riverOrigin = ChooseRiverOrigin(mapDepth, mapWidth, mapData);
            Debug.Log(riverOrigin.ToString());
            //Build the river starting from the origin and proceeding downwards
            BuildRiver(mapDepth, mapWidth, riverOrigin, mapData);
        }
    }

    private Vector3 ChooseRiverOrigin(int mapDepth, int mapWidth, MapData mapData)
    {
        bool found = false;
        int randomZIndex = 0;
        int randomXIndex = 0;

        //Iterate until find a good river origin
        while (!found)
        {
            //Pick a random coordinate inside the map
            randomZIndex = Random.Range(0, mapDepth);
            randomXIndex = Random.Range(0, mapWidth);

            //Convert from map coordinate system to tile coordinate system and retrieve corresponding tile data
            TileCoordinate tileCoordinate = mapData.ConvertToTileCoordinate(randomZIndex, randomXIndex);
            TileData tileData = mapData.tilesData[tileCoordinate.tileZIndex, tileCoordinate.tileXIndex];

            //If the height value of this coordinate is higher than the threshold, choose it as the river origin
            float heightValue = tileData.heightMap[tileCoordinate.coordinateZIndex, tileCoordinate.coordinateXIndex];

            if (heightValue >= this.heightThreshold)
            {
                found = true;
            }
        }

        return new Vector3(randomXIndex, 0, randomZIndex);
    }

    private void BuildRiver(int mapDepth, int mapWidth, Vector3 riverOrigin, MapData mapData)
    {
        HashSet<Vector3> visitedCoordinates = new HashSet<Vector3>();

        // the first coordinate is the river origin
        Vector3 currentCoordinate = riverOrigin;
        bool foundWater = false;

        while (!foundWater)
        {
            //Convert from map Coordinate System to Tile Coordinate System and retrieve the corresponding TileData
            TileCoordinate tileCoordinate = mapData.ConvertToTileCoordinate((int)currentCoordinate.z, (int)currentCoordinate.x);
            TileData tileData = mapData.tilesData[tileCoordinate.tileZIndex, tileCoordinate.tileXIndex];

            //Save the current coordinate as visited
            visitedCoordinates.Add(currentCoordinate);

            //Check if we have found water
            if (tileData.chosenHeightTerrainTypes[tileCoordinate.coordinateZIndex, tileCoordinate.coordinateXIndex].name == "Water")
            {
                //If we found water, stop
                foundWater = true;
            }
            else
            {
                //Change the texture of the tileData to show a river
                tileData.texture.SetPixel(tileCoordinate.coordinateXIndex, tileCoordinate.coordinateZIndex, this.riverColour);
                tileData.texture.Apply();

                //Pick neighbour coordinates, if they exist
                List<Vector3> neighbours = new List<Vector3>();
                if (currentCoordinate.z > 0)
                {
                    neighbours.Add(new Vector3(currentCoordinate.x, 0, currentCoordinate.z - 1));
                }
                if (currentCoordinate.z < mapDepth - 1)
                {
                    neighbours.Add(new Vector3(currentCoordinate.x, 0, currentCoordinate.z + 1));
                }
                if (currentCoordinate.x > 0)
                {
                    neighbours.Add(new Vector3(currentCoordinate.x - 1, 0, currentCoordinate.z));
                }
                if (currentCoordinate.x < mapWidth - 1)
                {
                    neighbours.Add(new Vector3(currentCoordinate.x + 1, 0, currentCoordinate.z));
                }

                //Find the minimum neighbour that has not been visited yet and flow to it
                float minHeight = float.MaxValue;
                Vector3 minNeighbour = new Vector3(0, 0, 0);
                foreach (Vector3 neighbour in neighbours)
                {
                    //Convert from map Coordinate System to Tile Coordinate System and retrieve the corresponding TileData
                    TileCoordinate neighbourTileCoordinate = mapData.ConvertToTileCoordinate((int)neighbour.z, (int)neighbour.x);
                    TileData neighbourTileData = mapData.tilesData[neighbourTileCoordinate.tileZIndex, neighbourTileCoordinate.tileXIndex];

                    //If the neighbour is the lowest one and has not been visited yet, save it
                    float neighbourHeight = tileData.heightMap[neighbourTileCoordinate.coordinateZIndex, neighbourTileCoordinate.coordinateXIndex];
                    if (neighbourHeight < minHeight && !visitedCoordinates.Contains(neighbour))
                    {
                        minHeight = neighbourHeight;
                        minNeighbour = neighbour;
                    }
                }
                // flow to the lowest neighbour
                currentCoordinate = minNeighbour;
            }
        }
    }
}
