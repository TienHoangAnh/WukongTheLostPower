using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SaveDebugHelper : MonoBehaviour
{
     [SerializeField] private string slotId = "slotA";

     void Update()
     {
         if (Input.GetKeyDown(KeyCode.F5))
         {
         _ = SaveAndDump();
         }
     }

     private async Task SaveAndDump()
     {
         Debug.Log("[SaveDebugHelper] Triggering SaveService.SaveGame...");
         await SaveService.SaveGame(slotId);
         await Task.Delay(200); // small delay to ensure IO flushed

         var path = Application.persistentDataPath + $"/save_{slotId}.json";
         if (File.Exists(path))
         {
             var content = File.ReadAllText(path);
             Debug.Log($"[SaveDebugHelper] Save file content ({path}):\n{content}");
         }
         else
         {
             Debug.LogWarning($"[SaveDebugHelper] Save file not found at {path}");
         }
     }
}
