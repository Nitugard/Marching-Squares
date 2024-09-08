using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MarchingSquares
{

    public class ChunkMeshCreatorPool
    {

        private Stack<ChunkMeshCreator> pool = new Stack<ChunkMeshCreator>();


        public ChunkMeshCreator GetMeshCreator() {
            if (pool.Count > 0) {
                return pool.Pop();
            }
            return new ChunkMeshCreator(this);
        }


        public void ReturnToPool(ChunkMeshCreator creator) {
            pool.Push(creator);
        }


    }
}
