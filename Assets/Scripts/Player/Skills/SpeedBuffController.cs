using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class SpeedBuffController : MonoBehaviour
{
    private class Entry
    {
        public float speedMul;
        public float accelMul;
        public float endTime;
    }

    private readonly List<Entry> _entries = new();

    // Hệ số hiện tại (đã gộp)
    public float CurrentSpeedMul { get; private set; } = 1f;
    public float CurrentAccelMul { get; private set; } = 1f;

    public void Apply(float speedMul, float accelMul, float duration)
    {
        _entries.Add(new Entry
        {
            speedMul = Mathf.Max(0.01f, speedMul),
            accelMul = Mathf.Max(0.01f, accelMul),
            endTime = Time.time + Mathf.Max(0.01f, duration)
        });
        Recalc();
    }

    void Update()
    {
        // Xóa buff hết hạn
        bool changed = false;
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            if (Time.time >= _entries[i].endTime)
            {
                _entries.RemoveAt(i);
                changed = true;
            }
        }
        if (changed) Recalc();
    }

    private void Recalc()
    {
        // Lấy MAX để tránh nhân vô hạn (có thể đổi sang nhân nếu bạn muốn “stack” mạnh)
        float sp = 1f, ac = 1f;
        foreach (var e in _entries)
        {
            if (e.speedMul > sp) sp = e.speedMul;
            if (e.accelMul > ac) ac = e.accelMul;
        }
        CurrentSpeedMul = sp;
        CurrentAccelMul = ac;
    }
}
