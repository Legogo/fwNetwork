using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// everybody can create a new sync object
/// server will register and send to everybody else
/// everybody must be able to recreate that object in local sim
/// 
/// sert a stocker les temps de frequence d'envoi
/// </summary>

public class NwkSyncableData
{
  public int syncNwkClientUID; // nwkClient, owner ID
  public NwkSyncableId idCard; // all data needed to find the right object to unpack
  
  public iNwkPack handle; // handle to actual object that will pack/unpack

  //for syncer to work with
  float _sendTimer; // local timer
  float _sendFrequency; // time interval to send pack to server

  public NwkSyncableData(iNwkPack parent, float freq)
  {
    idCard = new NwkSyncableId();
    idCard.syncIID = NwkClient.generateUniqId();
    
    //this is overritten in syncer first message when it's not local
    syncNwkClientUID = NwkClient.nwkUid;

    handle = parent;
    _sendFrequency = freq;

    resetState();
    //GameObject.FindObjectOfType<NwkSyncer>().sub(parent);
  }

  void resetState()
  {
    _sendTimer = 0f;
  }

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
  public void unpackMessage(NwkMessageFull msg) => handle.unpack(msg.bytes.getObjectFromByteData());

  public NwkSyncableData overrideData(int nwkClientUID, int IID, int PID)
  {
    syncNwkClientUID = nwkClientUID;
    idCard.syncIID = IID;
    idCard.syncPID = PID;
    return this;
  }
}

[System.Serializable]
public struct NwkSyncableId
{
  public int syncIID; // uniq instance ID, uniq id to be able to find the right object
  public int syncPID; // name of prefab (from the database)
}