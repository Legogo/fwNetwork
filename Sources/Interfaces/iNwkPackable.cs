using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// suitable for syncer
/// </summary>
public interface iNwkSync : iNwkPack
{

  /// <summary>
  /// store and create on each object
  /// </summary>
  NwkSyncableData getData();

}

/// <summary>
/// lier un packable a un objet
/// </summary>
public interface iNwkPack : iNwkPackable
{

  int getSyncIID();

}

/// <summary>
/// pack, unpack
/// </summary>
public interface iNwkPackable
{
  /// <summary>
  /// don't forget to spacify an IID (instance id) during packing 
  /// to be able to transfert the package to the right object
  /// </summary>
  object pack(); // generate an object with all data to sync

  /// <summary>
  /// explains how to apply data received from server
  /// unpack should not be HARD-link to syncer (bc objects are not sub to syncer when unpacking first time)
  /// </summary>
  void unpack(object obj);

  //string getPrefabUID();
}

static public class iNwkExtensions
{
  /// <summary>
  /// explains how to sub to syncer
  /// </summary>
  static public NwkSyncableData subSync(this iNwkSync instance)
  {
    return GameObject.FindObjectOfType<NwkSyncer>().sub(instance);
  }

  /// <summary>
  /// generic signature to be called in getData of syncables
  /// meant to pass specific balancing data on object generation
  /// DO NOT MANAGE idcard
  /// </summary>
  static public NwkSyncableData createData(this iNwkPack instance, float syncTime, short PID = -1)
  {
    NwkSyncableData data = new NwkSyncableData(instance, syncTime);
    data.idCard.syncPID = PID;
    return data;
  }

}
