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

                source.Stop();
                source.clip = null;
                source.loop = false;
                source.volume = 1f;
                source.pitch = 1f;
            }

            _activeStorySfxDic.Clear();
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

            // 이번 스토리에 없는 기존 SFX만 정지
            StopUnusedStorySFX(nextSfxSet);

            for (int i = 0; i < soundID.Count; i++)
            {
                string id = soundID[i];

                if (string.IsNullOrEmpty(id))
                    continue;

                int playCount = 1;

                if (count != null && i < count.Count)
                    playCount = count[i];

                // 이미 같은 SFX가 재생 중이면 유지
                if (_activeStorySfxDic.ContainsKey(id))
                {
                    AudioSource activeSource = _activeStorySfxDic[id];

                    if (activeSource != null && activeSource.isPlaying)
                    {
                        continue;
                    }
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
                    source.Stop();
                    source.clip = null;
                    source.loop = false;
                    source.volume = 1f;
                    source.pitch = 1f;
                }

                removeList.Add(id);
            }

            for (int i = 0; i < removeList.Count; i++)
            {
                _activeStorySfxDic.Remove(removeList[i]);
            }
        }

        private void PlayLoopSFX(AudioSource source, SoundData data)
        {
            if (source == null || data == null || data.clip == null)
                return;

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
                source.Stop();
                source.clip = null;
                source.loop = false;
                source.volume = 1f;
                source.pitch = 1f;
            }

            if (!string.IsNullOrEmpty(id))
            {
                _activeStorySfxDic.Remove(id);
                _storySfxCoroutineDic.Remove(id);
            }
        }
        #endregion

        #region Volume Control

        public void SetVolume(SoundCategory category, float volume)
        {
            volume = Mathf.Clamp01(volume);

            string param = category switch
            {
                SoundCategory.Master => MasterVolParam,
                SoundCategory.BGM => BGMVolParam,
                SoundCategory.SFX => SFXVolParam,
                SoundCategory.UI => UIVolParam,
                SoundCategory.Opening => OpeningVolParam,
                _ => MasterVolParam
            };

            float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;

            if (audioMixer != null)
            {
                bool result = audioMixer.SetFloat(param, dB);

#if UNITY_EDITOR
                if (!result)
                {
                    Debug.LogError($"AudioMixer Exposed Parameter를 찾을 수 없습니다. Param: {param}");
                }
#endif
            }

            //PlayerPrefs.SetInt(param, Mathf.RoundToInt(volume * 100f));
        }

        public float GetVolume(SoundCategory category)
        {
            string param = category switch
            {
                SoundCategory.Master => MasterVolParam,
                SoundCategory.BGM => BGMVolParam,
                SoundCategory.SFX => SFXVolParam,
                SoundCategory.UI => UIVolParam,
                SoundCategory.Opening => OpeningVolParam,
                _ => MasterVolParam
            };

            return PlayerPrefs.GetFloat(param, 0.75f);
        }

        public void LoadVolumeSettings()
        {
            //SetVolume(SoundCategory.Master, PlayerPrefs.GetFloat(MasterVolParam, 0.75f));
            SetVolume(SoundCategory.BGM, Data_Manager.Instance.Sound_BG * 0.01f);
            SetVolume(SoundCategory.SFX, Data_Manager.Instance.Sound_Effect * 0.01f);
            SetVolume(SoundCategory.UI, Data_Manager.Instance.Sound_UI * 0.01f);
            //SetVolume(SoundCategory.Opening, PlayerPrefs.GetFloat(OpeningVolParam, 0.75f));
        }

        public void Mute(bool isMuted)
        {
            if (audioMixer == null)
                return;

            float volume = GetVolume(SoundCategory.Master);
            float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;

            audioMixer.SetFloat(MasterVolParam, isMuted ? -80f : dB);
        }

        #endregion
    }
}