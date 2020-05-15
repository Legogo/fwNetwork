using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NwkUiTabs : MonoBehaviour
{
  public Transform tabsPivot;
  GameObject refTabButton;
  
  private void Start()
  {
    refTabButton = tabsPivot.transform.GetChild(0).gameObject;
    Debug.Assert(refTabButton);

    //viewsPivot = refTabButton.transform.parent;

    refreshTabs();
  }
  
  void clearView()
  {
    if(tabsPivot != null)
    {
      while (tabsPivot.childCount > 1)
      {
        GameObject.DestroyImmediate(tabsPivot.GetChild(0).gameObject);
      }
    }
    
    if(refTabButton != null)
    {
      refTabButton.SetActive(false);
    }
  }

  public void refreshTabs()
  {
    clearView();

    NwkUiView[] views = GameObject.FindObjectsOfType<NwkUiView>();

    Debug.Log("refreshing tabs " + views.Length);

    for (int i = 0; i < views.Length; i++)
    {
      GameObject view = refTabButton;

      if (i > 0)
      {
        view = GameObject.Instantiate(refTabButton);
        view.transform.SetParent(tabsPivot);
      }

      view.gameObject.SetActive(true);

      Text label = view.GetComponentInChildren<Text>();
      label.text = views[i].getTabLabel();
    }
  }

  public void tabButtonPress(Button refButton)
  {
    Button[] buttons = tabsPivot.GetComponentsInChildren<Button>();
    for (int i = 0; i < buttons.Length; i++)
    {
      NwkUiView view = getViewByTabName(buttons[i].GetComponentInChildren<Text>().text);

      if (buttons[i] == refButton)
      {
        view.toggleVisible();
      }
      else
      {
        view.hide();
      }
    }
  }

  NwkUiView getViewByTabName(string tabName)
  {
    NwkUiView[] views = GameObject.FindObjectsOfType<NwkUiView>();
    for (int i = 0; i < views.Length; i++)
    {
      if (views[i].getTabLabel() == tabName) return views[i];
    }
    return null;
  }


  public static void loadView(string viewName, System.Action<bool> onComplete = null)
  {
    NwkSystemBase.nwkSys.StartCoroutine(loadingView(viewName, onComplete));
  }

  protected static IEnumerator loadingView(string viewName, System.Action<bool> onComplete = null)
  {
    if (!isSceneInBuildSettings(viewName))
    {
      if(onComplete != null) onComplete(false);
      yield break;
    }

    string path = getScenePathInBuildSettings(viewName);
    int idx = SceneUtility.GetBuildIndexByScenePath(path);

    AsyncOperation async = SceneManager.LoadSceneAsync(idx, LoadSceneMode.Additive);

    if (async != null)
    {
      while (!async.isDone) yield return null;
    }

    if (onComplete != null) onComplete(true);
  }

  protected static string getScenePathInBuildSettings(string sceneNameContains)
  {
    for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
    {
      string path = SceneUtility.GetScenePathByBuildIndex(i);
      if (path.Contains(sceneNameContains)) return path;
    }
    return "";
  }

  protected static bool isSceneInBuildSettings(string nmContains)
  {

    for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
    {
      string path = SceneUtility.GetScenePathByBuildIndex(i);
      if (path.Contains(nmContains)) return true;
    }

    return false;
  }

}
