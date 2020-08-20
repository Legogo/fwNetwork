using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// element that manage sending messages
/// 
/// SERVER
/// to client (need connId)
/// transaction (connId)
/// broadcast
/// 
/// CLIENT
/// to server
/// 
/// </summary>
abstract public class NwkSendWrapper
{
  List<MessageBase> msgs = new List<MessageBase>();

  float timerSafe = 0f; // timer qui permet de reset le count régulièrement (si on envoi pas bcp de messages)
  float timerLock = 0f; // timer qui permet de bloquer l'envoi des messages pendant un temps
  float sendLockFrequency = 0.05f; // delta où on doit pas envoyer trop de messages

  int maxPerBatch = 5; // qté d'envoi de messages en 1 frame
  int sentLimitCount = 5; // qté d'envoi de messages avant de déclancher le lock
  int sentCount = 0; // tracking du compte des envois

  public NwkSendWrapper()
  {
    timerSafe = sendLockFrequency;
  }

  public void update()
  {
    //lock sending message for a little while
    if(timerLock > 0f)
    {
      timerLock -= Time.deltaTime;
      if (timerLock < 0f)
      {
        sentCount = 0;
        timerLock = 0f;
        timerSafe = sendLockFrequency;
      }
      return;
    }

    //send batches
    if(sentCount < sentLimitCount)
    {
      if (msgs.Count > 0)
      {
        sentCount += sendABatch();


        //when at limit, reset count and lock
        if (sentCount > sentLimitCount)
        {
          sentCount = 0;
          timerSafe = 0f;
          timerLock = sendLockFrequency;
        }
      }
    }

    // don't need to lock if don't send a lot of messages
    if (timerSafe > 0f)
    {
      timerSafe -= Time.deltaTime;
      if(timerSafe < 0f)
      {
        timerSafe = sendLockFrequency;
        sentCount = 0;
      }
    }

  }

  /// <summary>
  /// return total sent
  /// </summary>
  int sendABatch()
  {
    throw new System.NotImplementedException("no yet implem");

    //send a pack
    int toSend = Mathf.Min(msgs.Count, maxPerBatch);
    for (int i = 0; i < toSend; i++)
    {
      //send(msgs[i]);
    }

    //clear sent from list
    while (toSend > 0 && msgs.Count > 0)
    {
      msgs.RemoveAt(0);
    }

    return toSend;
  }

  public void subMessage(MessageBase msg)
  {
    msgs.Add(msg);
  }

  //abstract protected void send(int sendType, MessageBase msg);
}
