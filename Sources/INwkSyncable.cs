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

  /// <summary>
  /// this method explains how to sub to syncer
  /// and MUST be called by hand on object boot whenver the sync must start
  /// </summary>
  void subSync();

}
