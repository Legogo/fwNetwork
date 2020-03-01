using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

static public class NwkTools
{

  static public Vector3Serializer packVec3(Vector3 v)
  {
    Vector3Serializer vs = new Vector3Serializer();
    vs.x = v.x;
    vs.y = v.y;
    vs.z = v.z;
    return vs;
  }

  static public Vector3 unpackVec3(Vector3Serializer v)
  {
    return new Vector3(v.x, v.y, v.z);
  }

  static public QuaternionSerializer packQuat(Quaternion q)
  {
    QuaternionSerializer tmp = new QuaternionSerializer();
    tmp.x = q.x;
    tmp.y = q.y;
    tmp.z = q.z;
    tmp.w = q.w;
    return tmp;
  }

  static public Quaternion unpackQuat(QuaternionSerializer q)
  {
    return new Quaternion(q.x, q.y, q.z, q.w);
  }

}

[Serializable]
public struct Vector3Serializer
{
  public float x;
  public float y;
  public float z;
}

[Serializable]
public struct QuaternionSerializer
{
  public float x;
  public float y;
  public float z;
  public float w;
}