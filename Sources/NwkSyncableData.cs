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

  //for syncer to work with
  float _sendTimer; // local timer
  float _sendFrequency; // time interval to send pack to server

  public NwkSyncableData(INwkSyncable parent, float freq)
  {
    syncUid = NwkClient.generateUniqId();

    handle = parent;

    _sendTimer = 0f;
    _sendFrequency = freq;

    //GameObject.FindObjectOfType<NwkSyncer>().sub(parent);
  }

  //public float 

  public bool updateFreqTimer(float dt)
  {
    _sendTimer += dt;

    if (_sendTimer > _sendFrequency)
    {
      _sendTimer = 0f;
      return true;
    }

    return false;
  }

  public void resetFreqTimer() => _sendTimer = 0f;

  public void forceSend() => _sendTimer = _sendFrequency - 0.000001f; // next frame !

  public NwkMessage packMessage() => NwkSyncer.nwkSyncInject(this);
  public void unpackMessage(NwkMessage msg) => handle.unpack(msg.getMessage());

}
