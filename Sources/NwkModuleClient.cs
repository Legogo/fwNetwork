using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// unused for now
/// </summary>

abstract public class NwkModuleClient : NwkModule
{
  protected NwkClient _client;

  protected override void setup()
  {
    base.setup();

    _client = owner as NwkClient;

    if(_client == null)
    {
      Debug.LogError("can't create a " + GetType() + " for non client setup");
      Debug.Log(owner);
      GameObject.Destroy(this);
    }
  }

  protected override bool canUpdate()
  {
    //Debug.Log(GetType() + " " + _client.client.isConnected);

    if (!_client.isConnected()) return false;
    return true;
  }

}
