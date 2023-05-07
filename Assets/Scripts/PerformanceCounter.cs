using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;
using System.Text;
using UnityEngine.UI;

public class PerformanceCounter : MonoBehaviour
{
    private Text performanceText;
    ProfilerRecorder sysMemoryRecorder;
    ProfilerRecorder gcMemoryRecorder;
    ProfilerRecorder mainThreadTimeRecorder;

    double maxFrameTime;

    static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double r = 0;
        unsafe
        {
            var samples = stackalloc ProfilerRecorderSample[samplesCount];
            recorder.CopyTo(samples, samplesCount);
            for (var i = 0; i < samplesCount; ++i)
                r += samples[i].Value;
            r /= samplesCount;
        }

        return r;
    }

    void CheckForMaxFrameTime(double time)
    {
        if (time > maxFrameTime) { maxFrameTime = time; }
    }

    // Start is called before the first frame update
    void Start()
    {
        performanceText = GetComponent<Text>();
        sysMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);

        maxFrameTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        StringBuilder sb = new StringBuilder(1000);
        double frameTime = GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f);
        CheckForMaxFrameTime(frameTime);

        sb.AppendLine($"Framerate: {1000f / frameTime:F2} fps");
        sb.AppendLine($"Frame Time: {frameTime:F2} ms");
        sb.AppendLine($"GC Memory: {gcMemoryRecorder.LastValue / (1024 * 1024)} MB");
        sb.AppendLine($"System Memory: {sysMemoryRecorder.LastValue / (1024 * 1024)} MB");

        performanceText.text = sb.ToString();
    }
}
