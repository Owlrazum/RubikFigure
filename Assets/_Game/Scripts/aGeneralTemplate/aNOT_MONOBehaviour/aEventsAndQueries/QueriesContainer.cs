using System;

/// <summary>
/// Game specifics queries.
/// </summary>
public static class QueriesContainer
{
    public static Func<int> Custom;
    public static int QueryCustom()
    { 
#if UNITY_EDITOR
        if (Custom.GetInvocationList().Length != 1)
        {
            throw new NotSupportedException("There should be only one subscription");
        }
#endif        

        return Custom.Invoke();
    }
}
