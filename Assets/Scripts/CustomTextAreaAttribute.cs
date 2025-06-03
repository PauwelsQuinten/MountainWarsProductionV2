using UnityEngine;

public class CustomTextAreaAttribute : PropertyAttribute
{
    public int MinLines;
    public int MaxLines;

    public CustomTextAreaAttribute(int minLines, int maxLines)
    {
        MinLines = minLines;
        MaxLines = maxLines;
    }
}
