public interface ICooldownSkill
{
 /// <summary>
 /// Restore the skill internal cooldown state given the remaining seconds (>=0).
 /// </summary>
 void RestoreCooldown(float remainingSeconds);
}