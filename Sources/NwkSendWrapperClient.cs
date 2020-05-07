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

  public void sendClientToServer(iNwkMessage message, bool reliable = true)
  {
    Debug.Assert(message != null, "no message given ?");

    //bool silent = false;
    //NwkMessageFull mFull = message as NwkMessageFull;
    //if (mFull != null) silent = mFull.isSilent();

    string sId = "";
    iNwkMessageId id = message as iNwkMessageId;
    if(id != null)
    {
      sId = id.getIdCard().getMessageSender()+"<>"+id.getIdCard().getMessageType();
    }

    //NwkSystemBase.nwkSys.log("<b>sending "+message.GetType()+"</b> "+ sId, message.isSilent());
    
    Debug.Assert((message as MessageBase) != null, "can't cast message to unet type ?");
    if (!reliable)
    {
      //0 is reliable, 1 unreliable
      unetClient.SendByChannel(message.getMessageUnetId(), message as MessageBase, 1);
    }
    else
    {
      unetClient.Send(message.getMessageUnetId(), message as MessageBase);
    }
    
  }

}
