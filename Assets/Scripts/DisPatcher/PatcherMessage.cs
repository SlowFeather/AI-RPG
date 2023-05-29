using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PatcherMessage
{
    /// <summary>
    /// 收到文字回调
    /// </summary>
    public const ushort GetGPTTxtMessage = 10001;

    /// <summary>
    /// 收到图片回调
    /// </summary>
    public const ushort GetGPTImgMessage = 10002;
}
