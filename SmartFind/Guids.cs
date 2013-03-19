// Guids.cs
// MUST match guids.h
using System;

namespace ChristianZangl.SmartFind
{
  static class GuidList
  {
    public const string guidSmartFindPkgString = "3b28ec01-ab7f-427a-81d3-afc41415ee5e";
    public const string guidSmartFindCmdSetString = "56eca5cf-209d-45ac-a3d4-09ea3df7a7c4";

    public static readonly Guid guidSmartFindCmdSet = new Guid(guidSmartFindCmdSetString);
  };
}