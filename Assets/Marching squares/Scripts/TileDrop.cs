using UnityEngine;
using System.Collections;
using MarchingSquares;

namespace MarchingSquares
{

    public class TileDrop : MonoBehaviour
    {

        /// <summary>
        /// World reference.
        /// </summary>
        public World WorldReference;

        /// <summary>
        /// Parent tile.
        /// </summary>
        public Tile Parent;

        /// <summary>
        /// Object to drop on destroy.
        /// </summary>
        public GameObject ToDropOnDestroy;

        void Start()
        {

            Parent.Info.OnTileRemovedEvent += Info_OnTileRemovedEvent;
        }

        void Info_OnTileRemovedEvent(int x, int y, Chunk chunk)
        {
            //Check world
            if (chunk.myWorld == WorldReference)
            {
                if (ToDropOnDestroy != null)
                {
                    Instantiate(ToDropOnDestroy, new Vector3(x + 0.5f, y + 0.5f, 0.5f) + chunk.transform.position, Quaternion.identity);
                }
            }
        }
    }
}