﻿using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// module that generate ping behavior
/// </summary>

public class NwkModPing : NwkModuleClient
{
  float pingTimer = 0f;
  float pingItv = 1f;

  NwkMessage msg;

  //public Action<float> onPong;

  float _pingTime = 0f;
  float _pongTime = 0f;
  float _lastDelta = 0f;

  protected override void setup()
  {
    base.setup();

    msg = new NwkMessage();
    msg.silentLogs = true; // dont log
    msg.setupNwkType(NwkMessageType.PING);
    
    pingTimer = pingItv;

    if(pingTimer <= 0f)
    {
      Debug.LogWarning("ping NOT active");
    }

  }

  protected override void updateNwk()
  {
    base.updateNwk();

    //Debug.Log(pingTimer);

    if (pingTimer > 0f)
    {
      pingTimer -= Time.deltaTime;
      if (pingTimer <= 0f)
      {
        pingTimer = pingItv;
        ping();
      }
    }

  }

  public void ping()
  {
    _pingTime = Time.realtimeSinceStartup;

    //_client.log("ping at "+_pingTime);

    //_client.sendClient.sendServerToClientTransaction(msg, _client.client.connection.connectionId, pong);
    _client.sendWrapperClient.sendClientToServer(msg);
  }

  public int pong()
  {
    _pongTime = Time.realtimeSinceStartup;
    if (_pingTime <= 0f) _pingTime = _pongTime;

    _lastDelta = _pongTime - _pingTime;

    int ms = getMilliSec(_lastDelta);
    //_client.log("pong = " + ms);

    //owner.getClientData(NwkClient.nwkUid)

    return ms;
    //if(onPong != null) onPong(dlt);
  }

  public int getCurrentPing() => getMilliSec(_lastDelta);

  static public int getMilliSec(float dlt)
  {
    int ms = Mathf.FloorToInt(dlt * 1000f);
    return ms;
  }

  public override void drawGui()
  {
    base.drawGui();

    //GUILayout.Label(new Rect(position.x, position.y + 30, size.x, 30f), "dt : "+_lastDelta);
    GUILayout.Label("dt : " + _lastDelta);
    GUILayout.Label("ping : " + getCurrentPing());

  }
}