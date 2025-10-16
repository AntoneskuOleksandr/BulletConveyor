using UnityEngine;

public enum TutorialActionType
{
    Click,
    Drag,
    None,
    Merge,
}

[System.Serializable]
public class TutorialStep
{
    public string instructionText;
    public GameObject highlightTarget;
    public TutorialActionType actionType;
    public System.Action OnStepStart;
    public System.Action OnStepComplete;
}
