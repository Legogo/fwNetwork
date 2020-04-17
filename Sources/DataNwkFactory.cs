using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// 
/// https://docs.unity3d.com/ScriptReference/EditorBuildSettingsScene.html
/// 
/// </summary>

[CreateAssetMenu(menuName = "sarko/create DataNwkFactory", order = 100)]
public class DataNwkFactory : ScriptableObject
{

  public GameObject[] items;
  //public NwkFactoryItem[] items;

  public GameObject copy(string type)
  {

    for (int i = 0; i < items.Length; i++)
    {
      if(items[i].name == type)
      {
        return GameObject.Instantiate(items[i]);
      }
    }

    Debug.LogWarning("no object " + type + " in factory");

    return null;
  }
}

[System.Serializable]
public struct NwkFactoryItem
{
  public string type;
  public GameObject prefab;
}