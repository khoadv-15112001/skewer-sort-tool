using System.Collections.Generic;
using System.IO;
using Sonat.Enums;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEditor;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    public class CardCollectionGenerator : EditorWindow
    {
        private string basePath = "Assets/ScriptableObject/SonatFramework/Configs/CardCollection";
        private int albumCount = 5;
        private int cardCountInAlbum = 10;

        [MenuItem("Tools/CardCollection/CardCollectionGenerator")]
        public static void ShowWindow()
        {
            GetWindow<CardCollectionGenerator>("Card Collection Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Generate Card & Album ScriptableObjects", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            GUILayout.Label("Note: This tool will generate the assets (configSO) in the folder _Albums_genFromTool and _Cards_genFromTool");

            EditorGUILayout.Space(20);
            basePath = EditorGUILayout.TextField("Base Path", basePath);
            albumCount = EditorGUILayout.IntField("Number of Albums", albumCount);
            cardCountInAlbum = EditorGUILayout.IntField("Number of Cards in Album", cardCountInAlbum);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Albums"))
            {
                GenerateAlbums();
            }

            // if (GUILayout.Button("Generate Cards"))
            // {
            //     GenerateCards();
            // }
        }

        private void GenerateAlbums()
        {
            string folderPath = Path.Combine(basePath, "_Albums_genFromTool");
            CreateFolderIfNotExists(folderPath);

            for (int i = 0; i < albumCount; i++)
            {
                string assetName = $"Album_{i}.asset";
                string fullPath = Path.Combine(folderPath, assetName);

                if (!File.Exists(fullPath))
                {
                    var album = ScriptableObject.CreateInstance<AlbumConfig>();
                    album.type = (AlbumType)System.Enum.Parse(typeof(AlbumType), $"Album_{i}");
                    album.albumName = album.type.ToString();
                    album.reward = new RewardData()
                    {
                        resourceDatas = new List<ResourceData>()
                        {
                            new ResourceData()
                            {
                                resource = GameResource.Coin,
                                quantity = 100
                            }
                        }
                    };
                    album.cards = new List<CardType>();
                    GenerateCards(i, album);
                    AssetDatabase.CreateAsset(album, fullPath.Replace("\\", "/"));

                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"✅ Generated {albumCount} AlbumConfig assets at {folderPath}");
        }

        private void GenerateCards(int idAlbum, AlbumConfig album)
        {
            string folderPath = Path.Combine(basePath, "_Cards_genFromTool");
            CreateFolderIfNotExists(folderPath);

            for (int i = 0; i < cardCountInAlbum; i++)
            {
                string assetName = $"Card_{idAlbum}_{i}.asset";
                string fullPath = Path.Combine(folderPath, assetName);

                if (!File.Exists(fullPath))
                {
                    var card = ScriptableObject.CreateInstance<CardConfig>();
                    card.type = (CardType)System.Enum.Parse(typeof(CardType), $"Card_{idAlbum}_{i}");
                    card.cardName = $"Card_{idAlbum}_{i}";
                    card.star = i < 3 ? 1 : (i < 6 ? 2 : 3);

                    AssetDatabase.CreateAsset(card, fullPath.Replace("\\", "/"));

                    album.cards.Add(card.type);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"✅ Generated {cardCountInAlbum} CardConfig assets at {folderPath}");
        }

        private void CreateFolderIfNotExists(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parent = Path.GetDirectoryName(folderPath);
                string folderName = Path.GetFileName(folderPath);

                if (!AssetDatabase.IsValidFolder(parent))
                    CreateFolderIfNotExists(parent);

                AssetDatabase.CreateFolder(parent.Replace("\\", "/"), folderName);
            }
        }
    }
}
