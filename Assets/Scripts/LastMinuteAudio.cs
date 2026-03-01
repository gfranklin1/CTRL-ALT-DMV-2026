using UnityEngine;

public class LastMinuteAudio : MonoBehaviour
{
    public AudioClip oneAudio;
    public AudioClip twoAudio;
    public AudioSource source;
    public GameObject failPanel;
    void Start()
    {
        source.clip = Random.Range(0f, 1f) > .5f? oneAudio : twoAudio;
        source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (failPanel.activeSelf)
        {
            source.Stop();
        }
    }
}
