using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using MarchingSquares;
using System.Linq;

[CustomEditor(typeof(MarchTile))]
public class MarchEditor : Editor {

    public override void OnInspectorGUI()
    {
        var script = (MarchTile)target;

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("Tile type:");
        EditorGUILayout.EndHorizontal();

        script.Base.Type = (TileType)GUILayout.SelectionGrid((int)script.Base.Type, new string[] { " With collider", "No collider", "No render" }, 3);

        if (script.Base.Type != TileType.Air)
        {
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Assign texture:");
            EditorGUILayout.EndHorizontal();

            script.Base.Mat = (Material)EditorGUILayout.ObjectField(script.Base.Mat, typeof(Material), true);

            if (script.Base.Mat && script.Base.Mat.mainTexture)
            {

                GUILayout.Space(20);
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                EditorGUILayout.LabelField("Tile information:");
                EditorGUILayout.EndHorizontal();

                script.Base.OffsetX = Mathf.Clamp(EditorGUILayout.IntField("Offset X: ", script.Base.OffsetX), 0, int.MaxValue);
                script.Base.OffsetY = Mathf.Clamp(EditorGUILayout.IntField("Offset Y: ", script.Base.OffsetY), 0, int.MaxValue);
                script.Base.Height = Mathf.Clamp(EditorGUILayout.IntField("Height: ", script.Base.Height), 0, script.Base.Mat.mainTexture.height);
                script.Base.Width = Mathf.Clamp(EditorGUILayout.IntField("Width: ", script.Base.Width), 0, script.Base.Mat.mainTexture.width);

                GUILayout.Space(20);
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                EditorGUILayout.LabelField("Parent:");
                EditorGUILayout.EndHorizontal();

                script.Parent = (Tile)EditorGUILayout.ObjectField("Parent: ",script.Parent, typeof(Tile), true);

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                EditorGUILayout.LabelField("Right:");
                if (GUILayout.Button("Add", EditorStyles.miniButton)) {
                    var list = script.Right.ToList();
                    list.Add(null);
                    script.Right = list.ToArray();
                }
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < script.Right.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    script.Right[i] = (Tile)EditorGUILayout.ObjectField(script.Right[i], typeof(Tile), true);
                    if (GUILayout.Button("Del", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
                    {
                        var list = script.Right.ToList();
                        list.RemoveAt(i);
                        script.Right = list.ToArray();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                EditorGUILayout.LabelField("Left:");
                if (GUILayout.Button("Add", EditorStyles.miniButton))
                {
                    var list = script.Left.ToList();
                    list.Add(null);
                    script.Left = list.ToArray();
                }
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < script.Left.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    script.Left[i] = (Tile)EditorGUILayout.ObjectField(script.Left[i], typeof(Tile), true);
                    if (GUILayout.Button("Del", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
                    {
                        var list = script.Left.ToList();
                        list.RemoveAt(i);
                        script.Left = list.ToArray();
                    }

                    EditorGUILayout.EndHorizontal();

                }
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                EditorGUILayout.LabelField("Top:");
                if (GUILayout.Button("Add", EditorStyles.miniButton))
                {
                    var list = script.Top.ToList();
                    list.Add(null);
                    script.Top = list.ToArray();
                }
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < script.Top.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    script.Top[i] = (Tile)EditorGUILayout.ObjectField(script.Top[i], typeof(Tile), true);
                    if (GUILayout.Button("Del", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
                    {
                        var list = script.Top.ToList();
                        list.RemoveAt(i);
                        script.Top = list.ToArray();
                    }

                    EditorGUILayout.EndHorizontal();

                }
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                EditorGUILayout.LabelField("Bot:");
                if (GUILayout.Button("Add", EditorStyles.miniButton))
                {
                    var list = script.Bot.ToList();
                    list.Add(null);
                    script.Bot = list.ToArray();
                }
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < script.Bot.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    script.Bot[i] = (Tile)EditorGUILayout.ObjectField(script.Bot[i], typeof(Tile), true);
                    if (GUILayout.Button("Del", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
                    {
                        var list = script.Bot.ToList();
                        list.RemoveAt(i);
                        script.Bot = list.ToArray();
                    }

                    EditorGUILayout.EndHorizontal();

                }

                GUILayout.Space(20);


                GUILayout.Space(20);
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                EditorGUILayout.LabelField("Preview:");
                EditorGUILayout.EndHorizontal();

                if (script.Base.Width * script.Base.Height != 0)
                {
                    var rect = EditorGUILayout.GetControlRect();
                    rect.width = rect.height = 64;

                    TileEditor.DrawTile(script.Base, rect);
                    GUILayout.Space(50);

                   /*
                    var rect = EditorGUILayout.GetControlRect();
                    rect.width = rect.height = 64;

                    rect.y += 64;
                    //Left
                    if(script.Left)
                    TileEditor.DrawTile(script.Left.Info, rect);

                    rect.x += 64;
                    //Base

                    rect.y += 64;
                    //Bot
                    if (script.Bot)
                    TileEditor.DrawTile(script.Bot.Info, rect);

                    rect.y -= 128;
                    //Top
                    if (script.Top)
                    TileEditor.DrawTile(script.Top.Info, rect);

                    rect.x += 64;
                    rect.y += 64;
                    //Right
                    if (script.Right)
                    TileEditor.DrawTile(script.Right.Info, rect);

                    */
                }
                else
                {
                    EditorGUILayout.HelpBox("Texture size must be larger than 0", MessageType.Warning);
                }
            }
        }

        if (GUI.changed)
        {
            //Calculate
            if (script.Base.Mat && script.Base.Mat.mainTexture)
            {
                script.Base.Unit.x = script.Base.Width / (float)script.Base.Mat.mainTexture.width;
                script.Base.Unit.y = script.Base.Height / (float)script.Base.Mat.mainTexture.height;

                script.Base.Offset.x = script.Base.OffsetX / (float)script.Base.Mat.mainTexture.width;
                script.Base.Offset.y = script.Base.OffsetY / (float)script.Base.Mat.mainTexture.height;
            }

            EditorUtility.SetDirty(target);
        }
        
    }




}
