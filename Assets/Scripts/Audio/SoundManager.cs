using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace God.Audio
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup bgmGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup uiGroup;
        [SerializeField] private AudioMixerGroup openingGroup;

        [Header("Settings")]
        [SerializeField] private int sfxPoolSize = 10;
        [SerializeField] private int uiPoolSize = 5;
        [SerializeField] private bool loadSettingsOnAwake = true;
        [SerializeField] private float sfxFadeOutDuration = 0.35f;

        [Header("BGM Channels")]
        [SerializeField] private AudioSource bgmSourceA;
        [SerializeField] private AudioSource bgmSourceB;

        [Header("Opening Channel")]
        [SerializeField] private AudioSource openingSource;

        [Header("Library")]
        [SerializeField] private SoundLibrary library;

        private readonly List<AudioSource> _sfxPool = new();
        private readonly List<AudioSource> _uiPool = new();

        private bool _isSourceAActive = true;
        private Coroutine _bgmFadeCoroutine;
        private Coroutine _openingFadeCoroutine;

        private const string MasterVolParam = "MasterVol";
        private const string BGMVolParam = "Sound_BG";
        private const string SFXVolParam = "Sound_Effect";
        private const string UIVolParam = "Sound_UI";
        private const string OpeningVolParam = "OpeningVol";

        private readonly Dictionary<SoundCategory, bool> _muteDic = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            InitializePools();

            if (loadSettingsOnAwake)
                LoadVolumeSettings();
        }

        #region Initialization

        private void InitializeAudioSources()
        {
            if (bgmSourceA == null)
                bgmSourceA = CreateAudioSource(gameObject, "BGM_Source_A", bgmGroup);

            if (bgmSourceB == null)
                bgmSourceB = CreateAudioSource(gameObject, "BGM_Source_B", bgmGroup);

            if (openingSource == null)
                openingSource = CreateAudioSource(gameObject, "Opening_Source", openingGroup);

            SetupAudioSource(bgmSourceA, bgmGroup);
            SetupAudioSource(bgmSourceB, bgmGroup);
            SetupAudioSource(openingSource, openingGroup);
        }

        private void InitializePools()
        {
            GameObject sfxRoot = new GameObject("SFXPool");
            sfxRoot.transform.SetParent(transform);

            for (int i = 0; i < sfxPoolSize; i++)
            {
                _sfxPool.Add(CreateAudioSource(sfxRoot, $"SFX_Source_{i}", sfxGroup));
            }

            GameObject uiRoot = new GameObject("UIPool");
            uiRoot.transform.SetParent(transform);

            for (int i = 0; i < uiPoolSize; i++)
            {
                _uiPool.Add(CreateAudioSource(uiRoot, $"UI_Source_{i}", uiGroup));
            }
        }

        private AudioSource CreateAudioSource(GameObject parent, string sourceName, AudioMixerGroup group)
        {
            GameObject obj = new GameObject(sourceName);
            obj.transform.SetParent(parent.transform);

            AudioSource source = obj.AddComponent<AudioSource>();
            SetupAudioSource(source, group);

            return source;
        }

        private void SetupAudioSource(AudioSource source, AudioMixerGroup group)
        {
            if (source == null)
                return;

            source.outputAudioMixerGroup = group;
            source.playOnAwake = false;
            source.loop = false;
            source.volume = 1f;
            source.pitch = 1f;
            source.spatialBlend = 0f;
        }

        #endregion

        #region BGM Management

        public void PlayBGM(string soundID, float fadeDuration = 1f, bool loop = true)
        {
            if (library == null)
            {
                Debug.LogError("SoundLibrary is not assigned.");
                return;
            }

            SoundData data = library.GetSound(soundID);
            PlayBGM(data, fadeDuration, loop);
        }

        public void PlayBGM(SoundData data, float fadeDuration = 1f, bool loop = true)
        {
            if (data == null || data.clip == null)
                return;

            PlayBGM(data.clip, fadeDuration, loop);
        }

        public void PlayBGM(AudioClip clip, float fadeDuration = 1f, bool loop = true)
        {
            if (clip == null)
                return;

            AudioSource activeSource = _isSourceAActive ? bgmSourceA : bgmSourceB;

            if (activeSource != null && activeSource.clip == clip && activeSource.isPlaying)
                return;

            if (_bgmFadeCoroutine != null)
                StopCoroutine(_bgmFadeCoroutine);

            _bgmFadeCoroutine = StartCoroutine(CrossfadeBGM(clip, fadeDuration, loop));
        }

        public void StopBGM(float fadeDuration = 1f)
        {
            if (_bgmFadeCoroutine != null)
                StopCoroutine(_bgmFadeCoroutine);

            _bgmFadeCoroutine = StartCoroutine(FadeOutActiveBGM(fadeDuration));
        }

        private IEnumerator CrossfadeBGM(AudioClip newClip, float duration, bool loop)
        {
            AudioSource oldSource = _isSourceAActive ? bgmSourceA : bgmSourceB;
            AudioSource newSource = _isSourceAActive ? bgmSourceB : bgmSourceA;

            if (oldSource == null || newSource == null)
                yield break;

            _isSourceAActive = !_isSourceAActive;

            newSource.clip = newClip;
            newSource.loop = loop;
            newSource.volume = 0f;
            newSource.Play();

            float elapsed = 0f;
            float startOldVolume = oldSource.volume;

            if (duration <= 0f)
            {
                newSource.volume = 1f;
                oldSource.volume = 0f;
                oldSource.Stop();
                yield break;
            }

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                newSource.volume = Mathf.Lerp(0f, 1f, t);
                oldSource.volume = Mathf.Lerp(startOldVolume, 0f, t);

                yield return null;
            }

            newSource.volume = 1f;
            oldSource.volume = 0f;
            oldSource.Stop();
        }

        private IEnumerator FadeOutActiveBGM(float duration)
        {
            AudioSource activeSource = _isSourceAActive ? bgmSourceA : bgmSourceB;

            if (activeSource == null)
                yield break;

            float startVolume = activeSource.volume;

            if (duration <= 0f)
            {
                activeSource.volume = 0f;
                activeSource.Stop();
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                activeSource.volume = Mathf.Lerp(startVolume, 0f, t);

                yield return null;
            }

            activeSource.volume = 0f;
            activeSource.Stop();
        }

        #endregion

        #region Opening Sound Management

        public void PlayOpening(string soundID, float fadeDuration = 1f, bool loop = true)
        {
            if (library == null)
            {
                Debug.LogError("SoundLibrary is not assigned.");
                return;
            }

            SoundData data = library.GetSound(soundID);
            PlayOpening(data, fadeDuration, loop);
        }

        public void PlayOpening(SoundData data, float fadeDuration = 1f, bool loop = true)
        {
            if (data == null || data.clip == null)
            {
                Debug.LogError("Opening SoundData is null or clip is missing.");
                return;
            }

            if (openingSource == null)
            {
                Debug.LogError("Opening AudioSource is not assigned.");
                return;
            }

            if (_openingFadeCoroutine != null)
                StopCoroutine(_openingFadeCoroutine);

            _openingFadeCoroutine = StartCoroutine(PlayOpeningRoutine(data, fadeDuration, loop));
        }

        public void StopOpening(float fadeDuration = 1f)
        {
            if (openingSource == null)
                return;

            if (_openingFadeCoroutine != null)
                StopCoroutine(_openingFadeCoroutine);

            _openingFadeCoroutine = StartCoroutine(StopOpeningRoutine(fadeDuration));
        }

        private IEnumerator PlayOpeningRoutine(SoundData data, float fadeDuration, bool loop)
        {
            openingSource.Stop();

            openingSource.clip = data.clip;
            openingSource.loop = loop;
            openingSource.pitch = data.GetRandomPitch();
            openingSource.spatialBlend = 0f;
            openingSource.volume = 0f;
            openingSource.Play();

            float targetVolume = data.GetRandomVolume();

            if (fadeDuration <= 0f)
            {
                openingSource.volume = targetVolume;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);

                openingSource.volume = Mathf.Lerp(0f, targetVolume, t);

                yield return null;
            }

            openingSource.volume = targetVolume;
        }

        private IEnumerator StopOpeningRoutine(float fadeDuration)
        {
            float startVolume = openingSource.volume;

            if (fadeDuration <= 0f)
            {
                openingSource.volume = 0f;
                openingSource.Stop();
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);

                openingSource.volume = Mathf.Lerp(startVolume, 0f, t);

                yield return null;
            }

            openingSource.volume = 0f;
            openingSource.Stop();
        }

        #endregion

        #region SFX & UI Management

        private readonly Dictionary<string, AudioSource> _activeStorySfxDic = new();
        private readonly Dictionary<string, Coroutine> _storySfxCoroutineDic = new();
        private readonly Dictionary<string, int> _activeStorySfxCountDic = new();
        private readonly Dictionary<AudioSource, Coroutine> _sfxFadeCoroutineDic = new();

        public void PlaySFX(string soundID, Vector3? position = null)
        {
            if (library == null)
            {
                Debug.LogError("SoundLibrary is not assigned.");
                return;
            }

            PlaySFX(library.GetSound(soundID), position);
        }

        public void PlayUI(string soundID)
        {
            if (library == null)
            {
                Debug.LogError("SoundLibrary is not assigned.");
                return;
            }

            PlayUI(library.GetSound(soundID));
        }

        public void PlaySFX(SoundData data, Vector3? position = null)
        {
            if (data == null || data.clip == null)
                return;

            AudioSource source = GetAvailableSource(_sfxPool);

            if (source == null)
            {
                Debug.LogWarning("No available SFX AudioSource.");
                return;
            }

            ConfigureAndPlay(source, data, position);
        }

        public void PlayUI(SoundData data)
        {
            if (data == null || data.clip == null)
            {
                Debug.LogError("SoundData is null or clip is missing.");
                return;
            }

            AudioSource source = GetAvailableSource(_uiPool);

            if (source == null)
            {
                Debug.LogWarning("No available UI AudioSource.");
                return;
            }

            ConfigureAndPlay(source, data, null);
        }

        private void ConfigureAndPlay(AudioSource source, SoundData data, Vector3? position)
        {
            if (source == null || data == null || data.clip == null)
                return;

            StopSFXFadeCoroutine(source);

            source.Stop();
            source.clip = data.clip;
            source.volume = data.GetRandomVolume();
            source.pitch = data.GetRandomPitch();
            source.spatialBlend = data.spatialBlend;
            source.loop = false;

            if (position.HasValue)
                source.transform.position = position.Value;

            source.Play();
        }

        private AudioSource GetAvailableSource(List<AudioSource> pool)
        {
            if (pool == null)
                return null;

            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && !pool[i].isPlaying)
                    return pool[i];
            }

            return null;
        }

        public void StopSFX()
        {
            foreach (var pair in _storySfxCoroutineDic)
            {
                if (pair.Value != null)
                    StopCoroutine(pair.Value);
            }

            _storySfxCoroutineDic.Clear();

            foreach (var pair in _activeStorySfxDic)
            {
                AudioSource source = pair.Value;

                if (source == null)
                    continue;

                FadeOutSFXSource(source, sfxFadeOutDuration);
            }

            _activeStorySfxDic.Clear();
            _activeStorySfxCountDic.Clear();
        }

        private void StopStorySFX(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            if (_storySfxCoroutineDic.ContainsKey(id))
            {
                if (_storySfxCoroutineDic[id] != null)
                    StopCoroutine(_storySfxCoroutineDic[id]);

                _storySfxCoroutineDic.Remove(id);
            }

            if (_activeStorySfxDic.ContainsKey(id))
            {
                AudioSource source = _activeStorySfxDic[id];

                if (source != null)
                {
                    FadeOutSFXSource(source, sfxFadeOutDuration);
                }

                _activeStorySfxDic.Remove(id);
            }

            _activeStorySfxCountDic.Remove(id);
        }

        public void PlaySFX(List<string> soundID, List<int> count)
        {
            if (library == null)
            {
                Debug.LogError("SoundLibrary is not assigned.");
                return;
            }

            if (soundID == null || soundID.Count <= 0)
            {
                StopSFX();
                return;
            }

            HashSet<string> nextSfxSet = new HashSet<string>();

            for (int i = 0; i < soundID.Count; i++)
            {
                if (!string.IsNullOrEmpty(soundID[i]))
                    nextSfxSet.Add(soundID[i]);
            }

            StopUnusedStorySFX(nextSfxSet);

            for (int i = 0; i < soundID.Count; i++)
            {
                string id = soundID[i];

                if (string.IsNullOrEmpty(id))
                    continue;

                int playCount = 1;

                if (count != null && i < count.Count)
                    playCount = count[i];

                if (_activeStorySfxDic.ContainsKey(id))
                {
                    AudioSource activeSource = _activeStorySfxDic[id];

                    int currentCount = 1;
                    if (_activeStorySfxCountDic.ContainsKey(id))
                        currentCount = _activeStorySfxCountDic[id];

                    if (activeSource != null && activeSource.isPlaying && currentCount == playCount)
                    {
                        continue;
                    }

                    StopStorySFX(id);
                }

                SoundData data = library.GetSound(id);

                if (data == null || data.clip == null)
                    continue;

                AudioSource source = GetAvailableSource(_sfxPool);

                if (source == null)
                {
                    Debug.LogWarning($"No available SFX AudioSource. SoundID: {id}");
                    continue;
                }

                _activeStorySfxDic[id] = source;
                _activeStorySfxCountDic[id] = playCount;

                if (playCount == -1)
                {
                    PlayLoopSFX(source, data);
                }
                else
                {
                    Coroutine coroutine = StartCoroutine(PlaySFXCountRoutine(id, source, data, playCount));
                    _storySfxCoroutineDic[id] = coroutine;
                }
            }
        }

        private void StopUnusedStorySFX(HashSet<string> nextSfxSet)
        {
            List<string> removeList = new List<string>();

            foreach (var pair in _activeStorySfxDic)
            {
                string id = pair.Key;

                if (nextSfxSet.Contains(id))
                    continue;

                AudioSource source = pair.Value;

                if (_storySfxCoroutineDic.ContainsKey(id))
                {
                    if (_storySfxCoroutineDic[id] != null)
                        StopCoroutine(_storySfxCoroutineDic[id]);

                    _storySfxCoroutineDic.Remove(id);
                }

                if (source != null)
                {
                    FadeOutSFXSource(source, sfxFadeOutDuration);
                }

                removeList.Add(id);
            }

            for (int i = 0; i < removeList.Count; i++)
            {
                _activeStorySfxDic.Remove(removeList[i]);
                _activeStorySfxCountDic.Remove(removeList[i]);
            }
        }

        private void PlayLoopSFX(AudioSource source, SoundData data)
        {
            if (source == null || data == null || data.clip == null)
                return;

            StopSFXFadeCoroutine(source);

            source.Stop();
            source.clip = data.clip;
            source.volume = data.GetRandomVolume();
            source.pitch = data.GetRandomPitch();
            source.spatialBlend = data.spatialBlend;
            source.loop = true;
            source.Play();
        }

        private IEnumerator PlaySFXCountRoutine(string id, AudioSource source, SoundData data, int count)
        {
            if (source == null || data == null || data.clip == null)
                yield break;

            if (count <= 0)
            {
                ReleaseStorySFXSource(id, source);
                yield break;
            }

            for (int i = 0; i < count; i++)
            {
                if (source == null)
                    yield break;

                StopSFXFadeCoroutine(source);

                source.Stop();
                source.clip = data.clip;
                source.volume = data.GetRandomVolume();
                source.pitch = data.GetRandomPitch();
                source.spatialBlend = data.spatialBlend;
                source.loop = false;
                source.Play();

                float pitch = Mathf.Abs(source.pitch);
                if (pitch <= 0.001f)
                    pitch = 1f;

                float waitTime = data.clip.length / pitch;
                float elapsed = 0f;

                while (elapsed < waitTime)
                {
                    if (source == null)
                        yield break;

                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            ReleaseStorySFXSource(id, source);
        }

        private void ReleaseStorySFXSource(string id, AudioSource source)
        {
            if (source != null)
            {
                ResetSFXSource(source);
            }

            if (!string.IsNullOrEmpty(id))
            {
                _activeStorySfxDic.Remove(id);
                _storySfxCoroutineDic.Remove(id);
                _activeStorySfxCountDic.Remove(id);
            }
        }

        private void FadeOutSFXSource(AudioSource source, float fadeDuration)
        {
            if (source == null)
                return;

            StopSFXFadeCoroutine(source);

            Coroutine coroutine = StartCoroutine(FadeOutSFXSourceRoutine(source, fadeDuration));
            _sfxFadeCoroutineDic.Add(source, coroutine);
        }

        private IEnumerator FadeOutSFXSourceRoutine(AudioSource source, float fadeDuration)
        {
            if (source == null)
                yield break;

            float startVolume = source.volume;

            if (fadeDuration <= 0f)
            {
                ResetSFXSource(source);
                _sfxFadeCoroutineDic.Remove(source);
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                if (source == null)
                    yield break;

                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / fadeDuration);
                source.volume = Mathf.Lerp(startVolume, 0f, t);

                yield return null;
            }

            ResetSFXSource(source);
            _sfxFadeCoroutineDic.Remove(source);
        }

        private void StopSFXFadeCoroutine(AudioSource source)
        {
            if (source == null)
                return;

            if (_sfxFadeCoroutineDic.ContainsKey(source))
            {
                if (_sfxFadeCoroutineDic[source] != null)
                    StopCoroutine(_sfxFadeCoroutineDic[source]);

                _sfxFadeCoroutineDic.Remove(source);
            }
        }

        private void ResetSFXSource(AudioSource source)
        {
            if (source == null)
                return;

            source.Stop();
            source.clip = null;
            source.loop = false;
            source.volume = 1f;
            source.pitch = 1f;
        }

        #endregion

        #region Volume Control

        public void SetVolume(SoundCategory category, float volume)
        {
            volume = Mathf.Clamp01(volume);

            string param = GetVolumeParam(category);
            bool isMuted = IsMuted(category);

            float dB = isMuted ? -80f : VolumeToDb(volume);

            SetMixerFloat(param, dB);
        }

        public void SetMute(SoundCategory category, bool isMuted)
        {
            if (_muteDic.ContainsKey(category))
                _muteDic[category] = isMuted;
            else
                _muteDic.Add(category, isMuted);

            ApplyCategoryVolume(category);
        }

        public void ToggleMute(SoundCategory category)
        {
            SetMute(category, !IsMuted(category));
        }

        public bool IsMuted(SoundCategory category)
        {
            return _muteDic.ContainsKey(category) && _muteDic[category];
        }

        public void SetMute_BGM(bool isMuted)
        {
            SetMute(SoundCategory.BGM, isMuted);
        }

        public void SetMute_SFX(bool isMuted)
        {
            SetMute(SoundCategory.SFX, isMuted);
        }

        public void SetMute_UI(bool isMuted)
        {
            SetMute(SoundCategory.UI, isMuted);
        }

        public void SetMute_Opening(bool isMuted)
        {
            SetMute(SoundCategory.Opening, isMuted);
        }

        public void SetMute_Master(bool isMuted)
        {
            SetMute(SoundCategory.Master, isMuted);
        }

        void ApplyCategoryVolume(SoundCategory category)
        {
            float volume = GetSavedVolume01(category);
            string param = GetVolumeParam(category);

            float dB = IsMuted(category) ? -80f : VolumeToDb(volume);

            SetMixerFloat(param, dB);
        }

        void SetMixerFloat(string param, float dB)
        {
            if (audioMixer == null)
                return;

            bool result = audioMixer.SetFloat(param, dB);

#if UNITY_EDITOR
            if (!result)
            {
                Debug.LogError($"AudioMixer Exposed Parameter를 찾을 수 없습니다. Param: {param}");
            }
#endif
        }

        string GetVolumeParam(SoundCategory category)
        {
            return category switch
            {
                SoundCategory.Master => MasterVolParam,
                SoundCategory.BGM => BGMVolParam,
                SoundCategory.SFX => SFXVolParam,
                SoundCategory.UI => UIVolParam,
                SoundCategory.Opening => OpeningVolParam,
                _ => MasterVolParam
            };
        }

        float GetSavedVolume01(SoundCategory category)
        {
            switch (category)
            {
                case SoundCategory.BGM:
                    return Data_Manager.Instance.Sound_BG * 0.01f;

                case SoundCategory.SFX:
                    return Data_Manager.Instance.Sound_Effect * 0.01f;

                case SoundCategory.UI:
                    return Data_Manager.Instance.Sound_UI * 0.01f;

                case SoundCategory.Master:
                    return PlayerPrefs.GetFloat(MasterVolParam, 1f);

                case SoundCategory.Opening:
                    return PlayerPrefs.GetFloat(OpeningVolParam, 1f);

                default:
                    return 1f;
            }
        }

        float VolumeToDb(float volume)
        {
            volume = Mathf.Clamp01(volume);
            return volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        }

        public float GetVolume(SoundCategory category)
        {
            return GetSavedVolume01(category);
        }

        public void LoadVolumeSettings()
        {
            ApplyCategoryVolume(SoundCategory.BGM);
            ApplyCategoryVolume(SoundCategory.SFX);
            ApplyCategoryVolume(SoundCategory.UI);
            ApplyCategoryVolume(SoundCategory.Master);
            ApplyCategoryVolume(SoundCategory.Opening);
        }

        public void Mute(bool isMuted)
        {
            SetMute(SoundCategory.Master, isMuted);
        }

        #endregion
    }
}