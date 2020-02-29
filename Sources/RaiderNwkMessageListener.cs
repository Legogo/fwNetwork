using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RaiderNwkMessageListener : MonoBehaviour
{
  List<RaiderNwkMessageListenerCouple> msgs = new List<RaiderNwkMessageListenerCouple>();
  
  public void add(RaiderNwkMessage msg, Action<RaiderNwkMessage> onCompletion)
  {
    RaiderNwkMessageListenerCouple couple = new RaiderNwkMessageListenerCouple();
    couple.onMsgReceived += onCompletion;
    couple.originalMessage = msg;
    msgs.Add(couple);
  }

  public int solveReceivedMessage(RaiderNwkMessage msg)
  {
    int i = 0;
    while (i < msgs.Count)
    {
      if (msgs[i].originalMessage.isSameTransaction(msg))
      {
        msgs[i].onMsgReceived(msg);
        msgs[i].clear();

        msgs.RemoveAt(i);
      }
      else i++;
    }
    return msgs.Count;
  }

  public int getStackCount()
  {
    return msgs.Count;
  }

  public string toString()
  {
    string ct = "[listener]";
    for (int i = 0; i < msgs.Count; i++)
    {
      ct += "\n " + msgs[i].originalMessage.token;
    }
    return ct;
  }

  static public RaiderNwkMessageListener getListener()
  {
    return GameObject.FindObjectOfType<RaiderNwkMessageListener>();
  }
}

public struct RaiderNwkMessageListenerCouple
{
  public Action<RaiderNwkMessage> onMsgReceived;
  public RaiderNwkMessage originalMessage;

  public void clear()
  {
    onMsgReceived = null;
    originalMessage = null;
  }
}
