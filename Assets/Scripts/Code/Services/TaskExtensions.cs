using System.Collections;
using System.Threading.Tasks;

public static class TaskExtensions
{
    // Biến Task thành IEnumerator để yield trong Unity
    public static IEnumerator AsIEnumerator(this Task task)
    {
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
            throw task.Exception;
    }
}
