using System;
using System.Collections;
using System.Collections.Generic;


using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UniRx;

[RequireComponent(typeof(Button), typeof(Image))]
public class AbilityComponent : MonoBehaviour
{
    [SerializeField] protected string id;
    [SerializeField][TextArea] protected string description;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private long price;
    [SerializeField] private bool isLearned = false;

    [SerializeField] private AbilityComponent[] linkedComponents;

    protected Ability<string> ability = null;

    public Image Image { get; private set; }

    public IEnumerable<AbilityComponent> LinkedComponents => linkedComponents;

    public virtual Ability<string> AbilityValue
    {
        get
        {
            if (ability == null)
            {
                ability = new Ability<string>(id, GetTitle(), description, price, isLearned);
            }

            return ability;
        }
    }

    public Ability<string>[] GetLinkedAbilities()
    {
        Ability<string>[] links = new Ability<string>[linkedComponents.Length];

        for (int i = 0; i < links.Length; i++)
        {
            links[i] = linkedComponents[i].AbilityValue;
        }

        return links;
    }

    public void SubscribeClick(Action onAbilitySelected)
    {
        GetComponent<Button>().onClick.AsObservable()
            .Subscribe(_ => onAbilitySelected())
            .AddTo(this);
    }

    protected string GetTitle()
    {
        return titleText.text;
    }

    private void Awake()
    {
        Image = GetComponent<Image>();
    }
}