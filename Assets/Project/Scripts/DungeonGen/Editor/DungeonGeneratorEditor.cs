using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    private DungeonGenerator generator;

    private void OnEnable()
    {
        generator = (DungeonGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        // Title for the Dungeon Generator settings
        EditorGUILayout.LabelField("Dungeon Generator Settings", EditorStyles.boldLabel);

        // Dungeon size inputs
        generator.width = EditorGUILayout.IntField("Width", generator.width);
        generator.height = EditorGUILayout.IntField("Height", generator.height);
        generator.depth = EditorGUILayout.IntField("Depth", generator.depth);

        // Settings for wall generation
        generator.hasWalls = EditorGUILayout.Toggle("Generate Walls", generator.hasWalls);
        generator.tileSize = EditorGUILayout.FloatField("Tile Size", generator.tileSize);

        // Ground Tiles selection
        EditorGUILayout.LabelField("Ground Tiles", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Choose the ground tiles to be used in the dungeon generation.", MessageType.Info);
        SerializedProperty groundTiles = serializedObject.FindProperty("groundTiles");
        EditorGUILayout.PropertyField(groundTiles, true);

        // Wall Tiles selection
        EditorGUILayout.LabelField("Wall Tiles", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Choose the wall tiles to be used in the dungeon generation.", MessageType.Info);
        SerializedProperty wallTiles = serializedObject.FindProperty("wallTiles");
        EditorGUILayout.PropertyField(wallTiles, true);

        // Object prefabs
        EditorGUILayout.LabelField("Dungeon Objects", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Choose objects that can be placed on the ground tiles.", MessageType.Info);
        SerializedProperty dungeonObjects = serializedObject.FindProperty("dungeonObjects");
        EditorGUILayout.PropertyField(dungeonObjects, true);

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
        
        // Generate button
        if (GUILayout.Button("Generate Dungeon"))
        {
            generator.GenerateDungeon();
        }
    }
}
