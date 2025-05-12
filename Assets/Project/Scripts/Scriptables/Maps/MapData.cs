using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MapData", menuName = "MapData", order = 0)]
public class MapData : ScriptableObject 

{
    public List<FloorData> floors;
}

[System.Serializable]
public class FloorData
{
    public Texture2D layoutTexture;  // The texture representing the floor's tile layout
    public int repeatCount = 1;      // How many times to repeat this floor pattern
    public List<DecorLayer> decorLayers = new List<DecorLayer>();  // Decoration layers for the floor
}

[System.Serializable]
public class DecorLayer
{
    public Texture2D decorTexture;   // The texture for decorations
    public List<GameObject> listOfDecor = new List<GameObject>();  // The list of possible decoration objects
    public DecorType decorType;      // The type of decoration (could be specific to your design)


}
    public enum DecorType
    {
        Wall,
        Floor,
        Ceiling
    }