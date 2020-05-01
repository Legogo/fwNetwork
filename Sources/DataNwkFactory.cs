using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// 
/// https://docs.unity3d.com/ScriptReference/EditorBuildSettingsScene.html
/// 
/// </summary>

[CreateAssetMenu(menuName = "fwNetwork/create DataNwkFactory", order = 100)]
public class DataNwkFactory : ScriptableObject
{

  //public GameObject[] items;
  public NwkFactoryItem[] items;

  public GameObject copy(int typeIndex)
  {

    if(typeIndex < items.Length)
    {
      return GameObject.Instantiate(items[typeIndex].prefab);
    }

    Debug.LogWarning("no object for index " + typeIndex + " in factory of total count : "+items.Length);

    return null;
  }

  public short getItemIndex(Transform tr)
  {
    for (short i = 0; i < items.Length; i++)
    {
      if(items[i].prefab.name == tr.name)
      {
        return i;
      }
    }
    return -1;
  }

  public string getItemName(short index)
  {
    return items[index].prefab.name;
  }
}

[System.Serializable]
public struct NwkFactoryItem
{
  public string type;
  public GameObject prefab;
}