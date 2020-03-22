using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// structure to describe a client
/// </summary>
public class NwkClientData
{
  public enum ClientState { CONNECTED, DISCONNECTED };
  public ClientState state = ClientState.CONNECTED;

  public string uid;
  public float timeout = -1f;
  public float ping = -1f; // on server it shows last ping time

  public void resetTimeout()
  {
    timeout = -1f;
  }

  /// <summary>
  /// called by server on disconnection
  /// </summary>
  public void startTimeout(float target = 5f)
  {
    timeout = 5f;
  }

  /// <summary>
  /// once server received a disconnection
  /// it will update this for everybody
  /// </summary>
  public bool updateTimeout(float timeStep)
  {
    if(timeout > 0f)
    {
      float next = timeout - timeStep;

      if (next > 0f && timeout < 0f)
      {
        timeout = -1f;

        setAsDisconnected();
        return true;
      }
    }

    return false;
  }

  public void setAsDisconnected()
  {
    state = ClientState.DISCONNECTED;
  }

  public bool isDisconnected() { return state == ClientState.DISCONNECTED; }
}
