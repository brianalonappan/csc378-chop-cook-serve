using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneOnEnter : MonoBehaviour
{
    public string nextSceneName;
    public float autoAdvanceTime = 30f;

    private bool loadingScene = false;

    void Start()
    {
        Invoke("LoadNextScene", autoAdvanceTime);
    }

    void Update()
    {
        if (!loadingScene && Input.GetKeyDown(KeyCode.Return))
        {
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        if (loadingScene) return;

        loadingScene = true;
        SceneManager.LoadScene(nextSceneName);
    }
}