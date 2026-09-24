using UnityEngine;
using UnityEngine.SceneManagement;

public class ConcursilloV2MusicBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsConcursilloV2Scene(scene.name))
        {
            MusicManager.EnsureInstance();
            SoundEffectsManager.EnsureInstance();
            return;
        }

        MusicManager.DestroyInstance();
        SoundEffectsManager.DestroyInstance();
    }

    private static bool IsConcursilloV2Scene(string sceneName)
    {
        return sceneName == "TeamChooser"
            || sceneName == "MiniGameTeamNameScene"
            || sceneName == "MiniGameScene";
    }
}
