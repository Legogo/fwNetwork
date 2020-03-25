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
    owner = GameObject.FindObjectOfType<NwkSystemBase>();
    Debug.Assert(owner != null, "no nwk system ?");

    setup();
  }

  virtual protected void setup()
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

  abstract protected bool canUpdate();

  virtual protected void updateNwk()
  { }

  //abstract public void onMessage(NwkMessage msg);

}
