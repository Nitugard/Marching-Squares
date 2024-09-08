using UnityEngine;
using System.Collections;
using System;

namespace MarchingSquares
{
    public class Chunk : MonoBehaviour
    {


        /// <summary>
        /// Chunk data.
        /// </summary>
        public ushort[] Data;

        /// <summary>
        /// Neighbours.
        /// </summary>
        public Chunk Top, Left, Bot, Right;

        /// <summary>
        /// World to which chunk belong.
        /// </summary>
        public World myWorld { get; private set; }

        /// <summary>
        /// Mesh renderer reference.
        /// </summary>
        private MeshRenderer Render;

        /// <summary>
        /// Mesh filter reference.
        /// </summary>
        private MeshFilter Filter;

        /// <summary>
        /// Collider reference.
        /// </summary>
        private MeshCollider Coll;

        /// <summary>
        /// Wheter this chunk should be updated.
        /// </summary>
        public bool FlaggedToUpdate { get; set; }

        /// <summary>
        /// Chunk finished generating?.
        /// </summary>
        public bool Generating { get; set; }

        /// <summary>
        /// Chunk initialized yet?
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// Has the reference to the cmc while generating.
        /// </summary>
        public ChunkMeshCreator CMC { get; private set; }

        /// <summary>
        /// Has this chunk atleast one block?
        /// </summary>
        public bool HasOneBlock { get; set; }

        /// <summary>
        /// Is this chunk marked to be disabled?
        /// </summary>
        private bool _MarkedToDisable;

        /// <summary>
        /// Position in chunks.
        /// </summary>
        public Vector3 PositionInChunks { get; private set; }


        public bool MarkedToDisable
        {
            get { return _MarkedToDisable; }
            set
            {
                if (value == true)
                {
                    DisableTime = Time.time+World.DISABLE_DELAY;
                }
                _MarkedToDisable = value;
            }
        }



        /// <summary>
        /// Time at which disabling was started.
        /// </summary>
        private float DisableTime;


        public void Init(World world) {

            myWorld = world;

            if (Data == null)
            {
                Data = new ushort[World.CHUNK_SIZE * World.CHUNK_SIZE];
            }
            else { //Data was set before the chunk is initialized - this is only possible when chunk was loaded

                //Call tile placed event on each tile
                for (int i = 0; i < Data.Length; i++) {
                    myWorld.GetTile(Data[i]).OnTilePlaced(i%World.CHUNK_SIZE, i/World.CHUNK_SIZE, this);
                }
            }

            PositionInChunks = World.PositionInChunks(transform.position);

            //Add componenets
            Render = gameObject.AddComponent<MeshRenderer>();
            Filter = gameObject.AddComponent<MeshFilter>();
            Coll = gameObject.AddComponent<MeshCollider>();

        }

