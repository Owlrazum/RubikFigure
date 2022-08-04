using System;

public static class PoolingDelegatesContainer
{
    public static Func<PoolableObjectPlaceHolder> FuncSpawn;
    public static Action<PoolableObjectPlaceHolder> EventDespawn;
}