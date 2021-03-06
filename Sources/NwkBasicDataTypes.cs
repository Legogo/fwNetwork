﻿
[System.Serializable]
public struct NwkDataTransform : iNwkDataId
{
  public Vector3Serializer position;
  public QuaternionSerializer rotation;

  public int uid; // parent object uid
  public int getIID() => uid;

  public string toHuman()
  {
    return "IID:"+uid+"\nposition : " + position;
  }
}

public interface iNwkDataId
{

  /// <summary>
  /// uid of a NwkData object must match packer (parent object)
  /// </summary>
  int getIID();

  /// <summary>
  /// for debug readability
  /// </summary>
  string toHuman();
}
