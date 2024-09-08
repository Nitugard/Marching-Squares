using UnityEngine;
using System.Collections;
using MarchingSquares;

public class CustomWorld : World {

    protected override void OnChunkCreated(Chunk chunk)
    {

        //SETS ALL TILES IN THE CHUNK TO TILE WITH ARRAY INDEX OF ZERO


        for (int x = 0; x < World.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < World.CHUNK_SIZE; y++)
            {
                chunk.SetTileLocal(x, y, 0);
            }
        }
    }
}
