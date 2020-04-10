using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// everybody can create a new sync object
/// server will register and send to everybody else
/// everybody must be able to recreate that object in local sim
/// </summary>

public class NwkSyncableData
{
  public string syncNwkUid; // owner uid
  public string syncUid; // given to msg header

  public INwkSyncable handle;

  //for syncer
  float sendTimer;
  float sendFrequency;

  public NwkSyncableData(INwkSyncable parent, float freq)
  {
    syncUid = NwkClient.generateUniqId();

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

  public void forceSend() => sendTimer = sendFrequency - 0.000001f; // next frame !

  public NwkMessage packMessage() => NwkSyncer.nwkSyncInject(this);
  public void unpackMessage(NwkMessage msg) => handle.unpack(msg.getMessage());

}
