using UnityEngine;
using System.Collections;
using UnityEditor;
using MarchingSquares;

[CustomEditor(typeof(Tile))]
public class TileEditor : Editor {

    public override void OnInspectorGUI()
    {


        var script = (Tile)target;


        EditorGUILayout.Space();

        script.Info.Type = (TileType)GUILayout.SelectionGrid((int)script.Info.Type, new string[] { " With collider", "No collider", "No render" }, 3);


        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("Assign texture:");
        EditorGUILayout.EndHorizontal();

        script.Info.Mat = (Material)EditorGUILayout.ObjectField(script.Info.Mat, typeof(Material), true);

        if (script.Info.Mat && script.Info.Mat.mainTexture)
        {

            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Tile information:");
            EditorGUILayout.EndHorizontal();

            script.Info.OffsetX = Mathf.Clamp(EditorGUILayout.IntField("Offset X: ", script.Info.OffsetX), 0, int.MaxValue);
            script.Info.OffsetY = Mathf.Clamp(EditorGUILayout.IntField("Offset Y: ", script.Info.OffsetY), 0, int.MaxValue);
            script.Info.Height = Mathf.Clamp(EditorGUILayout.IntField("Height: ", script.Info.Height), 0, script.Info.Mat.mainTexture.height);
            script.Info.Width = Mathf.Clamp(EditorGUILayout.IntField("Width: ", script.Info.Width), 0, script.Info.Mat.mainTexture.width);


            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Preview:");
            EditorGUILayout.EndHorizontal();

            if (script.Info.Width * script.Info.Height != 0)
            {
                var rect = EditorGUILayout.GetControlRect();
                rect.width = rect.height = 128;
                DrawTile(script.Info, rect);
                GUILayout.Space(128);
            }
            else
            {
                EditorGUILayout.HelpBox("Texture size must be larger than 0", MessageType.Warning);
            }
        }


        if (GUI.changed)
        {
            //Calculate
            if (script.Info.Mat && script.Info.Mat.mainTexture)
            {
                script.Info.Unit.x = script.Info.Width / (float)script.Info.Mat.mainTexture.width;
                script.Info.Unit.y = script.Info.Height / (float)script.Info.Mat.mainTexture.height;

                script.Info.Offset.x = script.Info.OffsetX / (float)script.Info.Mat.mainTexture.width;
                script.Info.Offset.y = script.Info.OffsetY / (float)script.Info.Mat.mainTexture.height;
            }


            EditorUtility.SetDirty(target);
        }
    }

    


    public static void DrawTile(BaseTile tile, Rect rect) {


        float unitX = tile.Width / (float)tile.Mat.mainTexture.width;
        float unitY = tile.Height / (float)tile.Mat.mainTexture.height;

        float offsetX = tile.OffsetX / (float)tile.Mat.mainTexture.width;
        float offsetY = tile.OffsetY / (float)tile.Mat.mainTexture.height;

        GUI.DrawTextureWithTexCoords(rect, tile.Mat.mainTexture, new Rect(offsetX, offsetY, unitX, unitY));
    }


}
