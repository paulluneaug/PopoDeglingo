using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "VirtualInput", menuName = "Scriptables/VirtualInput")]
public class VirtualInput : ScriptableObject
{
    public event Action<bool> OnInputValueChanged;


    [SerializeField] private InputActionReference m_action;

    [SerializeField] private bool m_actionState;
    [SerializeField] private bool m_arduinoState;

    [NonSerialized] private Queue<bool> m_inputQueue;

    public void Init()
    {
        m_inputQueue = new Queue<bool>();

        m_actionState = false;
        m_arduinoState = false;
        m_action.action.started += OnActionPerformed;
        m_action.action.canceled += OnActionPerformed;

    }

    public void Update()
    {
        while (m_inputQueue.Count > 0)
        {
            bool input = m_inputQueue.Dequeue();
            Debug.Log($"Sending {input}");
            OnInputValueChanged?.Invoke(input);
        }   
    }

    public void Dispose()
    {
        m_action.action.started -= OnActionPerformed;
        m_action.action.canceled -= OnActionPerformed;
    }

    public void SetArduinoState(bool state)
    {
        OnAnyStateUpdated(m_actionState, state);
    }

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        OnAnyStateUpdated(context.action.IsPressed(), m_arduinoState);
    }


    private void OnAnyStateUpdated(bool actionState, bool arduinoState)
    {
        bool currentState = m_actionState || m_arduinoState;
        bool newState = actionState || arduinoState;

        if (currentState != newState)
        {
            m_inputQueue.Enqueue(newState);
        }

        m_actionState = actionState;
        m_arduinoState = arduinoState;
    }
}
