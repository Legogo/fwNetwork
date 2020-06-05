using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// unused for now
/// </summary>

abstract public class NwkModule : NwkMono
{
  protected NwkSystemBase owner;

  //NwkServer _server;

  void Start()
  {
    //if(GameObject.FindObjectOfType<NwkSyncer>() == null) log("<color=red>no syncer</color> ; can't sync stuff");

    owner = GameObject.FindObjectOfType<NwkSystemBase>();
    Debug.Assert(owner != null, "no nwk system ?");

    setupModule();
  }

  virtual protected void setupModule()
  {

    Debug.Log("creating " + GetType() + " module");

  }

  void Update()
  {
    if (canUpdate())
    {
      updateNwk();
    }
  }

  virtual protected bool canUpdate() { return true; }

  virtual protected void updateNwk()
  { }

  //abstract public void onMessage(NwkMessage msg);

  /// <summary>
  /// describe what to display on ui
  /// returns its size
  /// </summary>
  virtual public void drawGui()
  {
    
    //GUI.Label(new Rect(position.x + 10, position.y + 10, size.x, 20f), GetType().ToString());
    GUILayout.Label(GetType().ToString());

  }

}