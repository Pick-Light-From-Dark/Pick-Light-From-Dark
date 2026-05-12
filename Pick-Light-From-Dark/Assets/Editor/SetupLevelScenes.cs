using UnityEngine;
using Game.Flow;

public class SetupLevelScenes
{
    public static void Execute()
    {
        // Setup Level1
        var coordinator1 = GameObject.FindObjectOfType<LevelFlowCoordinator>();
        if (coordinator1 != null)
        {
            coordinator1.openingStory = Resources.Load<TextAsset>("Dialogue/Dialogue2_pre");
            coordinator1.endingStory = Resources.Load<TextAsset>("Dialogue/Dialogue2_post");
            coordinator1.levelConfig = Resources.Load<Game.Config.LevelConfigSO>("Config/LevelConfig_2");
            coordinator1.levelId = 2;
            coordinator1.nextLevelSceneName = "";
            coordinator1.currentLevelSceneName = "Level2";

            var vnController = GameObject.FindObjectOfType<Game.Test.FungusVNController>();
            if (vnController != null)
            {
                coordinator1.vnController = vnController;
            }
            Debug.Log("[SetupLevelScenes] Level1 configured OK");
        }
        else
        {
            Debug.LogError("[SetupLevelScenes] No LevelFlowCoordinator found in scene");
        }
    }
}
