using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ComboData", menuName = "Combat/ComboData")]
public class ComboData : ScriptableObject
{
    public List<AttackStep> comboSteps = new List<AttackStep>();
}
