using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MarchingSquares;
using System.Linq;

namespace MarchingSquares
{

    [Serializable]
    public class ChunkData
    {

        /// <summary>
        /// Chunk data.
        /// </summary>
        [SerializeField]
        public ushort[] Data;

        /// <summary>
        /// if the chunks has more than one block.
        /// </summary>
        [SerializeField]
        public bool HasOneBlock;

    }


    /// <summary>
    /// Holds all information about saved chunks.
    /// </summary>
    public class ChunkSave
    {


        //Need some kind of seriazible dictionary
        [SerializeField]
        public Dictionary<string, ChunkData> SavedData = new Dictionary<string, ChunkData>();

        public void Load(string SaveName)
        {
            string path = (Application.persistentDataPath + "/" + SaveName + ".dat");
            if (File.Exists(path))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
                SavedData = (Dictionary<string, ChunkData>)bf.Deserialize(fs);
                fs.Close();
            }
            else
            {
                Debug.Log("Could not find the file. Load failed!");
            }
        }


        public void Save(string SaveName)
        {
            string path = (Application.persistentDataPath + "/" + SaveName + ".dat");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            bf.Serialize(fs, SavedData);
            fs.Close();
        }


        public string KeyName(Vector2 position)
        {
            int x = Mathf.FloorToInt(position.x);
            int y = Mathf.FloorToInt(position.y);
            return x + " " + y;
        }

        public void SaveChunk(Vector2 position, Chunk chunk)
        {

            ChunkData savedChunk = LoadChunk(position);

            if (savedChunk != null)
            {
                //There is already a chunk saved at this position so we overwrite
                savedChunk.Data = chunk.Data;
                savedChunk.HasOneBlock = chunk.HasOneBlock;

            }
            else
            {
                //Add a new value
                savedChunk = new ChunkData();
                savedChunk.Data = chunk.Data;
                savedChunk.HasOneBlock = chunk.HasOneBlock;
                SavedData.Add(KeyName(position), savedChunk);

            }

        }


        public ChunkData LoadChunk(Vector2 position)
        {
            ChunkData data = null;
            SavedData.TryGetValue(KeyName(position), out data);
            return data;
        }



    }
}