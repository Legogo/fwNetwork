using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INwkSyncable
{
  
  //bytes stuff
  object pack(); // generate an object with all data to sync

  /// <summary>
  /// how to apply nwk object
  /// unpack should only be used in NwkSyncableData
  /// </summary>
  void unpack(object obj);

  //sync data (timer, id, ...)
  NwkSyncableData getData();

  //must sub to NwkSyncer
  void subSync();

}
