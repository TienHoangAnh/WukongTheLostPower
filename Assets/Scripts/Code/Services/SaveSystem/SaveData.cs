using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public List<string> collectedIds = new();

    public Dictionary<string, int> collectedCounts = new();
}
