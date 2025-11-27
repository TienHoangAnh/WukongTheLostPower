using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// PlayerUIManager updates player HUD: health/stamina bars and quick item slots (HP/MP items).
/// - Manages quick-slot counts directly (no global inventory).
/// - Call `PlayerUIManager.I.AddItem(id,count)` when picking up an item.
/// - Call `UseHpItem()` / `UseMpItem()` to consume an item and restore player stats.
///
/// NOTE: This component is intended to be scene-local and attached to a HUD Canvas
/// (e.g. `HUD_Canvas` in each scene). The Player GameObject is persistent via
/// DontDestroyOnLoad; the HUD should be recreated per scene and will rebind to the
/// persistent PlayerStats when the scene loads.
/// </summary>
public class PlayerUIManager : MonoBehaviour
{
 public static PlayerUIManager I { get; set; }

 public Slider healthSlider;
 public Slider staminaSider;
 private PlayerStats playerStats;
 private PlayerCombat playerCombat; // new reference to query cooldowns

 [Header("Item Slots")] 
 [Tooltip("Item id to show on the HP slot (e.g. 'holy_water')")]
 [SerializeField] private string hpItemId = "holy_water";
 [Tooltip("Image component to display HP item icon (child Icon). Do NOT assign the frame)" )]
 [SerializeField] private Image hpItemImage;
 [Tooltip("Text component to display HP item count (use UnityEngine.UI.Text)")]
 [SerializeField] private TMPro.TMP_Text hpItemCountText;

 [Tooltip("Item id to show on the MP slot (e.g. 'elixir')")]
 [SerializeField] private string mpItemId = "elixir";
 [Tooltip("Image component to display MP item icon (child Icon). Do NOT assign the frame)" )]
 [SerializeField] private Image mpItemImage;
 [Tooltip("Text component to display MP item count (use UnityEngine.UI.Text)")]
 [SerializeField] private TMPro.TMP_Text mpItemCountText;

 [Header("Skill Cooldown Texts (Q/E/R/J)")]
 [Tooltip("TMP text to display cooldown remaining for Q (melee skill1)")]
 [SerializeField] private TMP_Text qCooldownText;
 [Tooltip("TMP text to display cooldown remaining for E (melee skill2)")]
 [SerializeField] private TMP_Text eCooldownText;
 [Tooltip("TMP text to display cooldown remaining for R (melee skill3)")]
 [SerializeField] private TMP_Text rCooldownText;
 [Tooltip("TMP text to display cooldown remaining for J (ranged)")]
 [SerializeField] private TMP_Text jCooldownText;

 private int hpCount =0;
 private int mpCount =0;

 void Awake()
 {
 // Warn if mistakenly attached to the Player GameObject
 if (gameObject.CompareTag("Player"))
 {
 Debug.LogWarning("PlayerUIManager is attached to a GameObject with tag 'Player'.\n" +
 "This component should be attached to a scene-local HUD Canvas (e.g. HUD_Canvas).\n" +
 "Move the script to the HUD prefab so the UI is recreated per scene and can rebind to the persistent Player.");
 }

 if (I == null)
 {
 I = this;
 // NOTE: HUD is scene-local now. Do NOT call DontDestroyOnLoad so each scene can provide its own HUD prefab.

 // Ensure this canvas renders above scene UI by default
 var canvas = GetComponentInChildren<Canvas>();
 if (canvas != null)
 {
 // use a large sorting order so scene-local UI is on top
 canvas.sortingOrder = Mathf.Max(canvas.sortingOrder,100);
 }
 }
 else if (I != this)
 {
 // If another instance already exists in this scene, destroy this duplicate
 Destroy(gameObject);
 return;
 }
 }

 void OnEnable()
 {
 SceneManager.sceneLoaded += OnSceneLoaded;
 RebindPlayer(); // in case player already exists
 }

 void OnDisable()
 {
 SceneManager.sceneLoaded -= OnSceneLoaded;
 }

 private void OnSceneLoaded(Scene s, LoadSceneMode m)
 {
 // delay a frame if needed
 RebindPlayer();
 }

