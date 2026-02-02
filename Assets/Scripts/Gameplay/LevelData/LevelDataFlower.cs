using System;
using System.Collections.Generic;
using Gameplay.LevelData;
using UnityEngine;

[Serializable]
public class LevelDataFlower : SonatFramework.Systems.LevelManagement.LevelData
{
    public float time;
    public List<BoardElement> boardElements;
    public List<DecorElement> decorElements;
    public List<ConveyData> conveyElements;
    public List<DropBoxData> dropBoxElements;
    public List<BoardElement> match3Elements;


    public LevelData ConvertToLevelData()
    {
        LevelData levelData = new LevelData();
        levelData.time = (ushort)this.time;
        levelData.grillData = new List<GrillData>();

        foreach (var boardElement in boardElements)
        {
            GrillData grillData = new GrillData();
            grillData.position = new Vector3Data(boardElement.position * 1.3f);
            grillData.layer = new List<LayerData>();
            foreach (var boardLayer in boardElement.layers)
            {
                LayerData layerData = new LayerData(boardLayer.tileIds.Count);
                layerData.itemData = new ItemData[3];
                for (int i = 0; i < 3; i++)
                {
                    if (boardLayer.tileIds[i] >= 0)
                    {
                        layerData.itemData[i] = new ItemData();
                        layerData.itemData[i].id = (byte)(boardLayer.tileIds[i] + 1);
                    }
                }

                grillData.layer.Add(layerData);
            }

            levelData.grillData.Add(grillData);
        }

        return levelData;
    }
}

[Serializable]
public class BoardElement
{
    public int id;
    public Vector2 position;

    public List<LayerDataFlower> layers;

    public int iceMask;

    public bool hasButterfly;
}

[Serializable]
public class DecorElement
{
    public int index;
    public Vector2 position;
}

[Serializable]
public class LayerDataFlower
{
    public int layerId;
    public List<int> tileIds;
}

[Serializable]
public class ConveyData
{
    public Vector2 position;
    public Vector2 movingDirection;
    public List<BoardElement> boardElements;
    public List<DecorElement> decorElements;
}

public class DropBoxData
{
    public Vector2 position;
    public List<BoardElement> boardElements;
}