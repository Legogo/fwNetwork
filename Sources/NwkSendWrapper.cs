using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// element that manage sending messages
/// </summary>
abstract public class NwkSendWrapper
{
  List<NwkMessage> stack = new List<NwkMessage>();

  public void add(NwkMessage msg)
  {
    stack.Add(msg);
  }

  public void update(float dt)
  {
    if (stack.Count <= 0) return;

    for (int i = 0; i < stack.Count; i++)
    {
      
    }
  }

}
