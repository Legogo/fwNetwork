using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// abstract generic base class
/// to be able to query for server list
/// </summary>

abstract public class NwkIpFetcher : MonoBehaviour
{
  abstract public string getServerQueryUrl();

  [ReadOnly]
  public string[] server_ips;

  IEnumerator Start()
  {

    while (NwkSystemBase.nwkSys == null) yield return null;

    yield return null;

    fetchIps();
  }

  protected void fetchIps()
  {
    getServerIps(delegate(string[] ips)
    {
      this.server_ips = ips;

      Debug.Log("stored " + server_ips.Length + " after fetch");
      for (int i = 0; i < server_ips.Length; i++)
      {
        Debug.Log("  L " + server_ips[i]);
      }
    });
  }

  public void getServerIps(Action<string[]> ipsCallback)
  {
    FormUrlEncodedContent form = new FormUrlEncodedContent(new[] {
        new KeyValuePair<string, string>("act", "get")
      });

    query(form, delegate (string data)
    {
      //Debug.Log(data);

      string[] split = data.Split(Environment.NewLine.ToCharArray());
      ipsCallback(split);

    });

  }

  void query(FormUrlEncodedContent form, Action<string> onComplete = null)
  {
    HttpClient http = new HttpClient();

    http.PostAsync(getServerQueryUrl(), form).ContinueWith(delegate (Task<HttpResponseMessage> msg)
    {
      msg.Result.Content.ReadAsStringAsync().ContinueWith(delegate (Task<string> output)
      {
        //Debug.Log(output.Result);
        if (onComplete != null) onComplete(output.Result);
      });
    });

  }


}
