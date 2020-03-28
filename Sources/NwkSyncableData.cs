using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NwkSyncableData
{
  public string syncUid;
  public INwkSyncable handle;

  //for syncer
  float sendTimer;
  float sendFrequency;

  public NwkSyncableData(INwkSyncable parent, float freq)
  {
    handle = parent;
    sendTimer = 0f;
    sendFrequency = freq;
  }

  public bool update(float dt)
  {
    sendTimer += dt;

    if (sendTimer > sendFrequency)
    {
      sendTimer = 0f;
      return true;
    }

    return false;
  }

  public NwkMessage packMessage() => NwkSyncer.nwkSyncInject(this);
  public void unpackMessage(NwkMessage msg) => handle.unpack(msg.getMessage());

}
