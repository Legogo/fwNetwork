using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class RaiderNwkModules : MonoBehaviour
{
  protected RaiderNwkClient client;

  private void Start()
  {
    client = GameObject.FindObjectOfType<RaiderNwkClient>();
  }

  abstract public void onMessage(RaiderNwkMessage msg);

}
