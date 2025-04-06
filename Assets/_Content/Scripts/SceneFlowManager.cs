using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowManager : MonoBehaviour
{
    public void LoadTitleScene()
    {
        SceneManager.LoadScene("Title");
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene("Main");
    }
}
