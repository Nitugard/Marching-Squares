using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MarchingSquares;
using System.Linq;

/// <summary>
/// World class. Multiple worlds are allowed.
/// </summary>
public class World : MonoBehaviour {

    /// <summary>
    /// Static values across the worlds.
    /// </summary>
    public static readonly int CHUNK_SIZE = 20, RENDER_DISTANCE = 14; //Chunk size in all worlds, can not be changed per world ( by default )
    public static readonly float DISABLE_DELAY = 0.1f; //How much time should pass before disabling chunk

    /// <summary>
    /// Render target.
    /// </summary>
    public Transform Target;

    /// <summary>
    /// Used for loading and saving the world.
    /// </summary>
    public string WorldName = "World"; 

    /// <summary>
    /// All tiles used in world.
    /// </summary>
    public Tile[] Tiles;

    /// <summary>
    /// Tiles with conditions.
    /// </summary>
    public MarchTile[] MarchingTiles;

    /// <summary>
    /// All chunks loaded in the world.
    /// </summary>
    private Dictionary<Vector2, Chunk> LoadedChunks;

    /// <summary>
    /// Self explanatory.
    /// </summary>
    public Dictionary<long, MarchTile> MarchingTilesDictionary { get; private set; }

    /// <summary>
    /// Old target position at which world got rendered.
    /// </summary>
    private Vector3 OldPosition;

    /// <summary>
    /// Chunks being rendered at the moment.
    /// </summary>
    private List<Chunk> ChunksBuffer;

    /// <summary>
    /// Reference to the class per world.
    /// </summary>
    public ChunkMeshCreatorPool MeshCreatorPool { get; private set; }

    /// <summary>
    /// Chunks that will be disabled soon.
    /// </summary>
    public List<Chunk> ChunksToBeGenerated { get; private set; }

    /// <summary>
    /// Save manager for this world.
    /// </summary>
    public ChunkSave SaveManager { get; private set; }

    private void Awake() {
        LoadedChunks = new Dictionary<Vector2, Chunk>();
        ChunksBuffer = new List<Chunk>();
        MeshCreatorPool = new ChunkMeshCreatorPool();
        OldPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        ChunksToBeGenerated = new List<Chunk>();
        SaveManager = new ChunkSave();

        //SaveManager.Load(WorldName);

        //Init marching tiles
        if (MarchingTiles != null && MarchingTiles.Length != 0)
            MarchingTilesDictionary = CreateMarchingTileDictionary(MarchingTiles);

    }




    private void Update()
    {

        //Check position
        if (Vector3.Distance(OldPosition, Target.position) > CHUNK_SIZE)
        {

            //Update position
            OldPosition = Target.position;

            //Convert target position in chunks
            Vector2 pos = PositionInChunks(Target.position);

            //Since we spawn around player
            int dist = Mathf.FloorToInt(RENDER_DISTANCE / 2);

            //Unload far chunks
            for (int i = 0; i < ChunksBuffer.Count; i++)
            {
                if (Vector3.Distance((Vector2)ChunksBuffer[i].transform.position, pos * CHUNK_SIZE) > dist * CHUNK_SIZE)
                {

                    ChunksBuffer[i].MarkedToDisable = true;
                    
                }
            }

            //Clear
            ChunksBuffer.Clear();

            for (int x = -dist + (int)pos.x; x <= dist + pos.x; x++)
            {
                for (int y = -dist + (int)pos.y; y <= dist + pos.y; y++)
                {
                    AddChunk(x, y);
                }
            }
        }

        ReBuildChunks();



    }



    /// <summary>
    /// This cost a lot, call it when you finish editing all chunks - in order to see the result.
    /// </summary>
    public void ReBuildChunks()
    {
        float closest = Mathf.Infinity;
        Chunk chunk = null;

        for (int i = 0; i < ChunksBuffer.Count; i++)
        {
            if (ChunksBuffer[i].FlaggedToUpdate && ChunksBuffer[i].HasOneBlock && !ChunksBuffer[i].Generating )
            {

                float dist = Vector2.Distance((Vector2)ChunksBuffer[i].transform.position, Target.position);
                if (dist < closest)
                {
                    closest = dist;
                    chunk = ChunksBuffer[i];
                }

            }
        }

        if (chunk == null)
            return;

        chunk.FlaggedToUpdate = false;
        
        chunk.UpdateChunk();



    }





    public Chunk GetChunkAt(int x, int y)
    {
        if (LoadedChunks.ContainsKey(new Vector2(x, y)))
        {
            //Chunks is already created at this position
            return LoadedChunks[new Vector2(x, y)];
        }
        return null;
    }




