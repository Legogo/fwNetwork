using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NwkMessageListener : MonoBehaviour
{
  List<NwkMessageListenerCouple> msgs = new List<NwkMessageListenerCouple>();
  
  public void add(NwkMessage msg, Action<NwkMessage> onCompletion)
  {
    NwkMessageListenerCouple couple = new NwkMessageListenerCouple();
    couple.onMsgReceived += onCompletion;
    couple.originalMessage = msg;
    msgs.Add(couple);
  }

  public int solveReceivedMessage(NwkMessage msg)
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

  static public NwkMessageListener getListener()
  {
    return GameObject.FindObjectOfType<NwkMessageListener>();
  }
}

public struct NwkMessageListenerCouple
{
  public Action<NwkMessage> onMsgReceived;
  public NwkMessage originalMessage;

  public void clear()
  {
    onMsgReceived = null;
    originalMessage = null;
  }
}
