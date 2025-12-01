using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
public abstract class LootUseable 
{
    public abstract int TotalValue {get;}
    public abstract void GetLoot (int value);
    public abstract void UsingLoot (int value);
    public abstract void SaveLoot ();
    public abstract void LoadLoot (int value);
    public abstract void SaveListDecorationIds (List <int> listIds, DateTime dateTimeNow);
    public abstract List <int> LoadListDecorationIds ();
    public abstract DateTime LoadLastRefreshTime ();
    public abstract int GetCurrency ();
}
