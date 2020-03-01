using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// unused for now
/// </summary>

abstract public class NwkModules : MonoBehaviour
{
  protected NwkClient client;

  private void Start()
  {
    client = GameObject.FindObjectOfType<NwkClient>();
  }

  abstract public void onMessage(NwkMessage msg);

}
