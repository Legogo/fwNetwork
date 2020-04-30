using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NwkTick : NwkModule
{
  public TickData data;

  protected override void setup()
  {
    base.setup();

    data.tickRateTimer = -1f;
    data.tickRate = 0.25f;
  }

  public void resetTickCount()
  {
    data.tick = 0;
    data.tickRateTimer = -1f;

    if (NwkSystemBase.isClient())
    {
      Debug.Log("client is connected, asking to server for tick data");

      NwkMessageBasic mb = new NwkMessageBasic();
      mb.getIdCard().setupId(NwkClient.nwkUid, (int)eNwkMessageType.TICK);
      NwkClient.nwkClient.sendWrapperClient.sendClientToServer(mb);
    }

    if(NwkSystemBase.isServer())
    {
      data.tickRateTimer = 0f; // starts counting
    }
  }

  protected override bool canUpdate()
  {
    return data.tickRateTimer >= 0f;
  }

  /// <summary>
  /// quand on recoit les infos du server
  /// </summary>
  public void setupTick(float servTickRate, int curServTick, float servTickTimer, float offsetTimeDelta = 0f)
  {
    data.tickRate = servTickRate;
    data.tick = curServTick;

    //le temps que le message arrive au server et revienne sur le client
    offsetTimeDelta *= 2f;

    data.tickRateTimer = servTickTimer + offsetTimeDelta;

    log("tickrate setup : " + data.tick + " / " + data.tickRate+" | <b>total offset</b> : "+ offsetTimeDelta);
  }

  protected override void updateNwk()
  {
    base.updateNwk();

    //lock rate progress if not sync
    //if (_tickRateTimer < 0f) return false;

    if (data.tickRateTimer < data.tickRate)
    {
      data.tickRateTimer += Time.deltaTime;

      if (data.tickRateTimer > data.tickRate)
      {
        data.tickRateTimer -= data.tickRate; // keep remaining dt for precision
        data.tick++;
      }
    }

  }

  public int getTickCount() => data.tick;

}


[System.Serializable]
public struct TickData
{
  public int tick;
  public float tickRateTimer; // locked until set
  public float tickRate; // 0.25 = 4 tick / secondes
}
