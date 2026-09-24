using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public partial class GameController
{
    void ControlWintonAnim()
    {
        if (gameModeType != GameModeType.CharSound) return;
        if (wintonAnimator == null || audioSource == null || audioSource.clip == null) return;

        float animDuration = wintonAnimator.GetCurrentAnimatorStateInfo(0).length;
        float audioDuration = audioSource.clip.length;

        if (!audioSource.isPlaying)
        {
            wintonAnimator.speed = 0f;

            if (audioSource.time >= audioDuration - 0.02f || audioSource.time == 0f)
            {
                wintonAnimator.Play("WintonAnim", 0, 0f);
            }
            return;
        }

        if (animDuration <= 0f || audioDuration <= 0f)
        {
            wintonAnimator.speed = 1f;
            return;
        }

        wintonAnimator.speed = Mathf.Max(0.05f, animDuration / audioDuration);
    }

    void AudioControl()
    {
        if (videoPlayer != null && videoPlayer.isPlaying) videoPlayer.Stop();
        if (audioSource.clip == null || isDragging) return;
        
        if (!audioSource.isPlaying && audioSource.time >= audioSource.clip.length - 0.02f)
        {
            audioSource.time = 0f;
            slider.value = 0f;
            SetMediaTime(0, audioSource.clip.length);
            return;
        }
            
        slider.value = audioSource.time / audioSource.clip.length;
        SetMediaTime(audioSource.time, audioSource.clip.length);
    }

    void VideoControl()
    {
        if (!videoPlayer.isPrepared) return;

        if (!isDragging)
        {
            slider.value = (float)(videoPlayer.time / videoPlayer.length);
            SetMediaTime(videoPlayer.time, videoPlayer.length);
        }
    }

    void SetMediaTime(double t, double total)
    {
        int minutes = (int)(t / 60);
        int seconds = (int)(t % 60);

        int totalMinutes = (int)(total / 60);
        int totalSeconds = (int)(total % 60);

        audioTime.text = $"{minutes}:{seconds:00} / {totalMinutes}:{totalSeconds:00}";
    }

    public void StartDrag()
    {
        isDragging = true;
    }

    public void EndDrag()
    {
        isDragging = false;

        if (!canPlayVideo)
        {
            if (audioSource.clip == null) return;

            float t = slider.value * audioSource.clip.length;
            audioSource.time = Mathf.Clamp(t, 0, audioSource.clip.length - 0.01f);
            
            if (!audioSource.isPlaying && wintonAnimator != null)
            {
                wintonAnimator.Play("WintonAnim", 0, slider.value);
                wintonAnimator.Update(0f);
            }
        }
        else
        {
            if (!videoPlayer.isPrepared) return;
            videoPlayer.time = slider.value * videoPlayer.length;
        }
    }

    AudioClip GetAudioSource()
    {
        string cpResource = checkpointPaths[actualCheckpoint];
        string levelPath = GetFullDirectory();
        return Resources.Load<AudioClip>($"{levelPath}/{cpResource}/{cpResource}");
    }

    VideoClip GetVideoSource()
    {
        string levelPath = GetFullDirectory();
        return Resources.Load<VideoClip>($"{levelPath}/Video");
    }

    void LoadAudio()
    {
        if (audioSource == null) return;

        audioSource.Stop();
        audioSource.clip = GetAudioSource();

        if (audioSource.clip == null) return;

        audioSource.time = 0f;
        audioSource.Play();

        if (gameModeType == GameModeType.CharSound && wintonAnimator != null)
        {
            wintonAnimator.Play("WintonAnim", 0, 0f);
            wintonAnimator.speed = 1f;
            wintonAnimator.Update(0f);
        }
    }

    void LoadVideo()
    {
        LoadVideo(GetVideoSource());
    }

    void LoadVideo(VideoClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning($"Could not load video for {GetFullDirectory()}.");
            return;
        }

        videoPlayer.Stop();
        videoPlayer.clip = clip;
        StartVideoPlayer();
    }

    void StartVideoPlayer()
    {
        if (videoPlayer == null || videoPlayer.clip == null) return;

        if (videoPlayer.targetTexture == null)
        {
            var renderTexture = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            videoPlayer.targetTexture = renderTexture;
        }

        if (rawImage != null)
        {
            rawImage.texture = videoPlayer.targetTexture;
            rawImage.enabled = true;
        }

        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.Prepare();
    }

    void OnPrepared(VideoPlayer vp)
    {
        StartCoroutine(ShowPreparedVideoPreview(vp));
    }

    IEnumerator ShowPreparedVideoPreview(VideoPlayer vp)
    {
        vp.frame = 0;
        vp.StepForward();
        vp.Pause();

        while (vp.frame < 0 || vp.texture == null)
        {
            yield return null;
        }

        Texture previewTexture = vp.targetTexture != null ? vp.targetTexture : vp.texture;

        if (rawImage == null || previewTexture == null) yield break;

        rawImage.texture = previewTexture;
        rawImage.enabled = true;
        ResizeVideo(vp);
    }

    void RefreshVideoPreview()
    {
        if (!canPlayVideo || videoPlayer == null || rawImage == null) return;

        Texture previewTexture = videoPlayer.targetTexture != null ? videoPlayer.targetTexture : videoPlayer.texture;

        if (previewTexture != null)
        {
            rawImage.texture = previewTexture;
            rawImage.enabled = true;
            ResizeVideo(videoPlayer);
            return;
        }

        if (videoPlayer.clip != null) StartVideoPlayer();
    }

    void ResizeVideo(VideoPlayer vp)
    {
        if (rawImage == null) return;

        Texture previewTexture = vp.targetTexture != null ? vp.targetTexture : vp.texture;
        if (previewTexture == null) return;

        float videoWidth = previewTexture.width;
        float videoHeight = previewTexture.height;

        if (vp.clip != null && videoWidth <= 0f)
        {
            videoWidth = vp.clip.width;
            videoHeight = vp.clip.height;
        }

        if (videoWidth <= 0f || videoHeight <= 0f) return;

        float aspect = videoWidth / videoHeight;
        AspectRatioFitter f = rawImage.GetComponent<AspectRatioFitter>();
        if (f != null) f.aspectRatio = aspect;

        Canvas.ForceUpdateCanvases();
    }

    public void PlayMedia()
    {
        if (audioSource.clip != null && !audioSource.isPlaying && !canPlayVideo)
        {
            audioSource.Play();
        }

        if (!videoPlayer.isPlaying && audioSource.clip == null && canPlayVideo)
        {
            if (videoPlayer.isPrepared)
                videoPlayer.Play();
            else
                LoadVideo();
        }
    }

    public void PauseMedia()
    {
        if (audioSource.clip != null && audioSource.isPlaying && !canPlayVideo)
        {
            audioSource.Pause();
        }

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
    }
}