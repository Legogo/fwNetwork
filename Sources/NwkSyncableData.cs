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
  public NwkSyncableId idCard;
  
  public INwkSyncable handle;

  //for syncer to work with
  float _sendTimer; // local timer
  float _sendFrequency; // time interval to send pack to server

  public NwkSyncableData(INwkSyncable parent, float freq)
  {
    idCard = new NwkSyncableId();
    idCard.syncIID = NwkClient.generateUniqId();
    
    //this is overritten in syncer first message when it's not local
    idCard.syncNwkClientUID = NwkClient.nwkUid;

    handle = parent;
    _sendFrequency = freq;

    resetState();
    //GameObject.FindObjectOfType<NwkSyncer>().sub(parent);
  }

  void resetState()
  {
    _sendTimer = 0f;
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

  public float getRemainingTime() => _sendFrequency - _sendTimer;

  public void resetFreqTimer() => _sendTimer = 0f;

  public void forceSend() => _sendTimer = _sendFrequency - 0.000001f; // next frame !

  public NwkMessageFull packMessage() => NwkSyncer.nwkSyncInject(this);
  public void unpackMessage(NwkMessageFull msg) => handle.unpack(msg.getMessage());

  public NwkSyncableData overrideData(int nwkClientUID, int IID, int PID)
  {
    idCard.syncNwkClientUID = nwkClientUID;
    idCard.syncIID = IID;
    idCard.syncPID = PID;
    return this;
  }
}

[System.Serializable]
public struct NwkSyncableId
{
  public int syncNwkClientUID; // client owner ID
  public int syncIID; // uniq instance ID
  public int syncPID; // name of prefab
}