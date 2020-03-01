using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// in change to react to disconnection ping
/// </summary>
public class NwkConnectionTimeout : NwkModules
{
  public override void onMessage(NwkMessage msg)
  {
    if (msg.messageType != (int)NwkMessageType.DISCONNECTION_PING) return;

    NwkMessage outgoing = new NwkMessage();
    outgoing.setSender(NwkClient.nwkUid);
    outgoing.setupNwkType(NwkMessageType.DISCONNECTION_PONG);

    NwkClient.nwkClient.sendClient.sendClientToServer(outgoing);
  }
}
