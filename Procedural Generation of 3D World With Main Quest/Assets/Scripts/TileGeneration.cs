using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TileGeneration : MonoBehaviour
{

    #region Variables
    //Terrain Types
    [SerializeField]
    private TerrainType[] heightTerrainTypes;

    [SerializeField]
    private TerrainType[] heatTerrainTypes;

    [SerializeField]
    private TerrainType[] moistureTerrainTypes;

    [SerializeField]
    private VisualizationMode visualizationMode;

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

    //Waves for noise generation in the height map, heat map and moisture map
    [SerializeField]
    private Wave[] heightWaves;

    [SerializeField]
    private Wave[] heatWaves;

    [SerializeField]
    private Wave[] moistureWaves;

    [SerializeField]
    private BiomeRow[] biomes;

    [SerializeField]
    private Color waterColour;

    [SerializeField]
    private float heightMultiplier;

    [SerializeField]
    private AnimationCurve heightCurve;

    [SerializeField]
    private AnimationCurve heatCurve;

    [SerializeField]
    private AnimationCurve moistureCurve;

    #endregion


    public void GenerateTile(float centerVertexZ, float maxDistanceZ)
    {
        //Calculate the tile depth and width based on the mesh vertices
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileDepth;

        //Calculate the offsets based on the tile position
        float offsetX = -this.gameObject.transform.position.x;
        float offsetZ = -this.gameObject.transform.position.z;

        //Generate heightmap using perlin noise
        float[,] heightMap = this.noiseMapGeneration.GeneratePerlinNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ, this.heightWaves);

        //Calculate vertex offset based on the tile position and the distance between vertices
        Vector3 tileDimensions = this.meshFilter.mesh.bounds.size;
        float distanceBetweenVertices = tileDimensions.z / (float)tileDepth;
        float vertexOffsetZ = this.gameObject.transform.position.z / distanceBetweenVertices;

        //Generate a Heatmap using uniform noise
        float[,] uniformHeatmap = this.noiseMapGeneration.GenerateUniformNoiseMap(tileDepth, tileWidth, centerVertexZ, maxDistanceZ, vertexOffsetZ);

        //Generate a Heatmap using Perlin noise
        float[,] randomHeatMap = this.noiseMapGeneration.GeneratePerlinNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ, this.heatWaves);
        float[,] heatMap = new float[tileDepth, tileWidth];

        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                //Mix both heatmaps together by multiplying their values
                heatMap[zIndex, xIndex] = uniformHeatmap[zIndex, xIndex] * randomHeatMap[zIndex, xIndex];

                //Make higher regions colder by adding the height value to the heatmap
                heatMap[zIndex, xIndex] += this.heatCurve.Evaluate(heightMap[zIndex, xIndex] * heightMap[zIndex, xIndex]);
            }
        }

        //Generate a moisture map using Perlin noise
        float[,] moistureMap = this.noiseMapGeneration.GeneratePerlinNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ, this.moistureWaves);
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                //makes higher regions drier by reducing the height value from the heat map
                moistureMap[zIndex, xIndex] -= this.moistureCurve.Evaluate(heightMap[zIndex, xIndex] * heightMap[zIndex, xIndex]);
            }
        }

        //Build a texture from the height map
        TerrainType[,] chosenHeightTerrainTypes = new TerrainType[tileDepth, tileWidth];
        Texture2D heightMapTexture = BuildTexture(heightMap, this.heightTerrainTypes, chosenHeightTerrainTypes);

        //Build a texture from the heat map
        TerrainType[,] chosenHeatTerrainTypes = new TerrainType[tileDepth, tileWidth];
        Texture2D heatMapTexture = BuildTexture(heatMap, this.heatTerrainTypes, chosenHeatTerrainTypes);

        //Build a texture from the moisture map
        TerrainType[,] chosenMoistureTerrainTypes = new TerrainType[tileDepth, tileWidth];
        Texture2D moistureMapTexture = BuildTexture(moistureMap, this.moistureTerrainTypes, chosenMoistureTerrainTypes);

        //Build a biomes texture from the three other noise variables
        Texture2D biomesTexture = BuildBiomeTexture(chosenHeightTerrainTypes, chosenHeightTerrainTypes, chosenMoistureTerrainTypes);

        switch(this.visualizationMode)
        {
            case VisualizationMode.Height:
                //Assign material texture to be the height texture
                this.tileRenderer.material.mainTexture = heightMapTexture;
                break;

            case VisualizationMode.Heat:
                //Assign material texture to be the heat texture
                this.tileRenderer.material.mainTexture = heatMapTexture;
                break;

            case VisualizationMode.Moisture:
                //Assign material texture to be the moisture texture
                this.tileRenderer.material.mainTexture = moistureMapTexture;
                break;

            case VisualizationMode.Biome:
                //Assign material texture to be the biome texture
                this.tileRenderer.material.mainTexture = biomesTexture;
                break;
        }

        //Update the tile mesh vertices according to the height map
        UpdateMeshVertices(heightMap);
    }

    private Texture2D BuildTexture(float[,] heightMap, TerrainType[] terrainTypes, TerrainType[,] chosenterrainTypes)
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
                TerrainType terrainType = ChooseTerrainType(height, terrainTypes);

                //Assign the colour according to the terrain type
                colourMap[colourIndex] = terrainType.colour;

                //Save the chosen terrain type
                chosenterrainTypes[zIndex, xIndex] = terrainType;
            }
        }

        //Create new texture and set its pixel colours
        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colourMap);
        tileTexture.Apply();

        return tileTexture;

    }

    TerrainType ChooseTerrainType (float height, TerrainType[] terrainTypes)
    {
        //for each terrain type, check if the height is lower than the terrain's height
        foreach (TerrainType terrainType in heightTerrainTypes)
        {
            //return the first terrain type whose height is higher than the generated one
            if (height < terrainType.threshold)
            {
                return terrainType;
            }
        }

        return heightTerrainTypes[heightTerrainTypes.Length - 1];
    }

    #region Update Mesh Vertices
    private void UpdateMeshVertices(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        Vector3[] meshVertices = this.meshFilter.mesh.vertices;

        //iterate through all the heightMap coordinates, updating the vertex index
        int vertexIndex = 0;
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                float height = heightMap[zIndex, xIndex];

                Vector3 vertex = meshVertices[vertexIndex];

                //Change the vertex Y coordinate, proportional to the height value
                meshVertices[vertexIndex] = new Vector3(vertex.x, this.heightCurve.Evaluate(height) * this.heightMultiplier, vertex.z);

                vertexIndex++;
            }
        }

        //Update the vertices in the mesh and update its properties
        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();

        //Update the mesh collider
        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }

    #endregion

    private Texture2D BuildBiomeTexture(TerrainType[,] heightTerrainTypes, TerrainType[,] heatTerrainTypes, TerrainType[,] moistureTerrainTypes)
    {
        int tileDepth = heatTerrainTypes.GetLength(0);
        int tileWidth = heatTerrainTypes.GetLength(1);

        Color[] colourMap = new Color[tileDepth * tileWidth];

        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                int colourIndex = zIndex * tileWidth + xIndex;

                TerrainType heightTerrainType = heightTerrainTypes[zIndex, xIndex];

                //Check if the current coordinate is a water region
                if (heightTerrainType.name != "water")
                {
                    //If a coordinate is not water, its biome will be defined by the heat and mositure values
                    TerrainType heatTerrainType = heatTerrainTypes[zIndex, xIndex];
                    TerrainType moistureTerrainType = moistureTerrainTypes[zIndex, xIndex];

                    //Terrain type index is used to access the biomes table
                    Biome biome = this.biomes[moistureTerrainType.index].biomes[heatTerrainType.index];

                    //Assign the colour according to the selected biome
                    colourMap[colourIndex] = biome.colour;
                }

                else
                {
                    //Water regions don't have biomes, they're simply water coloured
                    colourMap[colourIndex] = this.waterColour;
                }
            }
        }

        //Create a new texture and set its pixel colours
        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colourMap);
        tileTexture.Apply();

        return tileTexture;
    }
}

[System.Serializable]
public class TerrainType
{
    public string name;
    public float threshold;
    public Color colour;
    public int index;
}

[System.Serializable]
public class Biome
{
    public string name;
    public Color colour;
}

[System.Serializable]
public class BiomeRow
{
    public Biome[] biomes;
}

enum VisualizationMode { Height, Heat, Moisture, Biome }

