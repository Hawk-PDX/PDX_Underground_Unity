namespace PDXUnderground.Core.Interfaces
{
    public interface IBaseCharacter
    {
        float GetCurrentHealth();
        float GetMaxHealth();
        bool IsDead();
    }
}

