using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeScene : MonoBehaviour
{
    
    // Change the scene to the scene with the given name
    public void ChangeToScene(string sceneName)
    {
        // Load the scene with the given name
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
