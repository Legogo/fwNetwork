using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NwkMono : MonoBehaviour
{
  public void log(string ct) => NwkSystemBase.nwkSys.log(ct);
  protected string getStamp() => "<color=gray>" + GetType() + "</color> | ";
}
