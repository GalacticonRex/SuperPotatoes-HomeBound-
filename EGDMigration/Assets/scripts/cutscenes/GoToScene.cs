using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToScene : MonoBehaviour, ICutsceneAction
{
    public string SceneName;
    private void Start()
    {
        AlphaController alpha = GetComponent<AlphaController>();
        alpha.Action = this;
    }
    void ICutsceneAction.PerformAction()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
    }
}
