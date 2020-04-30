using System;
using System.Diagnostics;
using UnityEngine.Networking;

/// <summary>
/// 
/// CLIENT TO SERVER
/// 
/// </summary>

public class NwkSendWrapperClient : NwkSendWrapper
{
  NetworkClient unetClient;

  public NwkSendWrapperClient(NetworkClient unetClient)
  {
    this.unetClient = unetClient;
  }

  public void sendClientToServer(iNwkMessageId message)
  {
    Debug.Assert(message != null, "no message given ?");

    NwkSystemBase.nwkSys.log("<b>sending "+message.GetType()+"</b> | " + message.getIdCard().toString());
    
    Debug.Assert((message as MessageBase) != null, "can't cast message to unet type ?");

    unetClient.Send(message.getMessageId(), message as MessageBase);
  }

}
