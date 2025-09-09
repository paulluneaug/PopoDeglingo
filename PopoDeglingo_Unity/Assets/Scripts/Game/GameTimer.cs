using System;
using UnityEngine;

/// <summary>
/// Represents a timer that calls an action at the end
/// </summary>
public class GameTimer
{
    private float m_timer;
    private bool m_running = false;
    private Action m_callback;
    public GameTimer(float time, Action callback)
    {
        Reset(time, callback);
    }

    /// <summary>
    /// Resets the timer
    /// </summary>
    /// <param name="time">The </param>
    /// <param name="callback"></param>
    public void Reset(float time, Action callback)
    {
        m_running = true;
        m_timer = time;
        m_callback = callback;
    }

    public void Update(float deltaTime)
    {
        if (m_running)
        {
            m_timer -= deltaTime;
            if (m_timer <= 0)
            {
                m_running = false;
                m_callback.Invoke();
            }
        }
    }
}
