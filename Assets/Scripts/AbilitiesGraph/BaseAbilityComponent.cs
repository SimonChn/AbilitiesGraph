public class BaseAbilityComponent : AbilityComponent
{
    public override Ability<string> AbilityValue
    {
        get
        {
            if (ability == null)
            {
                ability = new Ability<string>(id, GetTitle(), description, 
                    price: 0, isLearned: true);
            }

            return ability;
        }
    }
}