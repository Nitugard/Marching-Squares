using UnityEngine;
using System.Collections;
using System;

namespace MarchingSquares{

    [Serializable]
    public class MarchTile : MonoBehaviour
    {

        public BaseTile Base;
        public Tile Parent;
        public Tile[] Top = new Tile[1];
        public Tile[] Left = new Tile[1];
        public Tile[] Right = new Tile[1];
        public Tile[] Bot = new Tile[1];

    }
}