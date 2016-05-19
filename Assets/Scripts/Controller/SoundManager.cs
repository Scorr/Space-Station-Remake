using UnityEngine;

namespace Controller {
    internal class SoundManager : Singleton<SoundManager> {

        private AudioSource _source;

        private SoundManager() {}

        private void Awake() {
            _source = gameObject.AddComponent<AudioSource>();
        }

        public void PlaySound(string path) {
            _source.PlayOneShot(Resources.Load<AudioClip>(path), 1.0f);
        }
    }
}
