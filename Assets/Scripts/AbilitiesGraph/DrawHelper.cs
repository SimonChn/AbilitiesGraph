using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI.Extensions;

public class DrawHelper : MonoBehaviour
{
    [SerializeField] private Color unlearnedColor;
    [SerializeField] private Color learnedColor;

    [Space(20)]
    [SerializeField] private Transform linesParent;
    [SerializeField] private UILineRenderer linePrefab;

    public void DrawLines(List<AbilityComponent> components)
    {
        for(int i = 0; i < components.Count; i++)
        {
            var component = components[i];

            var neighbors = component.LinkedComponents;

            foreach (var neighbor in neighbors)
            {
                DrawLine(component, neighbor);
            }         
        }      
    }

    public void DrawAbilityComponent(AbilityComponent target, bool isLearned)
    {
        target.Image.color = (isLearned ? learnedColor : unlearnedColor);
    }

    private void DrawLine(AbilityComponent c1, AbilityComponent c2)
    {
        var line = Instantiate(linePrefab, linesParent);
        line.transform.SetAsFirstSibling();

        line.Points = new Vector2[] { c1.transform.localPosition, c2.transform.localPosition };
    }
}