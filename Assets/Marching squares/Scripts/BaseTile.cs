using UnityEngine;
#if UNITY_ENGINE
using UnityEditor;
#endif
using System.Collections;
using System;

namespace MarchingSquares
{

    /// <summary>
    /// Possible tile types.
    /// </summary>
    public enum TileType { 
        Solid, NoCollider, Air
    }


    /// <summary>
    /// Tile class.
    /// </summary>
    [Serializable]
    public class BaseTile
    {

        //Events
        public delegate void _TileEvent(int x, int y, Chunk chunk);

        public event _TileEvent OnTilePlacedEvent;
        public event _TileEvent OnTileRemovedEvent;

        /// <summary>
        /// Each offset is noather submesh.
        /// </summary>
        public int zOffest;

        /// <summary>
        /// Unique tile id. This is initialized when creating tile for the first time!
        /// </summary>
        [NonSerialized]
        public int ID = -1;

        /// <summary>
        /// Tile type.
        /// </summary>
        public TileType Type;

        /// <summary>
        /// Material from which tile is extracted. 
        /// </summary>
        public Material Mat;

        /// <summary>
        /// X Coordiante of the tile in the texture.
        /// </summary>
        public int OffsetX;

        /// <summary>
        /// Y Coordiante of the tile in the texture.
        /// </summary>
        public int OffsetY;

        /// <summary>
        /// Tile width.
        /// </summary>
        public int Width;

        /// <summary>
        /// Tile height.
        /// </summary>
        public int Height;

        //Pre calculated
        public Vector2 Unit;
        public Vector2 Offset;

        public void OnTilePlaced(int x, int y, Chunk chunk)
        {
            if (OnTilePlacedEvent != null)
                OnTilePlacedEvent.Invoke(x, y, chunk);
        }

        public void OnTileRemoved(int x, int y, Chunk chunk)
        {
            if (OnTileRemovedEvent != null)
                OnTileRemovedEvent.Invoke(x, y, chunk);
        }

    }
}
