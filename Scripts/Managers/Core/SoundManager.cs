using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager
{
    AudioMixer mixer;
    public AudioMixer Mixer
    {
        get
        {
            if(mixer == null)
            {
                mixer = Managers.Resource.Load<AudioMixer>("Prefabs/Audio/MasterAudioMixer");
            }

            return mixer;
        }
    }
    AudioSource[] _audioSources2D = new AudioSource[(int)Define.Sound2D.MaxCount];
    
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
    // MP3 Player -> AudioSource
    // MP3 À½¿ø -> AudioClip
    // °ü°´(±Í) -> AudioListener

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            Init2DSound(root);
        }
    }

    void Init2DSound(GameObject root)
    {
        string[] soundNames2D = System.Enum.GetNames(typeof(Define.Sound2D));
        for (int i = 0; i < soundNames2D.Length - 1; i++)
        {
            GameObject go = new GameObject { name = soundNames2D[i] };
            _audioSources2D[i] = go.AddComponent<AudioSource>();
            _audioSources2D[i].volume = 0.5f;
            go.transform.parent = root.transform;
        }
       
        if(Mixer != null)
        {
            _audioSources2D[(int)Define.Sound2D.Effect2D].outputAudioMixerGroup = Mixer.FindMatchingGroups("Effect")[0];
            _audioSources2D[(int)Define.Sound2D.Bgm].outputAudioMixerGroup = Mixer.FindMatchingGroups("Background")[0];
        }
        _audioSources2D[(int)Define.Sound2D.Bgm].loop = true;
    }

    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources2D)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }

        _audioClips.Clear();
    }

    public void StartBGM()
    {
        AudioSource audioSource = _audioSources2D[(int)Define.Sound2D.Bgm];
        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    public void PauseBGM()
    {
        AudioSource audioSource = _audioSources2D[(int)Define.Sound2D.Bgm];
        if (audioSource.isPlaying)
            audioSource.Pause();
    }

    public void Play2D(string path, Define.Sound2D type = Define.Sound2D.Effect2D, float pitch = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip2D(path, type);
        Play2D(audioClip, type, pitch);
    }

    public void Play2D(AudioClip audioClip, Define.Sound2D type = Define.Sound2D.Effect2D, float pitch = 1.0f)
    {
        if (audioClip == null)
            return;

        if (type == Define.Sound2D.Bgm)
        {
            AudioSource audioSource = _audioSources2D[(int)Define.Sound2D.Bgm];
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            AudioSource audioSource = _audioSources2D[(int)Define.Sound2D.Effect2D];
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }
    AudioClip GetOrAddAudioClip2D(string path, Define.Sound2D type = Define.Sound2D.Effect2D)
    {
        if (path.Contains("Sounds/") == false)
            path = $"Sounds/{path}";

        AudioClip audioClip = null;

        if (type == Define.Sound2D.Bgm)
        {
            audioClip = Managers.Resource.Load<AudioClip>(path);
        }
        else
        {
            if (_audioClips.TryGetValue(path, out audioClip) == false)
            {
                audioClip = Managers.Resource.Load<AudioClip>(path);
                _audioClips.Add(path, audioClip);
            }
        }

        if (audioClip == null)
            Debug.Log($"AudioClip Missing! {path}");

        return audioClip;
    }

    public void Play3D(string path, Vector3 pos, Transform parent, Define.Sound3D type = Define.Sound3D.Effect3D, float pitch = 1.0f, float minDistance = 1f, float maxDistance = 500f)
    {
        AudioClip audioClip = GetOrAddAudioClip3D(path, type);
        Play3D(audioClip, pos, parent, type, pitch, minDistance, maxDistance);
    }

    public void Play3D(string path, Vector3 pos, string parentName, Define.Sound3D type, float pitch, float minDistance, float maxDistance)
    {
        AudioClip audioClip = GetOrAddAudioClip3D(path, type);
        Play3D(audioClip, pos, parentName, type, pitch, minDistance, maxDistance);
    }

    public void Play3D(AudioClip audioClip, Vector3 pos, Transform parent, Define.Sound3D type = Define.Sound3D.Effect3D, float pitch = 1.0f, float minDistance = 1f, float maxDistance = 500f)
    {
        if (audioClip == null)
            return;

        AudioSource3D audioSource3D = Managers.Resource.Instantiate("Sound/Effect3D").GetComponent<AudioSource3D>();
        audioSource3D.audioSource.clip = audioClip;
        audioSource3D.audioSource.transform.position = pos;
        if (parent != null)
            audioSource3D.audioSource.transform.parent = parent;

        audioSource3D.audioSource.pitch = pitch;
        audioSource3D.audioSource.minDistance = minDistance;
        audioSource3D.audioSource.maxDistance = maxDistance;

        audioSource3D.Play();
    }

    public void Play3D(AudioClip audioClip, Vector3 pos, string parentName, Define.Sound3D type, float pitch, float minDistance, float maxDistance)
    {
        if (audioClip == null)
            return;

        AudioSource3D audioSource3D = Managers.Resource.Instantiate("Sound/Effect3D").GetComponent<AudioSource3D>();
        audioSource3D.audioSource.clip = audioClip;
        audioSource3D.audioSource.transform.position = pos;

        if (parentName != "")
        {
            Transform targetPos = GameObject.Find(parentName).transform;
            if (targetPos != null)
                audioSource3D.transform.parent = targetPos;
        }

        audioSource3D.audioSource.pitch = pitch;
        audioSource3D.audioSource.maxDistance = maxDistance;
        audioSource3D.audioSource.minDistance = minDistance;

        audioSource3D.Play();
    }

    AudioClip GetOrAddAudioClip3D(string path, Define.Sound3D type = Define.Sound3D.Effect3D)
    {
        if (path.Contains("Sounds/") == false)
            path = $"Sounds/{path}";

        AudioClip audioClip = null;

        if (_audioClips.TryGetValue(path, out audioClip) == false)
        {
            audioClip = Managers.Resource.Load<AudioClip>(path);
            _audioClips.Add(path, audioClip);
        }

        if (audioClip == null)
            Debug.Log($"AudioClip Missing! {path}");

        return audioClip;
    }
}
