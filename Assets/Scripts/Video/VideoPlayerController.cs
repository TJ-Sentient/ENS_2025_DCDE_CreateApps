using UnityEngine;
using UnityEngine.Video;
using Sirenix.OdinInspector;
using System.IO;

public class VideoPlayerController : MonoBehaviour
{
    [Required]
    [SerializeField] private VideoPlayer videoPlayer;
    
    [ValueDropdown("GetVideoFiles")]
    [SerializeField] private string selectedVideo;

    private void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
        PlaySelectedVideo();
    }

    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    private void PlaySelectedVideo()
    {
        if (string.IsNullOrEmpty(selectedVideo))
        {
            Debug.LogError("No video selected!");
            return;
        }

        string videoPath = Path.Combine(Application.streamingAssetsPath, selectedVideo);
        
        if (!File.Exists(videoPath))
        {
            Debug.LogError($"Video not found: {videoPath}");
            return;
        }

        videoPlayer.url = videoPath;
        videoPlayer.Play();
    }

    [Button(ButtonSizes.Medium), GUIColor(1f, 0.5f, 0.3f)]
    private void StopVideo()
    {
        videoPlayer.Stop();
    }

    private ValueDropdownList<string> GetVideoFiles()
    {
        var list = new ValueDropdownList<string>();
        
        if (!Directory.Exists(Application.streamingAssetsPath))
            return list;

        string[] mp4Files = Directory.GetFiles(Application.streamingAssetsPath, "*.mp4", SearchOption.AllDirectories);
        string[] webmFiles = Directory.GetFiles(Application.streamingAssetsPath, "*.webm", SearchOption.AllDirectories);

        foreach (string file in mp4Files)
        {
            string relativePath = file.Replace(Application.streamingAssetsPath + Path.DirectorySeparatorChar, "");
            list.Add(relativePath, relativePath);
        }

        foreach (string file in webmFiles)
        {
            string relativePath = file.Replace(Application.streamingAssetsPath + Path.DirectorySeparatorChar, "");
            list.Add(relativePath, relativePath);
        }

        return list;
    }
}