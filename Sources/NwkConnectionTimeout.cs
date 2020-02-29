using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NwkConnectionTimeout : NwkModules
{
  public override void onMessage(NwkMessage msg)
  {

    if (msg.messageType != NwkMessageType.DISCONNECTION_PING) return;

    NwkMessage output = new NwkMessage();

    output.setupType(NwkMessageType.DISCONNECTION_PONG).sendToServer(NwkClient.nwkUid, client.client);
  }
}
