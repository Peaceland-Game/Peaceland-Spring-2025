using UnityEngine;

public static class DynamicCutHelper
{
    /// <summary>
    /// Converts a 1D array to a 2D array
    /// </summary>
    /// <typeparam name="T">Generic data type</typeparam>
    /// <param name="input">The original array</param>
    /// <param name="width">Desired width of the 2D array</param>
    /// <param name="height">Desired height of the 2D array</param>
    /// <returns></returns>
    public static Color[,] Make2DArrayFrom1D(Color[] input, int width, int height)
    {
        Color[,] output = new Color[height, width];
        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                output[i, j] = input[i * width + j];
            }
        }
        return output;
    }

    public static Color[] Make1DArrayFrom2D(Color[,] input)
    {
        Color[] output = new Color[input.Length];

        int write = 0;
        for (int col = 0; col <= input.GetUpperBound(0); col++)
        {
            for (int row = 0; row <= input.GetUpperBound(1); row++)
            {
                output[write++] = input[col, row];
            }
        }
        return output;
    }
}
