using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INwkSyncable
{
  
  //bytes stuff
  object pack(); // generate an object with all data to sync

  /// <summary>
  /// explains how to apply data received from server
  /// unpack should not be HARD-link to syncer (bc objects are not sub to syncer when unpacking first time)
  /// </summary>
  void unpack(object obj);

  /// <summary>
  /// store and create on each object
  /// </summary>
  NwkSyncableData getData();

  //string getPrefabUID();
}

static public class INwkExtensions
{
  /// <summary>
  /// explains how to sub to syncer
  /// </summary>
  static public NwkSyncableData subSync(this INwkSyncable instance)
  {
    return GameObject.FindObjectOfType<NwkSyncer>().sub(instance);
  }

  /// <summary>
  /// meant to pass specific balancing data on object generation
  /// DO NOT MANAGE idcard
  /// </summary>
  static public NwkSyncableData createData(this INwkSyncable instance, float syncTime, short PID = -1)
  {
    NwkSyncableData data = new NwkSyncableData(instance, syncTime);
    data.idCard.syncPID = PID;
    return data;
  }

}