 private void RebindPlayer()
 {
 // Try common ways to find PlayerStats: singleton, FindObjectOfType, or by tag
 var ps = PlayerStats.Instance ?? FindFirstObjectByType<PlayerStats>();
 if (ps == null)
 {
 var playerGo = GameObject.FindWithTag("Player");
 if (playerGo != null)
 ps = playerGo.GetComponent<PlayerStats>() ?? playerGo.GetComponentInChildren<PlayerStats>();
 }

 if (ps != null)
 {
 playerStats = ps;

 // Try to bind PlayerCombat on same GameObject
 playerCombat = ps.GetComponent<PlayerCombat>() ?? ps.GetComponentInChildren<PlayerCombat>();

 // configure sliders' ranges
 if (healthSlider != null)
 {
 healthSlider.maxValue = ps.maxHealth;
 healthSlider.value = ps.currentHealth;
 }
 if (staminaSider != null)
 {
 staminaSider.maxValue = ps.maxStamina;
 staminaSider.value = ps.currentStamina;
 }

 // update UI immediately from ps
 UpdateHpUi(ps.currentHealth, ps.maxHealth);
 UpdateStaminaUi(ps.currentStamina, ps.maxStamina);

 Debug.Log($"[PlayerUI] Bound to PlayerStats (HP={ps.currentHealth}, Stamina={ps.currentStamina})");

 // optionally subscribe to change events if you add them to PlayerStats
 // ps.OnStatsChanged += OnPlayerStatsChanged;
 }
 else
 {
 Debug.LogWarning("[PlayerUI] PlayerStats not found when rebinding UI. Will retry shortly.");
 // retry a few times in case player is created after UI
 StartCoroutine(CoRetryBindPlayer(5,0.2f));
 }

 // refresh item icons from db in case UI persisted across scenes
 RefreshIconsFromDatabase();
 // update item counts from saved data if GameSaveController exists
 if (GameSaveController.I != null)
 {
 hpCount = GameSaveController.I.GetCollectedCount(hpItemId);
 mpCount = GameSaveController.I.GetCollectedCount(mpItemId);
 UpdateItemSlots();
 }
 }

 private System.Collections.IEnumerator CoRetryBindPlayer(int attempts, float delay)
 {
 for (int i =0; i < attempts; i++)
 {
 yield return new WaitForSeconds(delay);
 var ps = PlayerStats.Instance ?? FindFirstObjectByType<PlayerStats>();
 if (ps == null)
 {
 var playerGo = GameObject.FindWithTag("Player");
 if (playerGo != null)
 ps = playerGo.GetComponent<PlayerStats>() ?? playerGo.GetComponentInChildren<PlayerStats>();
 }

 if (ps != null)
 {
 playerStats = ps;
 playerCombat = ps.GetComponent<PlayerCombat>() ?? ps.GetComponentInChildren<PlayerCombat>();
 if (healthSlider != null) { healthSlider.maxValue = ps.maxHealth; healthSlider.value = ps.currentHealth; }
 if (staminaSider != null) { staminaSider.maxValue = ps.maxStamina; staminaSider.value = ps.currentStamina; }
 UpdateHpUi(ps.currentHealth, ps.maxHealth);
 UpdateStaminaUi(ps.currentStamina, ps.maxStamina);
 Debug.Log($"[PlayerUI] Successfully rebound to PlayerStats after retry (HP={ps.currentHealth}).");
 yield break;
 }
 }
 Debug.LogWarning("[PlayerUI] Failed to bind PlayerStats after retries.");
 }

 void OnDestroy()
 {
 if (I == this) I = null;
 }

 void Update()
 {
 // Update sliders if playerStats and sliders are present
 if (playerStats != null)
 {
 if (healthSlider != null) healthSlider.value = playerStats.currentHealth;
 if (staminaSider != null) staminaSider.value = playerStats.currentStamina;
 }

 // Always update skill cooldown displays (separate from slider bindings)
 UpdateSkillCooldowns();
 }

 // Read icon sprites from InventoryDatabase (if present)
 private void RefreshIconsFromDatabase()
 {
 if (InventoryDatabase.I != null)
 {
 var hpAsset = InventoryDatabase.I.GetById(hpItemId);
 if (hpAsset != null && hpAsset.icon != null && hpItemImage != null)
 {
 hpItemImage.sprite = hpAsset.icon;
 hpItemImage.SetNativeSize();
 }

 var mpAsset = InventoryDatabase.I.GetById(mpItemId);
 if (mpAsset != null && mpAsset.icon != null && mpItemImage != null)
 {
 mpItemImage.sprite = mpAsset.icon;
 mpItemImage.SetNativeSize();
 }
 }
 }

