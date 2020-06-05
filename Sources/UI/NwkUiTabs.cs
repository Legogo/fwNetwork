using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NwkUiTabs : MonoBehaviour
{
  //public Transform tabsPivot;
  public GameObject refTabButton;

  List<NwkUiTab> tabs = new List<NwkUiTab>();
  
  private void Start()
  {
    Transform pivot = refTabButton.transform.parent;
    Debug.Assert(pivot != null);

    refTabButton = pivot.transform.GetChild(0).gameObject; // le bouton générique
    Debug.Assert(refTabButton);

    refTabButton.gameObject.SetActive(false);

    refreshTabs(); // start de tabs
  }

  iNwkUiTab[] fetchTabs()
  {
    List<iNwkUiTab> tmp = new List<iNwkUiTab>();
    
    //should be Component ?
    NwkUiView[] views = GameObject.FindObjectsOfType<NwkUiView>();
    for (int i = 0; i < views.Length; i++)
    {
      iNwkUiTab tab = views[i] as iNwkUiTab;
      if (tab != null) tmp.Add(tab);
    }

    return tmp.ToArray();
  }

  public void refreshTabs()
  {
    iNwkUiTab[] _refTabs = fetchTabs();

    //Debug.Log("found " + _refTabs.Length + " tabs");

    for (int i = 0; i < _refTabs.Length; i++)
    {
      NwkUiTab _tab = null;

      if (tabs.Count > i) _tab = tabs[i];
      if (_tab == null)
      {
        _tab = new NwkUiTab();
        tabs.Add(_tab);
      }

      _tab.tabRef = _refTabs[i];

      if(_tab.tabButton == null)
      {
        _tab.tabButton = GameObject.Instantiate(refTabButton).GetComponentInChildren<Button>();
        _tab.tabButton.gameObject.SetActive(true); // ref is disabled

        //Debug.Log("created " + _tab.tabButton, _tab.tabButton);
      }

      //part of the tab group
      _tab.tabButton.transform.SetParent(refTabButton.transform.parent);

      Text label = _tab.tabButton.GetComponentInChildren<Text>();
      label.text = _tab.tabRef.getTabLabel();

    }

    //Debug.Log(tabs.Count + " > " + _refTabs.Length);

    while(tabs.Count > _refTabs.Length)
    {
      tabs[tabs.Count - 1].destroy();
      tabs.RemoveAt(tabs.Count - 1);
    }

  }

  public void tabButtonPress(Button clickedButton)
  {
    //Debug.Log(clickedButton.name);

    for (int i = 0; i < tabs.Count; i++)
    {
      NwkUiTab tab = getTabByName(tabs[i].tabButton.GetComponentInChildren<Text>().text);

      //Debug.Log(tab.tabRef);

      if (tabs[i].tabButton == clickedButton)
      {
        tab.tabRef.toggleTab();
      }
      else
      {
        tab.tabRef.hideTab();
      }
    }
  }

  NwkUiTab getTabByName(string tabName)
  {
    if (tabs.Count <= 0) return null;
    for (int i = 0; i < tabs.Count; i++)
    {
      if (tabs[i].tabRef.getTabLabel() == tabName) return tabs[i];
    }
    return null;
  }


  public static void loadView(string containsViewName, System.Action<bool> onComplete = null)
  {
    NwkUiView[] views = GameObject.FindObjectsOfType<NwkUiView>();
    for (int i = 0; i < views.Length; i++)
    {
      if (views[i].gameObject.scene.name.Contains(containsViewName))
      {
        Debug.LogWarning(containsViewName + " is already loaded ?");
        if(onComplete != null) onComplete(true);
        return;
      }
    }

    NwkSystemBase.nwkSys.StartCoroutine(loadingView(containsViewName, onComplete));
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

public class NwkUiTab
{
  public iNwkUiTab tabRef;
  public Button tabButton;

  public void destroy()
  {
    if (tabButton != null) GameObject.Destroy(tabButton.gameObject);
    tabRef = null;

    //Debug.Log("destroyed");
  }
}