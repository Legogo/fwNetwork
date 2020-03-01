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
    if (msg.nwkMsgType != NwkMessageType.DISCONNECTION_PING) return;

    NwkMessage output = new NwkMessage();
    output.setSender(NwkClient.nwkUid);
    output.setupNwkType(NwkMessageType.DISCONNECTION_PONG);

    NwkClient.nwkClient.sendToServer(output);
  }
}
