using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NwkMono : MonoBehaviour
{
  protected NwkClient nwkClient;
  protected NwkServer nwkServer;

  virtual protected void Awake()
  {
    NwkSystemBase nwkCtx = NwkSystemBase.nwkSys;
    nwkClient = nwkCtx as NwkClient;
    nwkServer = nwkCtx as NwkServer;
  }

  public void log(string ct) => NwkSystemBase.nwkSys.log(ct);
  protected string getStamp() => "<color=gray>" + GetType() + "</color> | ";
}
