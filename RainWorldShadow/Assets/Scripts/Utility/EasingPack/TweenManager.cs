using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tweener
{
    public delegate void UpdateDelegate(float progress);
    public delegate void EndDelegate();
    public delegate void BeginDelegate();

    private bool m_IsActive = false;
    public bool IsActive { get { return m_IsActive; } private set { m_IsActive = value; } }

    private EasingFunction.Ease m_EaseType = EasingFunction.Ease.Linear;
    private float m_Speed = 1.0f;

    private UpdateDelegate m_UpdateCallback;
    private EndDelegate m_EndCallback;
    private BeginDelegate m_BeginCallback;
    private EndDelegate m_TerminateCallback;

    private float m_Progress = 0.0f;                     // 0 ~ 1 
    private float m_Delay;
 
    public static void SafeAbortTweener(ref Tweener tweener)
    {
        if (tweener != null)
        {
            tweener.Abort();
            tweener = null;
        }
    }

    public void InitForUse(UpdateDelegate updateCallback)
    {
        IsActive = true;
        m_Progress = 0.0f;

        m_Delay = -0.1f;
        m_EaseType = EasingFunction.Ease.Linear;
        m_Speed = 1;

        m_UpdateCallback = updateCallback;
        m_BeginCallback = null;
        m_EndCallback = null;
        m_TerminateCallback = null;
    }

    public void InitForUse(UpdateDelegate updateCallback, float delay, BeginDelegate beginCallback)
    {
        IsActive = true;
        m_Progress = 0.0f;

        m_Delay = delay;
        m_EaseType = EasingFunction.Ease.Linear;
        m_Speed = 1;

        m_UpdateCallback = updateCallback;
        m_BeginCallback = beginCallback;
        m_EndCallback = null;
        m_TerminateCallback = null;
    }

    public Tweener SetEase(EasingFunction.Ease easetype)
    {
        m_EaseType = easetype;
        return this;
    }

    public Tweener SetTime(float time)
    {
        m_Speed = 1 / time;
        return this;
    }

    public Tweener SetEndCallback(EndDelegate callback)
    {
        m_EndCallback = callback;
        return this;
    }

    public Tweener SetTerminateCallback(EndDelegate callback)
    {
        m_TerminateCallback = callback;
        return this;
    }

    public void Abort()
    {
        if (IsActive)
        {
            if (m_TerminateCallback != null)
            {
                m_TerminateCallback.Invoke();
            }
        }

        IsActive = false;
    }

    public void AbortWithoutFireEvent()
    {
        IsActive = false;
    }

    // return false if finished (inactive)
    public bool Update(float timeStep)
    {
        if (m_Delay > 0.0f)
        {
            m_Delay -= timeStep;
            if (m_Delay < 0.0f)
            {
                if (m_BeginCallback != null)
                    m_BeginCallback();
                if (m_UpdateCallback != null)
                    m_UpdateCallback(0);
            }
        }
        else
        {
            m_Progress += timeStep * m_Speed;

            float tvalue = m_Progress;
            if (m_Progress > 1.0f)
            {
                tvalue = 1.0f;
            }
            float xvalue = EasingFunction.GetEasingFunction(m_EaseType)(0, 1, tvalue);
            if (m_UpdateCallback != null)
                m_UpdateCallback(xvalue);
            
            if (m_Progress > 1.0f)
            {
                if (m_EndCallback != null)
                    m_EndCallback();

                if (m_TerminateCallback != null)
                    m_TerminateCallback();

                IsActive = false;
                m_UpdateCallback = null;
                m_EndCallback = null;
                m_BeginCallback = null;
                m_TerminateCallback = null;
            }
        }

        return IsActive;
    }
}

public class TweenManager 
{
    private static TweenManager s_Instance;
    public static TweenManager Instance 
    { 
        get 
        { 
            if (s_Instance == null)
            {
                s_Instance = new TweenManager();
            }
            return s_Instance; 
        } 
    }
    
    private List<Tweener> m_Tweeners;

    public TweenManager()
    {
        Init();
    }

    public void Init()
    {
		m_Tweeners = new List<Tweener>();
        s_Instance = this;
    }

    public void Cleanup()
    {
        for (int i = m_Tweeners.Count - 1; i >= 0; i--)
        {
            var twner = m_Tweeners[i];
            if (twner.IsActive)
            {
                twner.AbortWithoutFireEvent();
            }
        }
    }

    public void Update(float timeStep)
    {
		for (int i = m_Tweeners.Count - 1; i >= 0; i--)
        {
            var twner = m_Tweeners[i];
            if (twner.IsActive)
            {
                twner.Update(timeStep);
            }
        }
    }

    public Tweener Tween(Tweener.UpdateDelegate updateFunc)
    {
        Tweener ret = getInactiveTweener();

        ret.InitForUse(updateFunc);

        updateFunc(0.0f);

        return ret;
    }

    public Tweener Tween(Tweener.UpdateDelegate updateFunc, float delay, Tweener.BeginDelegate beginFunc = null)
    {
        Tweener ret = getInactiveTweener();
        ret.InitForUse(updateFunc, delay, beginFunc);

        return ret;
    }

    private Tweener getInactiveTweener()
    {
        Tweener ret = null;

        foreach (var twner in m_Tweeners)
        {
            if (!twner.IsActive)
            {
                ret = twner;
                break;
            }
        }

        if (ret == null)
        {
            m_Tweeners.Add(new Tweener());
            ret = m_Tweeners[m_Tweeners.Count - 1];
        }

        return ret;
    }
}
