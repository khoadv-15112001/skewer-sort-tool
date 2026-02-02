#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using MyGame.Modules.CardCollection;

public class AlbumAndCardNameFiller
{
    [MenuItem("Sonat Tools/Fill Album & Card Names")]
    public static void FillNames()
    {
        string[] albumNames = {
            "BBQ & Grill",
            "Seafood",
            "Summer Fruits",
            "Refreshing Drinks",
            "Desserts",
            "Vegetarian & Healthy",
            "American Dishes",
            "Japanese Dishes",
            "Korean Dishes",
            "Thai Dishes"
        };
        // 10x9 card
        string[,] cardNames = {
            {"Tongs","Hotdog","Oven Mitt","Chicken","Lamb Chop","Grill","Beef Steak","Barbecue","Golden Steak"},
            {"Oyster","Fishing Rod","Octopus","Cracker","Salmon","Scallops","Lobster","Fishing Ship","King Crab"},
            {"Peeler","Mango","Fruit Bowl","Fruit Skewer","Blender","Watermelon","Picnic Basket","Coconut Drink","Summer Set"},
            {"Water Set","Ice Bucket","Soda Can","Orange Juice","Hot Cocoa","Coconut Mix","Cosmo","Tea Time","Wine Bucket"},
            {"Macaron","Piping Bag","Éclairs","Waffle Iron","Ice Cream","Crème brûlée","Waffle","Mini Oven","Aurum Flow"},
            {"Avocado","Shrimp and Spinach","Berry Oatmeal","Salad","Veggie Wrap","Tofu & Broccoli","Chicken Salad","Shrimp Noodle","Healthy Power"},
            {"Cutlery","Clam Chowder","Hamburger","Fries","Cheese","Corn Dog","Fried Chicken","Masterchef","Steak Dinner"},
            {"Soy Sauce","Miso Soup","Takoyaki","Tempura","Dango","Bento","Omakase","Katsu Bowls","Kaiseki"},
            {"Utensils","Kimchi","Spicy Noodle","Tteokbokki","Kimbap","Mandu","Gopchang","Gamjatang","Hot Pot"},
            {"Mortar","Pad Thai","Satay","Tom Yum","Thai Ceramic","Kanom Tuay","Miang Kham","Khao Sap","Leng Saap"}
        };
        // Fill Album Names
        string[] albumGuids = AssetDatabase.FindAssets("t:AlbumConfig", new[] { "Assets/ScriptableObject/SonatFramework/Configs/CardCollection/Albums" });
        foreach (string guid in albumGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AlbumConfig album = AssetDatabase.LoadAssetAtPath<AlbumConfig>(path);
            if (album != null)
            {
                int index = GetIndexFromName(album.name.Replace("Album_", ""));
                if (index >= 0 && index < albumNames.Length)
                {
                    album.albumName = albumNames[index];
                    EditorUtility.SetDirty(album);
                }
            }
        }
        // Fill Card Names
        string[] cardGuids = AssetDatabase.FindAssets("t:CardConfig", new[] { "Assets/ScriptableObject/SonatFramework/Configs/CardCollection/Cards" });
        foreach (string guid in cardGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardConfig card = AssetDatabase.LoadAssetAtPath<CardConfig>(path);
            if (card != null)
            {
                string[] parts = card.name.Split('_'); // Card_0_0
                if (parts.Length == 3)
                {
                    int albumIndex = GetIndexFromName(parts[1]);
                    int cardIndex = GetIndexFromName(parts[2]);
                    if (albumIndex >= 0 && albumIndex < 10 && cardIndex >= 0 && cardIndex < 9)
                    {
                        card.cardName = cardNames[albumIndex, cardIndex];
                        EditorUtility.SetDirty(card);
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Album names filled successfully!");
    }

    private static int GetIndexFromName(string str)
    {
        if (int.TryParse(str, out int index))
            return index;
        return -1;
    }
}
#endif
