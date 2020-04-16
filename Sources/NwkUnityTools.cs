using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NwkUnityTools
{

  /// <summary>
  /// loaded or loading (but called to be loaded at least)
  /// </summary>
  /// <param name="endName"></param>
  /// <returns></returns>
  static public bool isSceneAdded(string endName)
  {
    for (int i = 0; i < SceneManager.sceneCount; i++)
    {
      Scene sc = SceneManager.GetSceneAt(i);
      //Debug.Log(sc.name + " , valid ? " + sc.IsValid() + " , loaded ? " + sc.isLoaded);
      if (sc.name.Contains(endName))
      {
        return true;
      }
    }

    return false;
  }

  static public AsyncOperation loadScene(string nm)
  {
    if (isSceneAdded(nm)) return null;

    AsyncOperation async = SceneManager.LoadSceneAsync(nm, LoadSceneMode.Additive);

    return async;
  }
}
