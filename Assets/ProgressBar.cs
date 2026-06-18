using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] float value;
    [SerializeField] int offset = 80;
    [SerializeField] int barLength = 940;
    [SerializeField] int iconLength = 940;
    [SerializeField] int iconOffset = 80;
    [SerializeField] RectMask2D mask;
    [SerializeField] RectTransform posBar;
    

    [SerializeField] bool reverseBar = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateValue(float _value)
    {
        value = _value;
        if (mask != null)
        {
            float maskY = (value * barLength) + offset;
            mask.padding = new Vector4(0, maskY, 0, 0);
            if (reverseBar) mask.padding = new Vector4(0, 0, 0, maskY);
        }

        if (posBar != null)
        {
            float posY = (-iconLength / 2) + (value * iconLength) + iconOffset;
            if (reverseBar) posY = (iconLength/2) + (value * -iconLength) + iconOffset;
            posBar.localPosition = new Vector3(posBar.localPosition.x, posY, posBar.localPosition.z);
        }
    }
}
