using System.Collections;
using UnityEngine;

public class ManagementBlockSound : MonoBehaviour
{
    public AudioSource audioSource;
    public void PlaySound(AudioClip clip, float initialPitch, bool randomPitch)
    {
        audioSource.clip = clip;
        audioSource.pitch = randomPitch ? Random.Range(0.5f, 1.5f) : Random.Range(initialPitch - 0.1f, initialPitch + 0.1f);
        audioSource.Play();
        StartCoroutine(DestroyBlock(clip));
    }
    IEnumerator DestroyBlock(AudioClip clip)
    {
        yield return new WaitForSecondsRealtime(clip.length);
        Destroy(gameObject);
    }
}
