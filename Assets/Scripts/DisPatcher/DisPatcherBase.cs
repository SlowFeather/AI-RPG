using System;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 消息派发机制
/// </summary>
public class DisPatcherBase<T,P,X> : IDisposable where T :new () where P:class
{

    #region Instance 单例

    private static T _instance;
    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new T();
                }
            }
            return _instance;
        }
    }

    #endregion

    #region delegate Dictionary 属性

    /// <summary>
    /// 委托原型
    /// </summary>
    /// <param name="p"></param>
    public delegate void OnActionHander(P p);

    /// <summary>
    /// 缓存
    /// </summary>
    protected Dictionary<X, List<OnActionHander>> dic = new Dictionary<X, List<OnActionHander>>();

    #endregion

    #region AddeventListener 创建监听

    /// <summary>
    /// 创建监听
    /// </summary>
    /// <param name="x"></param>
    /// <param name="hander"></param>
    public void AddeventListener(X x, OnActionHander hander)
    {
        if (dic.ContainsKey(x))
        {
            if (!dic[x].Contains(hander)) dic[x].Add(hander);
        }
        else
        {
            List<OnActionHander> lstHander = new List<OnActionHander>();
            lstHander.Add(hander);
            dic.Add(x,lstHander);
        }

    }

    #endregion

    #region RemoveEventListener 移除监听

    /// <summary>
    /// 移除监听
    /// </summary>
    public void RemoveEventListener(X x,OnActionHander hander)
    {
        if (dic.ContainsKey(x))
        {
            List<OnActionHander> lstHander = dic[x];
            if (lstHander == null) return;
            lstHander.Remove(hander);
            if (lstHander.Count == 0) dic.Remove(x);
        }
    }

    #endregion

    #region DisPath 派发

    /// <summary>
    /// 派发
    /// </summary>
    /// <param name="x"></param>
    /// <param name="p">buffer</param>
    public void DisPatch(X x, P p)
    {
        if (dic.ContainsKey(x))
        {
            List<OnActionHander> hander = dic[x];
            if (hander.Count > 0 && hander != null)
            {
                for (int i = 0; i < hander.Count; i++)
                {
                    hander[i]?.Invoke(p);
                }
            }
        }
    }

    /// <summary>
    /// 派发（无第二参数重载）
    /// </summary>
    public void DisPatch(X x)
    {
        DisPatch(x,null);
    }

    #endregion

    #region Dispose 释放

    /// <summary>
    /// 释放
    /// </summary>
    public virtual void Dispose()
    {

    }

    #endregion

}
