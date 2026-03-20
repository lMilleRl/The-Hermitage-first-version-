using System.Diagnostics;
using UnityEngine;

public class TestPerlinNoise : MonoBehaviour
{
    public PerlinNoiseData perlinData;
    [HideInInspector] public PerlinNoise perlin;
    public Texture2D tex;    

    public void StartTest()
    {        
        tex = new Texture2D((int)((float)(perlinData.SizeMap + perlinData.countChunks * (perlinData.sizeChunk - 1)) / 20), (int)((float)(perlinData.SizeMap + perlinData.countChunks * (perlinData.sizeChunk - 1)) / 20), TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Point;

        Color color = new Color(1, 1, 1);
        
        int PosC00X; int PosC00Y; int PosC20X; int PosC20Y; int PosC01X; int PosC01Y; int PosC11X; int PosC11Y;
        var sw = new Stopwatch();        
        sw.Start();
        for (int i = -Mathf.FloorToInt(perlinData.center / 20) * perlinData.countChunks; i < Mathf.FloorToInt(perlinData.center / 20) * perlinData.countChunks + perlinData.countChunks; i++)
        {
            for (int j = -Mathf.FloorToInt(perlinData.center / 20) * perlinData.countChunks; j < Mathf.FloorToInt(perlinData.center / 20) * perlinData.countChunks + perlinData.countChunks; j++)
            {
                float MapX = (float)i / perlinData.countChunks; float MapY = (float)j / perlinData.countChunks;

                (PosC00X, PosC20X, PosC01X, PosC11X) = PerlinNoise.SetPosX(MapX, perlinData.center / 20, 1);
                (PosC00Y, PosC20Y, PosC01Y, PosC11Y) = PerlinNoise.SetPosY(MapY, perlinData.center / 20, 1);

                for (int x = 0; x < perlinData.sizeChunk; x++)
                {
                    for (int y = 0; y < perlinData.sizeChunk; y++)
                    {
                        color.r = perlin.GetPointHeight(i, j, x, y);
                        color.g = color.r;
                        color.b = color.r;
                        if(color.r <= perlinData.waterLevel)
                        {
                            color.b = (perlinData.waterLevel - color.r) / perlinData.waterLevel;
                            color.r = 0;
                            color.g = 0;
                        } 
                            
                        tex.SetPixel(x + (i + Mathf.FloorToInt(perlinData.center / 20) * perlinData.countChunks) * (perlinData.sizeChunk - 1), y + (j + Mathf.FloorToInt(perlinData.center / 20) * perlinData.countChunks) * (perlinData.sizeChunk - 1), color);
                    }
                }
            }
        }
        sw.Stop();        
        UnityEngine.Debug.Log($"perlin ms: {sw.ElapsedMilliseconds}");
        tex.Apply();        
    }
}

