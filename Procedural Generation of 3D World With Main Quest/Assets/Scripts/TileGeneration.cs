using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TerrainType
{
    public string name;
    public float height;
    public Color colour;
}
public class TileGeneration : MonoBehaviour
{
    #region Variables
    //Terrain Types
    [SerializeField]
    private TerrainType[] terrainTypes;

    //Noise Map Generator
    [SerializeField]
    NoiseMapGeneration noiseMapGeneration;

    [SerializeField]
    private MeshRenderer tileRenderer;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    [SerializeField]
    private float mapScale;

    #endregion

    private void Start()
    {
        GenerateTile();
    }

    void GenerateTile()
    {
        //Calculate the tile depth and width based on the mesh vertices
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileDepth;

        //Calculate the offsets based on the tile position
        float[,] heightMap = this.noiseMapGeneration.GenerateNoiseMap(tileDepth, tileWidth, this.mapScale);

        //Generate a heightmap using noise
        Texture2D tileTexture = BuildTexture(heightMap);
        this.tileRenderer.material.mainTexture = tileTexture;
    }

    private Texture2D BuildTexture(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        Color[] colourMap = new Color[tileDepth * tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                //Transform the 2D map index into an array index
                int colourIndex = zIndex * tileWidth + xIndex;
                float height = heightMap[zIndex, xIndex];

                //Choose a Terrain Type based on the height value
                TerrainType terrainType = ChooseTerrainType(height);

                //Assign the colour according to the terrain type
                colourMap[colourIndex] = terrainType.colour;
            }
        }

        //Create new texture and set its pixel colours
        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colourMap);
        tileTexture.Apply();

        return tileTexture;

    }

    TerrainType ChooseTerrainType (float height)
    {
        //for each terrain type, check if the height is lower than the terrain's height
        foreach (TerrainType terrainType in terrainTypes)
        {
            //return the first terrain type whose height is higher than the generated one
            if (height < terrainType.height)
            {
                return terrainType;
            }
        }

        return terrainTypes[terrainTypes.Length - 1];
    }
}


