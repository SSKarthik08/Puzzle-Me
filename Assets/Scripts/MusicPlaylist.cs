using System.Collections.Generic;
using UnityEngine;

// Auto-plays through a list of music tracks, one after another,
// looping back to the start forever — until this GameObject is
// destroyed (which happens automatically when its scene unloads,
// e.g. leaving Home for Gameplay, or leaving Gameplay for Home).
//
// Works for a single looping track (just assign 1 clip) or a
// full rotating playlist (assign several) — same script either way.
[RequireComponent(typeof(AudioSource))]
public class MusicPlaylist : MonoBehaviour
{
    [Header("TRACKS")]
    [SerializeField] private AudioClip[] tracks;

    [Header("OPTIONS")]
    [Tooltip("Shuffle play order, reshuffling every time it loops back to the start. If off, tracks always play in the order listed above.")]
    [SerializeField] private bool shuffleOrder = true;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.6f;

    private AudioSource audioSource;
    private List<int> playOrder;
    private int currentIndex;


    private void Awake()
    {
        audioSource =
            GetComponent<AudioSource>();

        // We advance tracks manually in Update(), so the
        // AudioSource itself should not loop or auto-play.
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }


    private void Start()
    {
        if (tracks == null || tracks.Length == 0)
        {
            Debug.LogWarning(
                "MUSIC PLAYLIST HAS NO TRACKS ASSIGNED!"
            );

            return;
        }

        BuildPlayOrder();

        currentIndex = 0;

        PlayCurrentTrack();
    }


    private void Update()
    {
        if (tracks == null || tracks.Length == 0)
            return;

        // Current track finished — move on to the next one.
        if (!audioSource.isPlaying)
        {
            AdvanceToNextTrack();
        }
    }


    // =========================================================
    // PLAY ORDER
    // =========================================================

    private void BuildPlayOrder()
    {
        playOrder =
            new List<int>();

        for (int i = 0; i < tracks.Length; i++)
        {
            playOrder.Add(i);
        }

        if (shuffleOrder)
        {
            ShufflePlayOrder();
        }
    }


    private void ShufflePlayOrder()
    {
        for (int i = 0; i < playOrder.Count; i++)
        {
            int swapIndex =
                Random.Range(i, playOrder.Count);

            int temp =
                playOrder[i];

            playOrder[i] =
                playOrder[swapIndex];

            playOrder[swapIndex] =
                temp;
        }
    }


    // =========================================================
    // TRACK PLAYBACK
    // =========================================================

    private void PlayCurrentTrack()
    {
        int trackIndex =
            playOrder[currentIndex];

        AudioClip clip =
            tracks[trackIndex];

        if (clip == null)
        {
            AdvanceToNextTrack();
            return;
        }

        audioSource.clip = clip;

        audioSource.Play();

        Debug.Log(
            "NOW PLAYING: " +
            clip.name
        );
    }


    private void AdvanceToNextTrack()
    {
        currentIndex++;

        if (currentIndex >= playOrder.Count)
        {
            currentIndex = 0;

            // Reshuffle each time we loop back to the start,
            // so the rotation order isn't identical every cycle.
            if (shuffleOrder)
            {
                ShufflePlayOrder();
            }
        }

        PlayCurrentTrack();
    }
}