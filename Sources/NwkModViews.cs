using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NwkModViews : NwkModule
{
  [Header("a view that will be visible on startup")]
  public string openedStartupViewName;

  [Header("what views need to be added on startup")]
  public bool logView = false;

  //protected NwkUiViewLogs nvLogs = null;
  protected NwkUiTabs tabs = null; // might not exist if not needed

  protected override void setupModule()
  {
    base.setupModule();

    //tabs is loaded when multiple views are present
    //tabs = GameObject.FindObjectOfType<NwkUiTabs>();

    loadViews();
  }

  void loadViews()
  {
    string[] nms = getViewNames();

    int count = nms.Length;
    for (int i = 0; i < nms.Length; i++)
    {
      string viewShortName = nms[i];

      NwkUiTabs.loadView(viewShortName, delegate (bool success)
      {
        if (tabs == null) tabs = GameObject.FindObjectOfType<NwkUiTabs>();

        if (openedStartupViewName.Length > 0)
        {
          //Debug.Log(viewShortName + " == " + openedStartupViewName);

          if (viewShortName.Contains(openedStartupViewName))
          {
            iNwkUiTab tab = NwkUiTabs.getTabByNameGlobal(viewShortName);

            Debug.Log(tab);

            tab?.showTab();
            //tabs.getTabByName(viewShortName).showTab();
          }
        }


        count--;
        if (count <= 0) onAllViewsLoaded();
      });
    }

  }

  void onAllViewsLoaded()
  {
    //...
  }

  public string[] getViewNames() => gatherNames().ToArray();

  virtual protected List<string> gatherNames()
  {
    List<string> ct = new List<string>();
    if (logView) ct.Add("logs");
    return ct;
  }

  public T getTab<T>() where T : iNwkUiTab
  {
    if (tabs != null) return tabs.getTabByType<T>();
    else return default(T);
  }
}
