using UnityEngine;
using UnityEngine.Video;

public class LoopVideo : MonoBehaviour
{
    void Start()
    {
        VideoPlayer vp = GetComponent<VideoPlayer>();
        vp.isLooping = true;
        vp.Play();
    }
}