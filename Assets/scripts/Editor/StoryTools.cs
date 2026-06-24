using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class StoryTools
{
    [MenuItem("Tools/Abyss of Sins/Setup Story Scenes", false, 0)]
    public static void SetupStoryScenes()
    {
        CreateIntroData();
        CreatePostCreditsData();
        CreateIntroScene();
        CreatePostCreditsScene();
        AddScenesToBuild();
        Debug.Log("Story scenes created. Open Scenes/IntroScene or Scenes/PostCreditsScene and press Play.");
    }

    private static void CreateIntroData()
    {
        StoryData data = ScriptableObject.CreateInstance<StoryData>();
        data.slides = new StorySlide[]
        {
            new StorySlide
            {
                speaker = "Narrador",
                text = "En las profundidades del abismo, donde el fuego eterno devora las almas perdidas...",
                textColor = Color.white,
                panelTint = Color.white
            },
            new StorySlide
            {
                speaker = "Lucian",
                text = "He caído más bajo de lo que imaginé. El infierno no es un lugar... es un estado del alma.",
                textColor = new Color(1f, 0.7f, 0.3f),
                panelTint = Color.white
            },
            new StorySlide
            {
                speaker = "???",
                text = "Lucian... nos volvemos a encontrar. Esta vez no habrá escapatoria.",
                textColor = new Color(1f, 0.2f, 0.2f),
                panelTint = Color.white
            },
            new StorySlide
            {
                speaker = "Lucian",
                text = "No busco escapar. He venido a terminar esto.",
                textColor = new Color(1f, 0.7f, 0.3f),
                panelTint = Color.white
            }
        };
        string path = "Assets/Story/IntroData.asset";
        EnsureFolder("Assets/Story");
        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        Debug.Log("Created: " + path);
    }

    private static void CreatePostCreditsData()
    {
        StoryData data = ScriptableObject.CreateInstance<StoryData>();
        data.slides = new StorySlide[]
        {
            new StorySlide
            {
                speaker = "Narrador",
                text = "Y así, tras siglos de batalla, el abismo encontró la paz...",
                textColor = Color.white,
                panelTint = Color.white
            },
            new StorySlide
            {
                speaker = "Lucian",
                text = "Todo pecado tiene su precio. Todo dolor, su propósito.",
                textColor = new Color(1f, 0.7f, 0.3f),
                panelTint = Color.white
            },
            new StorySlide
            {
                speaker = "",
                text = "FIN.\n\nGracias por jugar Abyss of Sins.",
                textColor = new Color(0.8f, 0.8f, 0.8f),
                panelTint = Color.white
            }
        };
        string path = "Assets/Story/PostCreditsData.asset";
        EnsureFolder("Assets/Story");
        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        Debug.Log("Created: " + path);
    }

    private static void CreateIntroScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject storyGO = new GameObject("StoryManager");
        StoryManager sm = storyGO.AddComponent<StoryManager>();
        SerializedObject so = new SerializedObject(sm);
        so.FindProperty("storyData").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<StoryData>("Assets/Story/IntroData.asset");
        so.FindProperty("nextSceneName").stringValue = "p1";
        so.FindProperty("showSkipButton").boolValue = true;
        so.ApplyModifiedProperties();

        string path = "Assets/Scenes/IntroScene.unity";
        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("Created: " + path);
    }

    private static void CreatePostCreditsScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject storyGO = new GameObject("StoryManager");
        StoryManager sm = storyGO.AddComponent<StoryManager>();
        SerializedObject so = new SerializedObject(sm);
        so.FindProperty("storyData").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<StoryData>("Assets/Story/PostCreditsData.asset");
        so.FindProperty("nextSceneName").stringValue = "";
        so.FindProperty("showSkipButton").boolValue = true;
        so.ApplyModifiedProperties();

        string path = "Assets/Scenes/PostCreditsScene.unity";
        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("Created: " + path);
    }

    private static void AddScenesToBuild()
    {
        var scenes = EditorBuildSettings.scenes;
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes);

        void AddIfMissing(string path)
        {
            if (!list.Exists(s => s.path == path))
                list.Add(new EditorBuildSettingsScene(path, true));
        }

        AddIfMissing("Assets/Scenes/IntroScene.unity");
        AddIfMissing("Assets/Scenes/PostCreditsScene.unity");

        if (!list.Exists(s => s.path == "Assets/Scenes/p1.unity"))
            list.Add(new EditorBuildSettingsScene("Assets/Scenes/p1.unity", true));

        EditorBuildSettings.scenes = list.ToArray();
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
