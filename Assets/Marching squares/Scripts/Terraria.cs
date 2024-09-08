using UnityEngine;
using System.Collections;
using MarchingSquares;

public class Terraria : World {

    public float Height = 100f;
    public int Seed = 0;

    private PerlinNoise noise;

    void Start() {
        noise = new PerlinNoise(Seed);
        Random.seed = Seed;
    }

    protected override void OnChunkCreated(Chunk chunk)
    {
        int tx = (int)chunk.transform.position.x;
        int ty = (int)chunk.transform.position.y;

        for (int x = 0; x < World.CHUNK_SIZE; x++)
        {

            float value = (float)noise.InterpolatedNoise(tx + x, 0, 100, 10);
            value += (float)noise.InterpolatedNoise(tx + x, 0, 1, 21) / 20;
            value += (float)noise.InterpolatedNoise(tx + x, 0, 10, 210) / 10;

            for (int y = 0; y < World.CHUNK_SIZE; y++)
            {

                double holes = noise.InterpolatedNoise(tx + x, ty+y, 7, 20);
                holes += noise.InterpolatedNoise(tx + x, ty + y, 7, 20)/10;

                if (ty + y > value + Height)
                {
                    chunk.SetTileLocal(x, y, 0);
                }
                else if(holes >= 1f){

                    chunk.SetTileLocal(x,y, 0);

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

                    //Some noise
                    float vines = noise.InterpolatedNoise(tx + x, ty + y, 4, 20);

                    if (chunk.GetTileGlobal(x, y - 1) == 0 && vines >= 0.7f)
                    {


                        int ranHeight = Random.Range(2, 12);
                        //Check if there is a space, doubled height is needed
                        for (int i = 1; i <= ranHeight * 2; i++) {
                            if (chunk.GetTileGlobal(x, y - i) != 0)
                            {
                                ranHeight = -1;
                                break;
                            }        
                        }

                        if (ranHeight != -1)
                        {   
                            for (int i = 1; i <= ranHeight; i++)
                            {
                                chunk.SetTileGlobal(x, y - i, Random.Range(4, 7));
                            }
                        }

                    }
                    
                }



            }
        }
    }


}