    public Chunk AddChunk(int x, int y)
    {
        Chunk chunk;
        ChunkData savedChunk = SaveManager.LoadChunk(new Vector2(x,y));
        bool loadedChunk = savedChunk == null ? false : true;

        if (LoadedChunks.ContainsKey(new Vector2(x, y)))
        {
            //Chunks is already created at this position
            chunk = LoadedChunks[new Vector2(x, y)];
            if (!chunk.gameObject.activeSelf)
                chunk.Enable();

            chunk.MarkedToDisable = false;
        }
        else
        {
            //We need to create new chunk
            GameObject obj = new GameObject("Chunk [X: " + x + " Y: " + y + "]");
            obj.transform.SetParent(transform);
            obj.layer = gameObject.layer;

            chunk = obj.AddComponent<Chunk>();
            chunk.transform.position = new Vector3(x * CHUNK_SIZE, y * CHUNK_SIZE, transform.position.z);

            //Neighbours
            Chunk LeftChunk, RightChunk, TopChunk, BotChunk;

            //Get and set neighbours
            LoadedChunks.TryGetValue(new Vector2(x - 1, y), out LeftChunk);
            if (LeftChunk)
            {
                LeftChunk.Right = chunk;
                chunk.Left = LeftChunk;
            }

            LoadedChunks.TryGetValue(new Vector2(x + 1, y), out RightChunk);
            if (RightChunk)
            {
                RightChunk.Left = chunk;
                chunk.Right = RightChunk;
            }

            LoadedChunks.TryGetValue(new Vector2(x, y + 1), out TopChunk);
            if (TopChunk)
            {
                TopChunk.Bot = chunk;
                chunk.Top = TopChunk;
            }

            LoadedChunks.TryGetValue(new Vector2(x, y - 1), out BotChunk);
            if (BotChunk)
            {
                BotChunk.Top = chunk;
                chunk.Bot = BotChunk;
            }

            if (loadedChunk)
            {
                chunk.Data = savedChunk.Data;
                chunk.HasOneBlock = savedChunk.HasOneBlock;
            }

            //Init chunk
            chunk.Init(this);

            //Add to dictionary
            LoadedChunks.Add(new Vector2(x, y), chunk);

            if (!loadedChunk)
            {
                //On chunk created
                OnChunkCreated(chunk);
            }

            //Flag
            chunk.FlaggedToUpdate = true;

        }

        ChunksBuffer.Add(chunk);

        return chunk;
    }


    /// <summary>
    /// Called when chunk is created!
    /// </summary>
    protected virtual void OnChunkCreated(Chunk chunk) {
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                chunk.SetTileLocal(x, y, (ushort)Random.Range(0,Tiles.Length));
            }
        }
    }




    /// <summary>
    /// Convert player position in Chunks.
    /// </summary>
    public static Vector2 PositionInChunks(Vector3 inPos)
    {
        return new Vector2(Mathf.FloorToInt(inPos.x / CHUNK_SIZE), Mathf.FloorToInt(inPos.y / CHUNK_SIZE));
    }


    /// <summary>
    /// Returns a tile at the given index.
    /// </summary>
    public BaseTile GetTile(int Index)
    {
        return Tiles[Index].Info;
    }


    /// <summary>
    /// Next tile id will have this id.
    /// </summary>
    public static int NextID = 0;

    public Dictionary<long, MarchTile> CreateMarchingTileDictionary(MarchTile[] array)
    {

        Dictionary<long, MarchTile> dict = new Dictionary<long, MarchTile>();

        for (int i = 0; i < array.Length; i++)
        {
            var march = array[i];


            if (SetTileID(march.Parent) == -1) {
                Debug.LogError("Failed to add marching tile with ID: " + i + " since parent tile is not assigned!");
                continue;
            }

            for (int b = 0; b < march.Bot.Length; b++)
            {
                if (SetTileID(march.Bot[b]) == -1)
                    continue;

                for (int t = 0; t < march.Top.Length; t++)
                {
                    if (SetTileID(march.Top[t]) == -1)
                        continue;

                    for (int l = 0; l < march.Left.Length; l++)
                    {

                        if (SetTileID(march.Left[l]) == -1)
                            continue;

                        for (int r = 0; r < march.Right.Length; r++)
                        {
                            if (SetTileID(march.Right[r]) == -1)
                                continue;

                            long key;
                            key = (long)array[i].Parent.Info.ID;
                            key |= ((long)array[i].Bot[b].Info.ID) << 12;
                            key |= ((long)array[i].Top[t].Info.ID) << 24;
                            key |= ((long)array[i].Left[l].Info.ID) << 36;
                            key |= ((long)array[i].Right[r].Info.ID) << 48;


                            if (!dict.ContainsKey(key))
                            {
                                dict.Add(key, array[i]);

                            }
                            else
                            {
                                Debug.unityLogger.Log("Failed to add marching tile with ID: " + i + " since one of the given conditions is not unique!");

                                //What coondition?
                                //Debug.logger.Log("Tile id: " + i + " Bot: " + array[i].Bot[b].name + " Top: " + array[i].Bot[t].name + " Left: " + array[i].Left[l].name + " Right: " + array[i].Right[r].name);
                            }
                        }
                    }
                }
            }


        }
        return dict;
    }


    public static int SetTileID(Tile tile)
    {

        if (tile == null)
            return -1;

        if (tile.Info.ID == -1)
        {
            tile.Info.ID = NextID;
            NextID++;
        }
        return 1;
    }


    /// <summary>
    /// Simple noise.
    /// </summary>
    public static int Noise(float x, float y, float scale, float mag, float exp)
    {
        return (int)(Mathf.Pow((Mathf.PerlinNoise(x / scale, y / scale) * mag), (exp)));
    }


    /// <summary>
    /// Saves all chunks that are currently visible.
    /// </summary>
    public void Save()
    {
        for (int i = 0; i < ChunksBuffer.Count; i++)
        {
            SaveManager.SaveChunk(ChunksBuffer[i].PositionInChunks, ChunksBuffer[i]);
        }
    }

    /// <summary>
    /// Saves the world on quit.
    /// </summary>
    void OnApplicationQuit()
    {
        Save();
        SaveManager.Save(WorldName);
    }


   

}