 /// <summary>
 /// Called by pickup logic when player picks up an item.
 /// Increments quick-slot count if the picked id matches hp/mp ids.
 /// </summary>
 public void AddItem(string id, int count =1)
 {
 if (string.IsNullOrEmpty(id) || count <=0) return;

 if (id == hpItemId)
 {
 hpCount += count;
 UpdateItemSlots();
 UI_Toasts.Show($"+{count} HP item");
 return;
 }

 if (id == mpItemId) 
 {
 mpCount += count;
 UpdateItemSlots();
 UI_Toasts.Show($"+{count} MP item");
 return;
 }

 // Unknown item id: ignore or log
 Debug.Log($"[PlayerUIManager] Picked unknown item id: {id}");
 }

 /// <summary>
 /// Use one HP item: heal player based on InventoryItem.hpRestore or default value.
 /// Returns true if used.
 /// </summary>
 public bool UseHpItem()
 {
 Debug.Log($"[PlayerUI] UseHpItem called. playerStats={(playerStats==null?"null":"set")}, hpCount={hpCount}, GameSave={(GameSaveController.I==null?"null":"present")} ");

 if (playerStats == null) return false;

 // Prefer using saved/global counts if available
 if (GameSaveController.I != null)
 {
 var available = GameSaveController.I.GetCollectedCount(hpItemId);
 Debug.Log($"[PlayerUI] GameSave available hp items = {available}");
 if (available <=0) return false;

 float heal =50f;
 if (InventoryDatabase.I != null)
 {
 var asset = InventoryDatabase.I.GetById(hpItemId);
 if (asset != null) heal = asset.hpRestore >0 ? asset.hpRestore : heal;
 }

 playerStats.Heal(heal);

 // Deduct from save and sync internal count from save
 GameSaveController.I.UseItem(hpItemId,1);
 hpCount = GameSaveController.I.GetCollectedCount(hpItemId);

 UpdateItemSlots();
 UI_Toasts.Show($"Used HP item. Healed {heal}");
 Debug.Log($"[PlayerUI] Used HP item via GameSave. New hpCount={hpCount}");
 return true;
 }

 // Fallback to local count logic
 if (hpCount <=0) { Debug.Log("[PlayerUI] No local hpCount to use."); return false; }

 float fallbackHeal =50f;
 if (InventoryDatabase.I != null)
 {
 var asset = InventoryDatabase.I.GetById(hpItemId);
 if (asset != null) fallbackHeal = asset.hpRestore >0 ? asset.hpRestore : fallbackHeal;
 }

 playerStats.Heal(fallbackHeal);
 hpCount = Mathf.Max(0, hpCount -1);
 UpdateItemSlots();
 UI_Toasts.Show($"Used HP item. Healed {fallbackHeal}");
 Debug.Log($"[PlayerUI] Used HP item locally. New hpCount={hpCount}");
 return true;
 }

 /// <summary>
 /// Use one MP (stamina) item: recover stamina based on InventoryItem.staminaRestore or default.
 /// </summary>
 public bool UseMpItem()
 {
 Debug.Log($"[PlayerUI] UseMpItem called. playerStats={(playerStats==null?"null":"set")}, mpCount={mpCount}, GameSave={(GameSaveController.I==null?"null":"present")} ");

 if (playerStats == null) return false;

 if (GameSaveController.I != null)
 {
 var available = GameSaveController.I.GetCollectedCount(mpItemId);
 Debug.Log($"[PlayerUI] GameSave available mp items = {available}");
 if (available <=0) return false;

 float recover =50f; 
 if (InventoryDatabase.I != null)
 {
 var asset = InventoryDatabase.I.GetById(mpItemId);
 if (asset != null) recover = asset.staminaRestore >0 ? asset.staminaRestore : recover;
 }

 playerStats.RecoverStamina(recover);

 GameSaveController.I.UseItem(mpItemId,1);
 mpCount = GameSaveController.I.GetCollectedCount(mpItemId);

 UpdateItemSlots();
 UI_Toasts.Show($"Used MP item. Recovered {recover}");
 Debug.Log($"[PlayerUI] Used MP item via GameSave. New mpCount={mpCount}");
 return true;
 }

 if (mpCount <=0) { Debug.Log("[PlayerUI] No local mpCount to use."); return false; }

 float fallbackRecover =50f;
 if (InventoryDatabase.I != null)
 {
 var asset = InventoryDatabase.I.GetById(mpItemId);
 if (asset != null) fallbackRecover = asset.staminaRestore >0 ? asset.staminaRestore : fallbackRecover;
 }

 playerStats.RecoverStamina(fallbackRecover);
 mpCount = Mathf.Max(0, mpCount -1);
 UpdateItemSlots();
 UI_Toasts.Show($"Used MP item. Recovered {fallbackRecover}");
 Debug.Log($"[PlayerUI] Used MP item locally. New mpCount={mpCount}");
 return true;
 }

