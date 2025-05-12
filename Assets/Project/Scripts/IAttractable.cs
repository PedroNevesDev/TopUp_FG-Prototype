using UnityEngine;

public interface IAttractable
{
    void AttractTo(Transform target);
    bool CanBeAttracted(); // Optional: e.g. for cooldowns or active state
}
