using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGeneration : MonoBehaviour
{
    //Function to generate a noise map using Perlin noise. Creates a float at each coordinate on the terrain
    //as a number between 0 and 1 to give that coordinate a random height.

    public float[,] GeneratePerlinNoiseMap(int mapDepth, int mapWidth, float scale, float offsetX, float offSetZ, Wave[] waves)
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
                float sampleX = (xIndex + offsetX) / scale;
                float sampleZ = (zIndex + offSetZ) / scale;

                //Code for single Perlin noise wave

                ////Generate noise value using perlin noise
                //float noise = Mathf.PerlinNoise(sampleX, sampleZ);
                //noiseMap[zIndex, xIndex] = noise;

                //Using multiple waves
                float noise = 0f;
                float normalization = 0f;

                foreach (Wave wave in waves)
                {
                    //Generate noise value using Perlin Noise for a given wave
                    noise += wave.amplitude * Mathf.PerlinNoise(sampleX * wave.frequency + wave.seed, sampleZ * wave.frequency + wave.seed);
                    normalization += wave.amplitude;
                }

                //Normalise the noise value so that it is within 0 and 1
                noise /= normalization;

                noiseMap[zIndex, xIndex] = noise;

            }
        }

        //Return the generated coordinates
        return noiseMap;
    }

    //Function to generate a noise map for heat distribution for generating biomes. Using perlin nosie again to simulate uneven distribution (as is present in the real world)
    //Z coordinate required this time to find the middle of the world and distribute heat accordingly (think hemispheres on Earth)
    public float[,] GenerateUniformNoiseMap (int mapDepth, int mapWidth, float centerVertexZ, float maxDistanceZ, float offsetZ)
    {
        //Create an empty noise map with mapDepth and mapWidth coordinates
        float[,] noiseMap = new float[mapDepth, mapWidth];

        for (int zIndex = 0; zIndex < mapDepth; zIndex++)
        {
            //Calculate the sampleZ by summing the index and the offset
            float sampleZ = zIndex + offsetZ;

            //Calculate the noise proportional to the distance of the sample to the center of the level
            float noise = Mathf.Abs(sampleZ - centerVertexZ) / maxDistanceZ;

            //Apply the noise for all points with this Z coordinate
            for (int xIndex = 0; xIndex < mapWidth; xIndex++)
                noiseMap[mapDepth - zIndex - 1, xIndex] = noise;
        }

        return noiseMap;
    }
}

[System.Serializable]
public class Wave
{
    //Wave class serves to add complexity to the generated terrain and increase randomness

    public float seed;
    public float frequency;
    public float amplitude;
}