        /// <summary>
        /// Sets the tile at the given location, in the chunk. 
        /// </summary>
        public bool SetTileLocal(int x, int y, int tile, bool sendEvents = true) {

            if (x < 0 || y < 0 || x >= World.CHUNK_SIZE || y >= World.CHUNK_SIZE)
                return false;

            if (x == 0 && Left)
                Left.FlaggedToUpdate = true;

            if (y == 0 && Bot)
                Bot.FlaggedToUpdate = true;

            if (x+1 >= World.CHUNK_SIZE && Right)
                Right.FlaggedToUpdate = true;

            if (y+1 >= World.CHUNK_SIZE && Top)
                Top.FlaggedToUpdate = true;

            FlaggedToUpdate = true;

            //Old tile
            BaseTile oldTile = myWorld.GetTile(Data[x + y * World.CHUNK_SIZE]);
            BaseTile newTile = myWorld.GetTile(tile);

            if (sendEvents && newTile.ID != oldTile.ID)
                oldTile.OnTileRemoved(x, y, this);

            Data[x + y * World.CHUNK_SIZE] = (ushort)tile;

            //Get tile
            if (sendEvents && newTile.ID != oldTile.ID)
                newTile.OnTilePlaced(x, y, this);

            if(newTile.Type != TileType.Air)
                HasOneBlock = true;

            return true;
        }

        
        /// <summary>
        /// Sets the tile in this chunk and even in neighbours chunks. If they exsist. Slow. Returns chunk at which tile was set.
        /// </summary>
        public Chunk SetTileGlobal(int x, int y, int TileIndex, bool sendEvents = true, bool createChunkIfNeeded = false)
        {

            Vector2 pos = World.PositionInChunks(transform.position);
            int chunkX = (int)pos.x;
            int chunkY = (int)pos.y;

            if (x < 0)
            {
                if (Left != default(Chunk))
                    return Left.SetTileGlobal(World.CHUNK_SIZE + x, y, TileIndex, sendEvents, createChunkIfNeeded);
                else
                    if (createChunkIfNeeded)
                        return myWorld.AddChunk(chunkX - 1, chunkY).SetTileGlobal(World.CHUNK_SIZE + x, y, TileIndex, sendEvents, createChunkIfNeeded);
                    else return null;
            }
            if (y < 0)
            {
                if(Bot != default(Chunk))
                    return Bot.SetTileGlobal(x, World.CHUNK_SIZE + y, TileIndex, sendEvents, createChunkIfNeeded);
                else
                    if (createChunkIfNeeded)
                        return myWorld.AddChunk(chunkX, chunkY - 1).SetTileGlobal(x, World.CHUNK_SIZE + y, TileIndex, sendEvents, createChunkIfNeeded);
                    else return null;
            }
            if (x >= World.CHUNK_SIZE)
            {
                if(Right != default(Chunk))
                    return Right.SetTileGlobal(x - World.CHUNK_SIZE, y, TileIndex, sendEvents, createChunkIfNeeded);
                else
                    if (createChunkIfNeeded)
                        return myWorld.AddChunk(chunkX + 1, chunkY).SetTileGlobal(x - World.CHUNK_SIZE, y, TileIndex, sendEvents, createChunkIfNeeded);
                    else return null;
            }
            if (y >= World.CHUNK_SIZE)
            {
                if(Top != default(Chunk))
                    return Top.SetTileGlobal(x, y - World.CHUNK_SIZE, TileIndex, sendEvents, createChunkIfNeeded);
                else
                    if (createChunkIfNeeded)
                        return myWorld.AddChunk(chunkX, chunkY + 1).SetTileGlobal(x, y - World.CHUNK_SIZE, TileIndex, sendEvents, createChunkIfNeeded);
                    else return null;
            }


            SetTileLocal(x, y, TileIndex, sendEvents);
            return this;
        }

        

        /// <summary>
        /// Get tile only from this chunk data array. Fast.
        /// </summary>
        public int GetTileLocal(int x, int y) {
            return Data[x + y * World.CHUNK_SIZE];
        }


        /// <summary>
        /// Get tile only from this chunk and it's neighbours. Slow.
        /// </summary>
        public int GetTileGlobal(int x, int y)
        {
            if (x < 0)
            {
                if (Left != default(Chunk))
                    return Left.GetTileGlobal(World.CHUNK_SIZE + x, y);
                else
                    return int.MaxValue;
            }
            if (y < 0)
            {
                if (Bot != default(Chunk))
                    return Bot.GetTileGlobal(x, World.CHUNK_SIZE + y);
                else
                    return int.MaxValue;
            }
            if (x >= World.CHUNK_SIZE)
            {
                if (Right != default(Chunk))
                    return Right.GetTileGlobal(x - World.CHUNK_SIZE, y);
                else
                    return int.MaxValue;
            }
            if (y >= World.CHUNK_SIZE)
            {
                if (Top != default(Chunk))
                    return Top.GetTileGlobal(x, y - World.CHUNK_SIZE);
                else
                    return int.MaxValue;
            }

            return Data[x + y * World.CHUNK_SIZE];
        }



        /// <summary>
        /// Updates chunk mesh. If the chunk was re-generating already. The process will stop and it will start again.
        /// </summary>
        public void UpdateChunk() {
            if (!Generating)
            {
                Generating = true;
                FlaggedToUpdate = false;
                CMC = myWorld.MeshCreatorPool.GetMeshCreator();
                CMC.Build(this, Data, Filter, Render, Coll);
            }
            else {
                //Flag it for future updates
                FlaggedToUpdate = true;
            }

        }


        private void Update() {
            if (Generating) {
                CMC.ThreadChecker();
            }
            if (Time.time >= DisableTime) {
                if (MarkedToDisable)
                {
                    if (Generating)
                    {
                        CMC.Abort();
                    }
                    Disable();
                }
                else
                    DisableTime = -1;
            }

        }



        public void UpdateIn(float time) {
            Invoke("UpdateChunk", time);
        }



        public void Disable() {

            gameObject.SetActive(false);
            
        }


        public void Enable() {

            gameObject.SetActive(true);
        }


        private void OnDisable()
        {

            myWorld.SaveManager.SaveChunk(PositionInChunks, this);

        }

        private void OnEnable() {

        }



        public void LitChunk()
        {

        }

    }
}
