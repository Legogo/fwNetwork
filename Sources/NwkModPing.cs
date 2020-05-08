using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

/// <summary>
/// module that generate ping behavior
/// </summary>

public class NwkModPing : NwkModuleClient
{
  float pingTimer = 0f;
  public float pingItv = 1f; // won't work if go through 0

  NwkMessageBasic pingMessage;

  //public Action<float> onPong;

  float _pingTime = 0f;
  float _pongTime = 0f;
  float _lastDelta = 0f;

  List<float> _lastDeltas = new List<float>();

  protected override void setupModule()
  {
    base.setupModule();

    pingMessage = new NwkMessageBasic();
    pingMessage.getIdCard().setMessageType(eNwkMessageType.PING);
    
    pingTimer = pingItv;

    if(pingTimer <= 0f)
    {
      Debug.LogWarning("ping NOT active");
    }

  }

  private void OnValidate()
  {
    pingTimer = pingItv;
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

    pingMessage.getIdCard().setMessageSender(NwkClient.nwkUid);
    _client.sendWrapperClient.sendClientToServer(pingMessage, false);

    //Debug.Log("ping ! " + _pingTime);
  }

  /// <summary>
  /// called on pong reception
  /// </summary>
  public int pong()
  {
    _pongTime = Time.realtimeSinceStartup;
    if (_pingTime <= 0f) _pingTime = _pongTime; // first time

    _lastDelta = _pongTime - _pingTime;
    _lastDeltas.Add(_lastDelta);
    if(_lastDeltas.Count > 10) _lastDeltas.RemoveAt(0);

    //Debug.Log(_pingTime+" -> "+ _pongTime + " => " + _lastDelta);

    int ms = getMilliSec(_lastDelta);
    return ms;
  }

  /// <summary>
  /// in secondes
  /// </summary>
  public float getRawPing() => _lastDelta;

  /// <summary>
  /// in millisec
  /// </summary>
  public float getCurrentPing(bool ms = true)
  {
    float avg = 0f;
    for (int i = 0; i < _lastDeltas.Count; i++)
    {
      avg += _lastDeltas[i];
    }
    avg /= _lastDeltas.Count;

    if (!ms) return avg;

    return getMilliSec(avg);
  }

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
