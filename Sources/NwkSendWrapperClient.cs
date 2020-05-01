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

    //bool silent = false;
    //NwkMessageFull mFull = message as NwkMessageFull;
    //if (mFull != null) silent = mFull.isSilent();

    NwkSystemBase.nwkSys.log("<b>sending "+message.GetType()+"</b> | " + message.getIdCard().toString(), message.isSilent());
    
    Debug.Assert((message as MessageBase) != null, "can't cast message to unet type ?");

    unetClient.Send(message.getMessageId(), message as MessageBase);
  }

}
