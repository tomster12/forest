using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public Outline Outline => outline;

    public void Toggle(bool open)
    {
        outline.OutlineColor = open ? openHighlightColour : closedHighlightColour;
        onToggle.Invoke(open);
    }

    [Header("Config")]
    [SerializeField] private Outline outline;
    [SerializeField] private Color closedHighlightColour = Color.white;
    [SerializeField] private Color openHighlightColour = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private UnityEvent<bool> onToggle = new UnityEvent<bool>();
}
