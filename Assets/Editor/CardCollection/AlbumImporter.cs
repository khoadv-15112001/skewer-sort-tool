using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    public class AlbumImporter : CSVImporter
    {
        [SerializeField] private List<AlbumConfig> albumConfigs = new List<AlbumConfig>();
        [SerializeField] private List<CardConfig> cardConfigs = new List<CardConfig>();
        [SerializeField] private List<Sprite> sprites = new List<Sprite>();

        private SerializedObject so;
        private SerializedProperty cardConfigsProp;
        private SerializedProperty spritesProp;
        private SerializedProperty albumConfigsProp;
        private AlbumImageType selectedAlbumImageType;

        // Lưu scroll position ở cấp class thay vì trong OnGUI
        private Vector2 scrollPosition;

        [MenuItem("Tools/CardCollection/Album Importer")]
        public static void Open()
        {
            GetWindow<AlbumImporter>("Album Importer");
        }


        private void OnEnable()
        {
            so = new SerializedObject(this);
            albumConfigsProp = so.FindProperty("albumConfigs");
            cardConfigsProp = so.FindProperty("cardConfigs");
            spritesProp = so.FindProperty("sprites");
            selectedAlbumImageType = AlbumImageType.AlbumIcon;
        }

        protected override void OnGUI()
        {
            GUILayout.Label("Import data into card config", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            GUILayout.Label("Note: This tool will import data into the card configuration (CardConfigSO)");

            EditorGUILayout.Space(20);

            so.Update();
            EditorGUILayout.BeginVertical();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition); // Set a fixed height for the scroll view
            EditorGUILayout.PropertyField(albumConfigsProp, true);
            EditorGUILayout.PropertyField(cardConfigsProp, true);
            EditorGUILayout.PropertyField(spritesProp, true);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            so.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            GUILayout.Label("Select Album Images to Import", EditorStyles.boldLabel);
            selectedAlbumImageType = (AlbumImageType)EditorGUILayout.EnumPopup("AlbumImageType", selectedAlbumImageType);
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Import Sprites to CardConfigs"))
            {
                if (albumConfigs != null && albumConfigs.Count != 0)
                {
                    ImportSpritesToAlbumConfigs(selectedAlbumImageType);
                }
                else if (cardConfigs != null && cardConfigs.Count != 0)
                {
                    ImportSpritesToCardConfigs();
                }
                else
                {
                    Debug.LogError("Chưa chọn dataList!");

                }
            }
            // base.OnGUI();
        }

        protected override void ImportCSV()
        {


            string[] lines = GetLines();
        }

        private void ImportSpritesToCardConfigs()
        {
            if (cardConfigs.Count != sprites.Count)
            {
                Debug.LogError("Số lượng cardConfigs và sprites không khớp!");
                return;
            }
            for (int i = 0; i < cardConfigs.Count; i++)
            {
                //cardConfigs[i].sprite = sprites[i];
                EditorUtility.SetDirty(cardConfigs[i]);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("<color=green>Import Sprites to CardConfigs success</color>");
        }

        private void ImportSpritesToAlbumConfigs(AlbumImageType albumImageType)
        {
            if (albumConfigs.Count != sprites.Count)
            {
                Debug.LogError("Số lượng albumConfigs và sprites không khớp!");
                return;
            }
            for (int i = 0; i < albumConfigs.Count; i++)
            {
                switch (albumImageType)
                {
                    case AlbumImageType.AlbumIcon:
                        //albumConfigs[i].albumIcon = sprites[i];
                        break;
                    case AlbumImageType.AlbumBorder:
                        albumConfigs[i].albumBorder = sprites[i];
                        break;
                    case AlbumImageType.AlbumBackground:
                        albumConfigs[i].albumBackground = sprites[i];
                        break;
                    case AlbumImageType.AlbumExit:
                        albumConfigs[i].albumExit = sprites[i];
                        break;
                }
                EditorUtility.SetDirty(albumConfigs[i]);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("<color=green>Import Sprites to AlbumConfigs success</color>");

        }
    }

    public enum AlbumImageType
    {
        AlbumIcon,
        AlbumBorder,
        AlbumBackground,
        AlbumExit
    }
}
