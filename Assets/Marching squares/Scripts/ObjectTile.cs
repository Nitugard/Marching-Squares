using UnityEngine;
using System.Collections;
using MarchingSquares;
using System.Collections.Generic;

namespace MarchingSquares
{

    public class ObjectTile : MonoBehaviour
    {

        public GameObject Sprite;

        /// <summary>
        /// World reference.
        /// </summary>
        public World WorldReference;

        /// <summary>
        /// Parent tile.
        /// </summary>
        public Tile Parent;

        /// <summary>
        /// Easier managment.
        /// </summary>
        private Dictionary<Vector3, GameObject> SavedObjects;

        void Start()
        {

            SavedObjects = new Dictionary<Vector3, GameObject>();

            Parent.Info.OnTilePlacedEvent += MainTile_OnTilePlacedEvent;
            Parent.Info.OnTileRemovedEvent += Info_OnTileRemovedEvent;
        }



        void Info_OnTileRemovedEvent(int x, int y, Chunk chunk)
        {
            GameObject obj;
            SavedObjects.TryGetValue(new Vector3(x, y, -1) + chunk.transform.position, out obj);
            //Check world
            if (chunk.myWorld == WorldReference)
            {
                if (obj != null)
                {
                    Destroy(obj);
                    SavedObjects.Remove(new Vector3(x, y, -1) + chunk.transform.position);
                }
            }
        }

        void MainTile_OnTilePlacedEvent(int x, int y, Chunk chunk)
        {
            GameObject obj;
            SavedObjects.TryGetValue(new Vector3(x, y, -1) + chunk.transform.position, out obj);
            //Check world
            if (chunk.myWorld == WorldReference)
            {
                if (obj == null)
                {
                    obj = Instantiate(Sprite, new Vector3(x, y, -1) + chunk.transform.position, Quaternion.identity) as GameObject;
                    obj.transform.parent = chunk.transform;
                    foreach (Transform t in obj.transform)
                    {
                        t.gameObject.layer = chunk.gameObject.layer;
                    }
                    SavedObjects.Add(new Vector3(x, y, -1) + chunk.transform.position, obj);
                }
                else
                    obj.SetActive(true);

            }

        }




    }
}