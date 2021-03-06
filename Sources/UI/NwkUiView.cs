﻿using UnityEngine;

/// <summary>
/// tabs compatible
/// </summary>

abstract public class NwkUiView : MonoBehaviour, iNwkUiTab
{
  Canvas _canvas;
  public CanvasGroup groupData;

  private void Awake()
  {
    build();
  }

  private void Start()
  {
    setup();
  }

  virtual protected void build()
  {
    _canvas = GetComponent<Canvas>();
    hide(); // instant hide
  }

  virtual protected void setup()
  {
    NwkUiTabs tabs = GameObject.FindObjectOfType<NwkUiTabs>();
    NwkUiView[] views = GameObject.FindObjectsOfType<NwkUiView>();

    if(views.Length > 0)
    {
      //hide(); // default is not visible when tabs is coming

      if (tabs == null) NwkUiTabs.loadView("tabs", delegate(bool success)
      {
        //failed to load tabs view
        if (!success) show();
      });
    }
    else
    {
      Debug.Log(name + " refreshing tabs", transform);
      tabs.refreshTabs(); // nwk ui view setup
    }

    //hide();
  }

  private void OnDestroy()
  {
    destroy();
  }

  virtual protected void destroy()
  {
    gameObject.SetActive(false);

    if(Application.isPlaying)
    {
      //force refresh tabs
      NwkUiTabs views = GameObject.FindObjectOfType<NwkUiTabs>();
      if (views != null) views.refreshTabs(); // destroy ui view
    }

  }

  public void toggleVisible()
  {
    if (_canvas.enabled) hide();
    else show();

    //Debug.Log(name+" => "+_canvas.enabled);
  }

  public void show()
  {
    _canvas.enabled = true;
    //Debug.Log(name + " => show");
  }

  public void hide()
  {
    _canvas.enabled = false;
    //Debug.Log(name + " => hide");
  }


  virtual public string getTabLabel() => GetType().ToString();

  public void showTab() => show();
  public void hideTab() => hide();
  public void toggleTab() => toggleVisible();
}
