using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiderNwkConnectionTimeout : RaiderNwkModules
{
  public override void onMessage(RaiderNwkMessage msg)
  {

    if (msg.messageType != RaiderNwkMessageType.DISCONNECTION_PING) return;

    RaiderNwkMessage output = new RaiderNwkMessage();

    output.setupType(RaiderNwkMessageType.DISCONNECTION_PONG).sendToServer(RaiderNwkClient.nwkUid, client.client);
  }
}
