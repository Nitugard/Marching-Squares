using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace MarchingSquares
{


    public class ChunkMeshCreator
    {

        private bool isDone = false;
        private object isDoneLocker = new object();
        public bool IsDone
        {
            get
            {
                bool tmp;
                lock (isDoneLocker)
                {
                    tmp = isDone;
                }
                return tmp;
            }
            set
            {
                lock (isDoneLocker)
                {
                    isDone = value;
                }
            }
        }

        private MeshRenderer render;
        private MeshFilter filter;
        private MeshCollider coll;
        private Thread thread;

        private List<Vector3> Vertices;
        private List<List<int>> Triangles;
        private List<Vector2> UVs;
        private List<Material> Maps;

        private Material[] MapArray;

        private List<Vector3> ColVertices;
        private List<int> ColTriangles;

        private Chunk chunk;
        private ChunkMeshCreatorPool pool;
        private ushort[] data;

        public ChunkMeshCreator(ChunkMeshCreatorPool pool)
        {
            Vertices = new List<Vector3>();
            Triangles = new List<List<int>>();
            UVs = new List<Vector2>();
            Maps = new List<Material>();
            ColVertices = new List<Vector3>();
            ColTriangles = new List<int>();


            this.pool = pool;

        }


        public void Build(Chunk chunk, ushort[] data, MeshFilter filter, MeshRenderer render, MeshCollider coll) {

            if (IsDone)
            {
                Debug.LogError("How did you get here?");
                return;
            }


            this.chunk = chunk;
            this.data = data;
            this.filter = filter;
            this.render = render;
            this.coll = coll;

            //Clear
            Vertices.Clear();
            Triangles.Clear();
            UVs.Clear();
            Maps.Clear();
            ColVertices.Clear();
            ColTriangles.Clear();

            thread = new Thread(ThreadFunction);
            thread.Start();
        }


        private void ThreadFunction() {
            IsDone = false;
            for (int i = 0; i < data.Length; i++)
            {
                int y = i / World.CHUNK_SIZE;
                int x = i % World.CHUNK_SIZE;

                BaseTile mainTile = chunk.myWorld.GetTile(data[i]);


                if (mainTile.Type == TileType.Air)
                    continue;

                MarchTile mtile;
                BaseTile tile = mainTile;

                if (chunk.myWorld.MarchingTilesDictionary != null)
                {
                    long value = TileKeyAt(x, y, tile.ID);

                    chunk.myWorld.MarchingTilesDictionary.TryGetValue(value, out mtile);

                    if (mtile != null)
                        tile = mtile.Base;
                }


                int index = Maps.IndexOf(tile.Mat);

                if (index == -1)
                {
                    Maps.Add(tile.Mat);
                    Triangles.Add(new List<int>());
                    index = Maps.Count - 1;
                }

                if (tile.Type == TileType.Solid)
                    AddColl(x, y);

                CreateSquare(x, y, index, tile);
            }

            MapArray = Maps.ToArray();
            

            IsDone = true;
        }


        public void ThreadChecker() {
            if (chunk.Generating && IsDone) {

                render.materials = MapArray;

                filter.mesh = new Mesh();
                filter.mesh.subMeshCount = Maps.Count;
                filter.mesh.SetVertices(Vertices);

                for (int j = 0; j < Triangles.Count; j++)
                {
                    filter.mesh.SetTriangles(Triangles[j], j);
                }
                filter.mesh.SetUVs(0, UVs);
                filter.mesh.UploadMeshData(true);

                //Collider
                Mesh mesh = new Mesh();
                mesh.SetVertices(ColVertices);
                mesh.SetTriangles(ColTriangles, 0);

                coll.sharedMesh = mesh;

                Finish();

                chunk.Generating = false;

            }
        }



        public void Abort() {
            chunk.FlaggedToUpdate = true;
            chunk.Generating = false;
            thread.Abort();
            Finish();
        }

        private void CreateSquare(int x, int y, int index, BaseTile tile)
        {
            int VCount = Vertices.Count;

            //vertices
            Vertices.Add(new Vector3(x, y));
            Vertices.Add(new Vector3(x, y + 1));
            Vertices.Add(new Vector3(x + 1, y + 1));
            Vertices.Add(new Vector3(x + 1, y));

            //Triangles
            Triangles[index].Add(VCount);
            Triangles[index].Add(VCount + 1);
            Triangles[index].Add(VCount + 2);
            Triangles[index].Add(VCount + 3);
            Triangles[index].Add(VCount);
            Triangles[index].Add(VCount + 2);

            //uvs
            UVs.Add(new Vector2(tile.Offset.x, tile.Offset.y));
            UVs.Add(new Vector2(tile.Offset.x, tile.Offset.y + tile.Unit.y));
            UVs.Add(new Vector2(tile.Offset.x + tile.Unit.x, tile.Offset.y + tile.Unit.y));
            UVs.Add(new Vector2(tile.Offset.x + tile.Unit.x, tile.Offset.y));


        }


        private void AddColl(int x, int y) {

            int top = chunk.GetTileGlobal(x, y + 1);
            int bot = chunk.GetTileGlobal(x, y - 1);
            int left = chunk.GetTileGlobal(x - 1, y);
            int right = chunk.GetTileGlobal(x + 1, y);

            BaseTile tile;

            if (top != int.MaxValue)
            {

                tile = chunk.myWorld.GetTile(top);
                if (tile.Type == TileType.Air || tile.Type == TileType.NoCollider)
                {

                    CollTriangles();

                    ColVertices.Add(new Vector3(x, y+1, 1));
                    ColVertices.Add(new Vector3(x + 1, y+1, 1));
                    ColVertices.Add(new Vector3(x + 1, y+1, 0));
                    ColVertices.Add(new Vector3(x, y+1, 0));

                }
            }


            if (bot != int.MaxValue)
            {

                tile = chunk.myWorld.GetTile(bot);
                if (tile.Type == TileType.Air || tile.Type == TileType.NoCollider)
                {
                    CollTriangles();

                    ColVertices.Add(new Vector3(x, y, 0));
                    ColVertices.Add(new Vector3(x + 1, y, 0));
                    ColVertices.Add(new Vector3(x + 1, y, 1));
                    ColVertices.Add(new Vector3(x, y, 1));

                }
            }


            if (left != int.MaxValue)
            {

                tile = chunk.myWorld.GetTile(left);
                if (tile.Type == TileType.Air || tile.Type == TileType.NoCollider)
                {

                    CollTriangles();

                    ColVertices.Add(new Vector3(x, y, 1));
                    ColVertices.Add(new Vector3(x, y+1, 1));
                    ColVertices.Add(new Vector3(x, y+1, 0));
                    ColVertices.Add(new Vector3(x, y, 0));

                }
            }


            if (right != int.MaxValue)
            {

                tile = chunk.myWorld.GetTile(right);
                if (tile.Type == TileType.Air || tile.Type == TileType.NoCollider)
                {
                    CollTriangles();

                    ColVertices.Add(new Vector3(x + 1, y+1, 1));
                    ColVertices.Add(new Vector3(x + 1, y, 1));
                    ColVertices.Add(new Vector3(x + 1, y, 0));
                    ColVertices.Add(new Vector3(x + 1, y+1, 0));

                }
            }
        }



        private void CollTriangles() {
            int count = ColVertices.Count;
            ColTriangles.Add(count);
            ColTriangles.Add(count+1);
            ColTriangles.Add(count+3);
            ColTriangles.Add(count+1);
            ColTriangles.Add(count+2);
            ColTriangles.Add(count+3);
        }



        public void Finish() {

            IsDone = false;

            //Return to the pool
            pool.ReturnToPool(this);
        }





        public long TileKeyAt(int x, int y, long baseKey)
        {
            //Bot
            int bot = chunk.GetTileGlobal(x, y - 1);
            if (bot != int.MaxValue)
                baseKey |= ((long)chunk.myWorld.GetTile(bot).ID) << 12;

            //Top
            int top = chunk.GetTileGlobal(x, y + 1);
            if (top != int.MaxValue)
                baseKey |= ((long)chunk.myWorld.GetTile(top).ID) << 24;


            //Left
            int left = chunk.GetTileGlobal(x - 1, y);
            if (left != int.MaxValue)
                baseKey |= ((long)chunk.myWorld.GetTile(left).ID) << 36;


            //Right
            int right = chunk.GetTileGlobal(x + 1, y);
            if (right != int.MaxValue)
                baseKey |= ((long)chunk.myWorld.GetTile(right).ID) << 48;

            return baseKey;
        }


    }
}