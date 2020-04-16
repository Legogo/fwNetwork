using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// structure to describe a client
/// </summary>
public class NwkClientData
{

  public struct NwkClientDataMessageSize
  {
    public float msgStamp;
    public float msgSize;
  }

  public enum ClientState { CONNECTED, DISCONNECTED };
  public ClientState state = ClientState.DISCONNECTED;

  public string nwkUid;
  public int connId;

  float ping = 0f; // on server it shows time elapsed since last ping reception
  float timeoutLimit = 5f;

  public float sizeSeconds; // quantité de data dans le laps de temps

  public List<float> msgSizes = new List<float>(); // only within timeframe
  float sizesTimer = 0f;
  float sizesTime = 1f;

  public NwkClientData(string nwkUid)
  {
    this.nwkUid = nwkUid;

    sizesTime = sizesTimer;
    msgSizes.Clear();

    ping = Time.realtimeSinceStartup; // for server side
  }

  /// <summary>
  /// must be called by client (or server)
  /// </summary>
  public void update(bool serverUpdate)
  {
    if (state == ClientState.DISCONNECTED) return;

    if(sizesTimer > 0f)
    {
      sizesTimer -= Time.deltaTime;
      if(sizesTimer <= 0f)
      {
        sizesTimer = sizesTime;

        sizeSeconds = 0f;
        for (int i = 0; i < msgSizes.Count; i++)
        {
          sizeSeconds += msgSizes[i];
        }
        
        msgSizes.Clear();
      }
    }

  }

  public void eventPing(float dlt)
  {
    //if(NwkSystemBase.isServer()) ping = Time.realtimeSinceStartup;
    ping = dlt;
  }

  /// <summary>
  /// used in server context
  /// return true only on timeout frame !
  /// </summary>
  public bool updateTimeout(float curTime)
  {
    if (isDisconnected()) return false;

    float elasped = Mathf.FloorToInt(curTime - ping);

    if (elasped > timeoutLimit)
    {
      setAsDisconnected();
      return true;
    }

    return false;
  }

  public float getPingValue() => ping;

  public int getPingDelta()
  {
    if (state == ClientState.DISCONNECTED) return -1;

    float output = ping; // default is client ping

    if (NwkSystemBase.isServer())
    {
      if (output <= 0f) output = Time.realtimeSinceStartup;
      output = Time.realtimeSinceStartup - output; // server is last seen time
    }

    //return NwkModPing.getMilliSec(output);
    return Mathf.FloorToInt(output); // ms
  }

  public void setConnected()
  {
    state = ClientState.CONNECTED;

    ping = Time.realtimeSinceStartup; // for server side

    NwkSystemBase.nwkSys.log(nwkUid + " setup as connected !");
  }

  public void setAsDisconnected()
  {
    state = ClientState.DISCONNECTED;
    ping = -1f;

    NwkSystemBase.nwkSys.log(nwkUid + " disconnected !");
  }

  public bool isDisconnected() { return state == ClientState.DISCONNECTED; }
}
