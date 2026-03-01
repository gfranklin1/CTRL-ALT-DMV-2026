using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public GameObject soundFXPrefab;
    public Dictionary<string, AudioClip> sfxClips;
    public Dictionary<string, AudioClip> musicClips;
    public AudioSource musicSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void PlaySoundFXClip(string clipName, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundFXPrefab, spawnTransform.position, Quaternion.identity).GetComponent<AudioSource>();

        audioSource.clip = sfxClips[clipName];
        audioSource.volume = volume;
        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }
    public void PlayMusicClip(string clipName)
    {
        musicSource.clip = musicClips[clipName];
        musicSource.Play();
    }
}
