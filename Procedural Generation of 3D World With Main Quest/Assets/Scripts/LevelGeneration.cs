using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    #region Variables

    [SerializeField]
    private int mapWidthInTiles, mapDepthInTiles;

    [SerializeField]
    private GameObject tilePrefab;

    [SerializeField]
    private float centerVertexZ, maxDistanceZ;

    [SerializeField]
    private RiverGeneration riverGeneration;

    [SerializeField]
    private CityGeneration cityGeneration;

    //Waves for noise generation in the height map, heat map and moisture map
    [SerializeField]
    private Wave[] heightWaves;

    [SerializeField]
    private Wave[] heatWaves;

    [SerializeField]
    private Wave[] moistureWaves;

    #endregion

    private void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        //Get the tile dimensions from the prefab
        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;

        //Calculate the number of vertices of the tile in each axis using its mesh
        Vector3[] tileMeshVertices = tilePrefab.GetComponent<MeshFilter>().sharedMesh.vertices;
        int tileDepthInVertices = (int)Mathf.Sqrt(tileMeshVertices.Length);
        int tileWidthInVertices = tileDepthInVertices;

        //Build an empty MapData object, to be filled with the generated tiles
        MapData mapData = new MapData(tileDepthInVertices, tileWidthInVertices, this.mapDepthInTiles, this.mapWidthInTiles, tileDepth, tileWidth);
        Debug.Log("About to randomise waves");
        //Randomise waves
        for (int i = 0; i < heightWaves.Length; i++)
        {
            heightWaves[i].seed = Random.Range(2500, 7500);
            heightWaves[i].amplitude = 1;
            heightWaves[i].frequency = 1;

            Debug.Log("Heightwave" + i + " randomised");
        }

        for (int i = 0; i < heatWaves.Length; i++)
        {
            heatWaves[i].seed = Random.Range(2500, 7500);
            heatWaves[i].amplitude = 1;
            heatWaves[i].frequency = 1;

            Debug.Log("Heatwave" + i + " randomised");
        }

        for (int i = 0; i < moistureWaves.Length; i++)
        {
            moistureWaves[i].seed = Random.Range(2500, 7500);
            moistureWaves[i].amplitude = 1;
            moistureWaves[i].frequency = 1;

            Debug.Log("Moisture wave" + i + " randomised");
        }

        //For each tile, instantiate a tile in the correct position
        for (int xTileIndex = 0; xTileIndex < mapWidthInTiles; xTileIndex++)
        {
            for (int zTileIndex = 0; zTileIndex < mapDepthInTiles; zTileIndex++)
            {
                //Calculate the tile position based on the X and Z indices
                Vector3 tilePosition = new Vector3(this.gameObject.transform.position.x + xTileIndex * tileWidth,
                    this.gameObject.transform.position.y,
                    this.gameObject.transform.position.z + zTileIndex * tileDepth);

                //Instantiate a new tile
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;

                //Generate the tile texture and save it in the map data
                TileData tileData = tile.GetComponent<TileGeneration>().GenerateTile(centerVertexZ, maxDistanceZ, heightWaves, heatWaves, moistureWaves);
                mapData.AddTileData(tileData, zTileIndex, xTileIndex);
            }
        }

        //Generate rivers for the map
        riverGeneration.GenerateRivers(this.mapDepthInTiles * tileDepthInVertices, this.mapWidthInTiles * tileWidthInVertices, mapData);

        //Generate cities in the map
        cityGeneration.GenerateCities(this.mapDepthInTiles * tileDepthInVertices, this.mapWidthInTiles * tileWidthInVertices, mapData);
    }
}

public class MapData
{
    private int tileDepthInVertices, tileWidthInVertices;

    public TileData[,] tilesData;

    public int tileDepth, tileWidth;

    public MapData(int tileDepthInVertices, int tileWidthInVertices, int mapDepthInTiles, int mapWidthInTiles, int tileDepth, int tileWidth)
    {
        //Build the tilesData matrix based on the map depth and width
        tilesData = new TileData[tileDepthInVertices * mapDepthInTiles, tileWidthInVertices * mapWidthInTiles];

        this.tileDepthInVertices = tileDepthInVertices;
        this.tileWidthInVertices = tileWidthInVertices;

        this.tileDepth = tileDepth;
        this.tileWidth = tileWidth;
    }

    public void AddTileData(TileData tileData, int tileZIndex, int tileXIndex)
    {
        //Save the tile data in the corresponding coordinate
        tilesData[tileZIndex, tileXIndex] = tileData;
    }

    public TileCoordinate ConvertToTileCoordinate(int zIndex, int xIndex)
    {
        //The tile index is calculated by dividing the index by the number of tiles in that axis
        int tileZIndex = (int)Mathf.Floor((float)zIndex / (float)this.tileDepthInVertices);
        int tileXIndex = (int)Mathf.Floor((float)xIndex / (float)this.tileWidthInVertices);

        //The coordinate index is calculated by getting the remainder of the division above
        //We also need to translate the origin to the bottom left corner
        int coordinateZIndex = this.tileDepthInVertices - (zIndex % this.tileDepthInVertices) - 1;
        int coordinateXIndex = this.tileWidthInVertices - (xIndex % this.tileDepthInVertices) - 1;

        TileCoordinate tileCoordinate = new TileCoordinate(tileZIndex, tileXIndex, coordinateZIndex, coordinateXIndex);
        return tileCoordinate;
    }
}

public class TileCoordinate
{
    //Class to represent a coordinate in the tile coordinate system
    /*Due to the way tiles are generated, each tile's coordinates are confused and inconsistent with Unity's built in coordinate system
    as I generate tiles beside previous ones and then vertices from the top right corner back to the bottom left.
    This class should change the way coordinates are accessed in each tile and make the system better compatible with Unity and therefore
    easier to work with.*/

    public int tileZIndex;
    public int tileXIndex;
    public int coordinateZIndex;
    public int coordinateXIndex;

    public TileCoordinate(int importTileZIndex, int importTileXIndex, int importCoordinateZIndex, int importCoordinateXIndex)
    {
        this.tileZIndex = importTileZIndex;
        this.tileXIndex = importTileXIndex;
        this.coordinateZIndex = importCoordinateZIndex;
        this.coordinateXIndex  = importCoordinateXIndex;
    }
}
