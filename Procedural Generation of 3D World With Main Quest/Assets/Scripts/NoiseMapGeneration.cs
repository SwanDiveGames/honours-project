using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGeneration : MonoBehaviour
{
    //Function to generate a noise map using Perlin noise. Creates a float at each coordinate on the terrain
    //as a number between 0 and 1 to give that coordinate a random height.

    public float[,] GenerateNoiseMap(int mapDepth, int mapWidth, float scale)
    {
        //Create an empty noise map with the mapDepth and mapWidth coordinates (Z and X respectively)
        float[,] noiseMap = new float[mapDepth, mapWidth];

        //Simple for loop to iterate each (X, Z) co-ordinate to apply a noise value.
        //Assigns a Z value, then iterates through all X values, then iterates Z and repeats.
        for (int zIndex = 0; zIndex < mapDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < mapWidth; xIndex++)
            {
                //Calculate sample indices based on the coordinates and scale
                float sampleX = xIndex / scale;
                float sampleZ = zIndex / scale;

                //Generate noise value using perlin noise
                float noise = Mathf.PerlinNoise(sampleX, sampleZ);
                noiseMap[zIndex, xIndex] = noise;

            }
        }

        //Return the generated coordinates
        return noiseMap;
    }
}
