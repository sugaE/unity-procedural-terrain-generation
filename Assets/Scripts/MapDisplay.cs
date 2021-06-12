using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender;

    public void DrawNoiseMap(float[,] noiseMap)
    {

        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {

                // attention ! for the loop is first row, then col
                colorMap[j * width + i] = Color.Lerp(Color.black, Color.white, noiseMap[i, j]);

            }
        }

        texture.SetPixels(colorMap);
        texture.Apply(); // remember

        // can be viewed in scene view
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(width, 1, height);

    }
}