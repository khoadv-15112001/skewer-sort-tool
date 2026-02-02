using Sonat.AdsModule;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager.Elements
{
    public class AdsConfigDraw
    {
        private AdsConfig adsConfig;
        private bool fold;
        private string label;

        // private Texture2D MakeTex(int width, int height, Color col)
        // {
        //     Color[] pix = new Color[width*height];
        //
        //     for(int i = 0; i < pix.Length; i++)
        //         pix[i] = col;
        //
        //     Texture2D result = new Texture2D(width, height);
        //     result.SetPixels(pix);
        //     result.Apply();
        //
        //     return result;
        // }

        public AdsConfigDraw(AdsConfig adsConfig, string label)
        {
            this.adsConfig = adsConfig;
            this.label = label;
        }

        public void Draw()
        {
            if (adsConfig == null) return;
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            fold = EditorGUILayout.Foldout(fold, label, true, EditorStyles.foldout);
            
            if (fold)
            {
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                adsConfig.appId = EditorGUILayout.TextField("App Id", adsConfig.appId);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                if (adsConfig.adUnitIds != null && adsConfig.adUnitIds.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    GUILayout.Label("Ad Unit", EditorStyles.boldLabel, GUILayout.Width(60));
                    GUILayout.Label("ID", EditorStyles.boldLabel, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(300));
                    GUILayout.Space(10);
                    GUILayout.Label("Placement", EditorStyles.boldLabel, GUILayout.Width(100));
                    GUILayout.Space(5);
                    GUILayout.Label("Ad Type", EditorStyles.boldLabel, GUILayout.Width(100));

                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(3);


                    for (int i = 0; i < adsConfig.adUnitIds.Count; i++)
                    {
                        AdUnitIdDraw adUnitIdDraw = new AdUnitIdDraw(adsConfig.adUnitIds[i], this);
                        adUnitIdDraw.Show();
                        GUILayout.Space(2);
                    }
                }
                else
                {
                    //GUILayout.Label("Ads unit is empty", EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.HelpBox("Ads unit is empty", MessageType.Warning);
                }

                GUILayout.Space(5);

                if (GUILayout.Button("+ Ad Unit", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    AddItem();
                }

                GUILayout.Space(10);
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void AddItem()
        {
            adsConfig.adUnitIds.Add(new AdUnitId());
        }

        public void RemoveItem(AdUnitId adUnitId)
        {
            adsConfig.adUnitIds.Remove(adUnitId);
        }
    }
}