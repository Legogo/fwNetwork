using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class NwkMono : MonoBehaviour
{
  protected NwkClient nwkClient;
  protected NwkServer nwkServer;

  virtual protected void Awake()
  {
    build();
  }

  virtual protected void build()
  {
  }

  private void Start()
  {
    enabled = false;

    setup();
  }

  virtual protected void setup()
  {
    NwkSystemBase nwkCtx = NwkSystemBase.nwkSys;
    nwkClient = nwkCtx as NwkClient;
    nwkServer = nwkCtx as NwkServer;

    enabled = true;
  }

  public void log(string ct) => NwkSystemBase.nwkSys.log(ct);
  protected string getStamp() => "<color=gray>" + GetType() + "</color> | ";
}
