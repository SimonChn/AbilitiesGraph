using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UniRx;
using SimpleGraph;

public class SampleAbilitiesGraphScript : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button earnButton;
    [SerializeField] private Button forgetAllButon;

    [Space(20)]
    [SerializeField] private Button learnButton;
    [SerializeField] private Button forgetButton;

    [Header("Displays")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Space(20)]
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;

    [Space(20)]
    [SerializeField] private DrawHelper drawer;

    [Header("Script")]
    [SerializeField] private Transform abilitiesParent;
    [SerializeField] private Transform selectionFrame;

    public void Start()
    {
        PerformScript();
    }

    public void PerformScript()
    {
        var score = new ReactiveProperty<long>();
        var selectedAbility = new ReactiveProperty<Ability<string>>();

        BaseAbilityComponent baseAbilityComp;
        List<AbilityComponent> abilitiesComponents;

        Ability<string> baseAbility;
        Ability<string>[] abilities;
        TraversableNodesRootGraph<string> abilitiesGraph;

        SubscribeScoreText(score);
        SubscribeEarnButton(score);

        GetAbilitiesComponents(out baseAbilityComp, out abilitiesComponents);
        GetAbilitiesAndSetTheirView(baseAbilityComp, abilitiesComponents, out baseAbility, out abilities);
        abilitiesGraph = GetGraph(baseAbilityComp, abilitiesComponents, baseAbility, abilities);

        DrawLinesBetweenConnected(baseAbilityComp, abilitiesComponents);

        selectedAbility.Value = baseAbility;
        Highlight(baseAbilityComp.transform);

        SubscribeDescriptionText(selectedAbility);
        SubscribePriceText(selectedAbility);

        SubscribeLearnButton(selectedAbility, score, abilitiesGraph);
        SubscribeForgetButton(selectedAbility, score, abilitiesGraph, baseAbility);
        SubscribeForgetAllButton(selectedAbility, score, abilitiesGraph);

        SubscribeComponentsClicks(baseAbilityComp, abilitiesComponents,
            selectedAbility, baseAbility, abilities);
    }

    private void SubscribeScoreText(ReactiveProperty<long> score)
    {
        score.Subscribe(value => scoreText.text = $"{Constants.ScorePreText} {value}")
           .AddTo(scoreText);
    }

    private void SubscribeEarnButton(ReactiveProperty<long> score)
    {
        earnButton.onClick.AsObservable()
          .Subscribe(_ => score.Value++)
          .AddTo(earnButton);
    }

    private void GetAbilitiesComponents(out BaseAbilityComponent baseAbilityComp, out List<AbilityComponent> abilitiesComponents)
    {
        baseAbilityComp = null;
        abilitiesComponents = new List<AbilityComponent>(abilitiesParent.childCount - 1);

        for (int i = 0; i < abilitiesParent.childCount; i++)
        {
            var child = abilitiesParent.GetChild(i);

            if (child.TryGetComponent<AbilityComponent>(out var ability))
            {
                if (ability is BaseAbilityComponent root)
                {
                    if (!baseAbilityComp)
                    {
                        baseAbilityComp = root;
                    }
                    else
                    {
                        Debug.LogError("Multiple base abilities");
                    }
                }
                else
                {
                    abilitiesComponents.Add(ability);
                }
            }
        }

        if(!baseAbilityComp)
        {
            Debug.LogError("Null base");
        }
    }

    private void GetAbilitiesAndSetTheirView(in BaseAbilityComponent baseAbilityComp, List<AbilityComponent> abilitiesComponents,
        out Ability<string> baseAbility, out Ability<string>[] abilities)
    {
        baseAbility = baseAbilityComp.AbilityValue;
        abilities = new Ability<string>[abilitiesComponents.Count];

        HashSet<string> checkUnique = new HashSet<string>();
        for(int i = 0; i < abilities.Length; i++)
        {
            int index = i;

            var a = abilitiesComponents[index].AbilityValue;

            if(checkUnique.Contains(a.Id))
            {
                Debug.LogError($"Some abilities have the same id: {a.Id}");
                return;
            }
            else
            {
                checkUnique.Add(a.Id);
            }

            abilities[index] = a;
            abilities[index].IsLearned.Subscribe(isLearned =>
            {
                drawer.DrawAbilityComponent(abilitiesComponents[index], isLearned);
            }).AddTo(baseAbilityComp).AddTo(drawer);
        }
    }

    private TraversableNodesRootGraph<string> GetGraph(BaseAbilityComponent baseAbilityComp, List<AbilityComponent> abilitiesComponents,
        Ability<string> baseAbility, Ability<string>[] abilities)
    {
        var abilitiesGraph = new TraversableNodesRootGraph<string>(baseAbility);
        abilitiesGraph.AddVertices(abilities);

        abilitiesGraph.AddEdges(baseAbility, baseAbilityComp.GetLinkedAbilities());
        for (int i = 0; i < abilities.Length; i++)
        {
            abilitiesGraph.AddEdges(abilities[i], abilitiesComponents[i].GetLinkedAbilities());
        }

        return abilitiesGraph;
    }

    private void DrawLinesBetweenConnected(AbilityComponent baseAbilityComp, List<AbilityComponent> abilitiesComponents)
    {
        List<AbilityComponent> allAbilities = new List<AbilityComponent>(abilitiesComponents);
        allAbilities.Add(baseAbilityComp);
        drawer.DrawLines(allAbilities);
    }

    private void SubscribeDescriptionText(ReactiveProperty<Ability<string>> selectedAbility)
    {
        selectedAbility.Subscribe(ability => descriptionText.text = $"{ability.Description}")
            .AddTo(descriptionText);
    }

    private void SubscribePriceText(ReactiveProperty<Ability<string>> selectedAbility)
    {
        selectedAbility.Subscribe(ability =>
        {
            priceText.text = (!ability.IsLearned.Value ? $"Price: {ability.Price}" : string.Empty);
        }).AddTo(priceText);
    }

    private void SubscribeLearnButton(ReactiveProperty<Ability<string>> selectedAbility, ReactiveProperty<long> score,
        TraversableNodesRootGraph<string> abilitiesGraph)
    {
        selectedAbility.CombineLatest(score, (ability, score) =>
        {
            return (!ability.IsLearned.Value
                && ability.Price <= score
                && abilitiesGraph.HasPath(abilitiesGraph.Root, ability));
        }).ToReactiveProperty()
            .Subscribe(canLearn => learnButton.interactable = canLearn)
            .AddTo(learnButton);

        learnButton.onClick.AsObservable()
            .Subscribe(_ =>
            {
                var a = selectedAbility.Value;

                score.Value -= a.Price;

                a.IsLearned.Value = true;
                selectedAbility.SetValueAndForceNotify(a);
            }).AddTo(learnButton);
    }

    private void SubscribeForgetButton(ReactiveProperty<Ability<string>> selectedAbility, ReactiveProperty<long> score,
        TraversableNodesRootGraph<string> abilitiesGraph, Ability<string> baseAbility)
    {
        selectedAbility.Subscribe(ability =>
        {
            bool canForget = ability.IsLearned.Value
                                && ability.Id != baseAbility.Id
                                && abilitiesGraph.CanMakeUntraversable(ability);

            forgetButton.interactable = canForget;
        }).AddTo(forgetButton);

        forgetButton.onClick.AsObservable()
            .Subscribe(_ =>
            {
                var a = selectedAbility.Value;

                score.Value += a.Price;

                a.IsLearned.Value = false;
                selectedAbility.SetValueAndForceNotify(a);
            }).AddTo(forgetButton);
    }

    private void SubscribeForgetAllButton(ReactiveProperty<Ability<string>> selectedAbility, ReactiveProperty<long> score, 
        TraversableNodesRootGraph<string> abilitiesGraph)
    {
        forgetAllButon.onClick.AsObservable()
            .Subscribe(_ =>
            {
                var nodes = abilitiesGraph.GetAllTraversableNodes();

                var learnedAbilities = nodes.ConvertAll(x => (Ability<string>)x);

                long resPrice = 0;

                foreach (var ability in learnedAbilities)
                {
                    ability.IsLearned.Value = false;
                    resPrice += ability.Price;
                }

                selectedAbility.SetValueAndForceNotify(selectedAbility.Value);
                score.Value += resPrice;
            }).AddTo(forgetAllButon);
    }

    private void SubscribeComponentsClicks(BaseAbilityComponent baseAbilityComp, List<AbilityComponent> abilitiesComponents,
        ReactiveProperty<Ability<string>> selectedAbility, Ability<string> baseAbility, Ability<string>[] abilities)
    {
        baseAbilityComp.SubscribeClick(() =>
        {
            selectedAbility.Value = baseAbility;
            Highlight(baseAbilityComp.transform);
        });

        for (int i = 0; i < abilitiesComponents.Count; i++)
        {
            int index = i;
            abilitiesComponents[i].SubscribeClick(() =>
            {
                selectedAbility.Value = abilities[index];
                Highlight(abilitiesComponents[index].transform);
            });
        }
    }

    private void Highlight(Transform element)
    {
        selectionFrame.SetParent(element);
        selectionFrame.localPosition = Vector3.zero;
    }

    private static class Constants
    {
        public const string ScorePreText = "Score:";
    }
}