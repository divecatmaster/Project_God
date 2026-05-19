using UnityEngine;
using UnityEngine.UI;

namespace LegacyTextEffects
{
    public class TextEffectDemo : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TypewriterEffect typewriter;
        [SerializeField] private ShakeEffect shake;
        [SerializeField] private ScaleBounceEffect bounce;
        [SerializeField] private FadeEffect fade;
        [SerializeField] private FloatingDamageText damageTextPrefab;
        [SerializeField] private Transform damageTextContainer;

        [Header("Test Settings")]
        [SerializeField] private string demoString = "Hello, this is a <color=red>RichText</color> typewriter effect!";

        public void RunTypewriter()
        {
            if (typewriter) typewriter.Play(demoString);
        }

        public void RunShake()
        {
            if (shake) shake.PlayShake();
        }

        public void RunBounce()
        {
            if (bounce) bounce.PlayBounce();
        }

        public void RunFadeOutIn()
        {
            if (fade)
            {
                fade.FadeOut(() => {
                    Debug.Log("Fade Out Complete");
                    fade.FadeIn(() => Debug.Log("Fade In Complete"));
                });
            }
        }

        public void SpawnDamageText()
        {
            if (damageTextPrefab && damageTextContainer)
            {
                // In a real project, use an Object Pool here
                FloatingDamageText instance = Instantiate(damageTextPrefab, damageTextContainer);
                instance.transform.localPosition = Vector3.zero;
                instance.Initialize("999!", Color.red, true);
            }
        }
    }
}
