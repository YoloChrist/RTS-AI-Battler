using UnityEngine;

public class SelectionHighlight : MonoBehaviour
{
    [SerializeField] private GameObject visual;

    private void Reset()
    {
        // Try to auto-assign a first child if not set
        if (visual == null && transform.childCount > 0)
            visual = transform.GetChild(0).gameObject;
    }

    public void SetVisible(bool visible)
    {
        if (visual != null)
            visual.SetActive(visible);
    }
}