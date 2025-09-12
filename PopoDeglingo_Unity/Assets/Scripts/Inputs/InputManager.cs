using UnityEngine;
using UnityUtility.Extensions;
using UnityUtility.Singletons;

public class InputManager : MonoBehaviourSingleton<InputManager>
{
    [SerializeField] private VirtualInput[] m_virtualInputs;

    public override void Initialize()
    {
        base.Initialize();
        m_virtualInputs.ForEach(input => input.Init());
    }

    private void Update()
    {
        m_virtualInputs.ForEach(input => input.Update());
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        m_virtualInputs.ForEach(input => input.Dispose());
    }

    public void SetButtonState(byte buttonCode, bool state)
    {
        m_virtualInputs[buttonCode].SetArduinoState(state);
    }
}
