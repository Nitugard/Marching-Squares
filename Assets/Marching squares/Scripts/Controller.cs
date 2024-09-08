using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MarchingSquares;
using UnityEngine.EventSystems;
public class Controller : MonoBehaviour {

    public float Speed = 10;
    public World WorldReference;

    private int selected = 0;
    private string[] Tiles;
    private int Size = 0;

    private void Start() {
        Tiles = new string[WorldReference.Tiles.Length];
        for (int i = 0; i < WorldReference.Tiles.Length; i++) {
            Tiles[i] = WorldReference.Tiles[i].name;
        }
    }

    private void Update()
    {
        
        Camera.main.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * 20;
        transform.Translate(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))*Speed*Time.deltaTime*10);

        if (Input.GetMouseButton(0) && GUIUtility.hotControl == 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            Edit(Input.mousePosition);
        }

        if (Input.touchCount > 0) {
            Edit(Input.touches[0].position);
        }
        Vector3 inp = Input.acceleration;
        inp.z = 0;
        transform.Translate(inp * Speed * Time.deltaTime * 10);
          
    }


    private void Edit(Vector3 atPos) {
        Vector3 pos = Camera.main.ScreenToWorldPoint(atPos);
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int cx = Mathf.FloorToInt(x / (float)World.CHUNK_SIZE);
        int cy = Mathf.FloorToInt(y / (float)World.CHUNK_SIZE);
        var chunk = WorldReference.GetChunkAt(cx, cy);

        if (!chunk)
            return;

        int nx = x - (int)chunk.transform.position.x;
        int ny = y - (int)chunk.transform.position.y;

        if (Size == 0)
        {
            chunk.SetTileLocal(nx, ny, (ushort)selected);
        }
        else
            Box(nx, ny, chunk);

        WorldReference.ReBuildChunks();

    }

    private void Box(int x, int y, Chunk chunk)
    {

        for (int px = x - Size; px < x + Size; px++)
        {
            for (int py = y - Size; py < y + Size; py++)
            {
                chunk.SetTileGlobal(px, py, (ushort)selected);
            }
        }
    }



    private void OnGUI() {

        GUILayout.BeginHorizontal();
        GUILayout.Label(World.RENDER_DISTANCE * World.RENDER_DISTANCE + " chunks | ");
        GUILayout.Label(World.CHUNK_SIZE * World.CHUNK_SIZE + " squares per chunk | ");
        GUILayout.Label((World.RENDER_DISTANCE * World.RENDER_DISTANCE * World.CHUNK_SIZE * World.CHUNK_SIZE) + " total squares");
        GUILayout.EndHorizontal();

        selected = GUILayout.SelectionGrid(selected, Tiles, 8);
        GUILayout.BeginHorizontal();

        Size = (int)GUILayout.HorizontalSlider(Size, 0, 100);
        Speed = (int)GUILayout.HorizontalSlider(Speed, 0, 20);
        GUILayout.EndHorizontal();


    }
}
