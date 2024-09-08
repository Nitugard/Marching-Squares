using UnityEngine;
using System.Collections;
using MarchingSquares;

public class BackWorld : World {

    public float Height = 100f;
    public int Seed = 0;

    private PerlinNoise noise;

    void Start() {
        noise = new PerlinNoise(Seed);
    }

    protected override void OnChunkCreated(Chunk chunk)
    {
        int tx = (int)chunk.transform.position.x;
        int ty = (int)chunk.transform.position.y;

        for (int x = 0; x < World.CHUNK_SIZE; x++)
        {

            float value = (float)noise.InterpolatedNoise(tx + x, 0, 100, 10);
                  value += (float)noise.InterpolatedNoise(tx + x, 0, 1, 21)/20;
                  value += (float)noise.InterpolatedNoise(tx + x, 0, 10, 210) / 10;

            for (int y = 0; y < World.CHUNK_SIZE; y++)
            {

                if (ty + y > value + Height)
                {
                    chunk.SetTileLocal(x, y, 0);
                }
                else if (ty + y + 1 > value + Height)
                {
                    if (Random.value >= 0.051f)
                    {
                        chunk.SetTileLocal(x, y, 2);
                    }
                    else if (chunk.GetTileGlobal(x - 1, y) > 0)
                    {
                        chunk.SetTileLocal(x, y, 3);
                    }
                }
                else {
                    chunk.SetTileLocal(x, y, 1);
                }
            }
        }
    }


}
