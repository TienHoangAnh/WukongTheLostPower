using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ComboData", menuName = "Combat/ComboData")]
public class ComboData : ScriptableObject
{
    public List<AttackStep> comboSteps = new List<AttackStep>();
}
