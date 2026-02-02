using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayGame : MonoBehaviour
{
    private float delay = 2;
    private void OnEnable()
    {
        DestroyAllDontDestroyOnLoad();
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(0);
    }


    public void DestroyAllDontDestroyOnLoad()
    {
        // Unity giữ các object này trong một scene ẩn đặc biệt
        var ignoreObjects = new List<string> {
            "ResourceManagerCallbacks",
            "Firebase Services",
            "UnityFacebookSDKPlugin"
        };
        var go = new GameObject("Destroyer");
        DontDestroyOnLoad(go);

        foreach (var root in go.scene.GetRootGameObjects())
        {
            if (root.name != "Destroyer" && !ignoreObjects.Contains(root.name)) // tránh tự xoá mình
                Destroy(root);
        }

        Destroy(go); // dọn dẹp luôn helper
    }
}