 /// <summary>
 /// Wrapper methods for UI Buttons: these are void and will appear in the Button OnClick inspector.
 /// </summary>
 public void OnClick_UseHpItem()
 {
 Debug.Log("[PlayerUI] OnClick_UseHpItem invoked");
 UseHpItem();
 }

 public void OnClick_UseMpItem()
 {
 Debug.Log("[PlayerUI] OnClick_UseMpItem invoked");
 UseMpItem();
 }

 /// <summary>
 /// Update UI visuals for quick slots based on internal counts.
 /// Only toggles icon/count visibility (frame remains under UI designer control).
 /// </summary>
 private void UpdateItemSlots()
 {
 if (hpItemImage != null && hpItemCountText != null)
 {
 hpItemImage.gameObject.SetActive(hpCount >0);
 hpItemCountText.gameObject.SetActive(hpCount >0);
 hpItemCountText.text = hpCount >0 ? hpCount.ToString() : "";
 }

 if (mpItemImage != null && mpItemCountText != null)
 {
 mpItemImage.gameObject.SetActive(mpCount >0);
 mpItemCountText.gameObject.SetActive(mpCount >0);
 mpItemCountText.text = mpCount >0 ? mpCount.ToString() : "";
 }

 // Also update sliders immediately from playerStats if present
 if (playerStats != null)
 {
 if (healthSlider != null) healthSlider.value = playerStats.currentHealth;
 if (staminaSider != null) staminaSider.value = playerStats.currentStamina;
 }
 }

 // Optional getters
 public int GetHpCount() => hpCount;
 public int GetMpCount() => mpCount;

 // Helper methods to update HP/MP UI safely
 private void UpdateHpUi(float current, float max)
 {
 if (healthSlider != null)
 {
 healthSlider.maxValue = max;
 healthSlider.value = current;
 }
 }

 private void UpdateStaminaUi(float current, float max)
 {
 if (staminaSider != null)
 {
 staminaSider.maxValue = max;
 staminaSider.value = current;
 }
 }

 // Update skill cooldown text fields
 private void UpdateSkillCooldowns()
 {
 if (playerCombat == null) return;

 // Q (index0)
 if (qCooldownText != null)
 {
 float rem = playerCombat.GetComboCooldownRemaining(0);
 if (rem >0f)
 {
 qCooldownText.gameObject.SetActive(true);
 qCooldownText.text = rem.ToString("F1") + "s";
 }
 else
 {
 qCooldownText.gameObject.SetActive(false);
 }
 }

 // E (index1)
 if (eCooldownText != null)
 {
 float rem = playerCombat.GetComboCooldownRemaining(1);
 if (rem >0f)
 {
 eCooldownText.gameObject.SetActive(true);
 eCooldownText.text = rem.ToString("F1") + "s";
 }
 else
 {
 eCooldownText.gameObject.SetActive(false);
 }
 }

 // R (index2)
 if (rCooldownText != null)
 {
 float rem = playerCombat.GetComboCooldownRemaining(2);
 if (rem >0f)
 {
 rCooldownText.gameObject.SetActive(true);
 rCooldownText.text = rem.ToString("F1") + "s";
 }
 else
 {
 rCooldownText.gameObject.SetActive(false);
 }
 }

 // J (ranged)
 if (jCooldownText != null)
 {
 float rem = playerCombat.GetRangedCooldownRemaining();
 if (rem >0f)
 {
 jCooldownText.gameObject.SetActive(true);
 jCooldownText.text = rem.ToString("F1") + "s";
 }
 else
 {
 jCooldownText.gameObject.SetActive(false);
 }
 }
 }
}
