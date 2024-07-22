using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class InterruptionController : MonoBehaviour
{
    public static InterruptionController Instance { get; private set; }

    public Animator animator; // Reference to the Animator component
    public Animator[] Animators;
    public PlayableDirector timelineDirector;

    private AnimationClipPlayable _clipPlayable;
    private AnimationLayerMixerPlayable _mixerPlayable;
    private PlayableGraph _playableGraph;
    private Playable _timelinePlayable;
    private bool _isClipConnected = false;
    private bool _clipFinished = true;
    //
    private void Start()
    {
        Instance = this;
    }

    private void CreatePlayableGraph()
    {
        // Create the PlayableGraph
        _playableGraph = PlayableGraph.Create("Animation");

        // Create a Playable for the Timeline
        _timelinePlayable = timelineDirector.playableAsset.CreatePlayable(_playableGraph, timelineDirector.gameObject);
  
        var count = _playableGraph.GetOutputCountByType<AnimationPlayableOutput>();

        for (var i = 0; i < count; i++)
        {
            var output = (AnimationPlayableOutput)_playableGraph.GetOutputByType<AnimationPlayableOutput>(i);
            output.SetTarget(Animators[i]);

            if (i == 0)
            {
                // Create a MixerPlayable
                _mixerPlayable = AnimationLayerMixerPlayable.Create(_playableGraph, 2);
                
                // Connect the AnimationClipPlayable to the MixerPlayable
                _playableGraph.Connect(_timelinePlayable, 0, _mixerPlayable, 0);

                // Set full weight for the animation clip initially (100% timeline animation, 0% null animation)
                _mixerPlayable.SetInputWeight(0, 1f); 

                output.SetSourcePlayable(_mixerPlayable);
            }
        }

        // Play the graph
        _playableGraph.Play();
    }

    private void Update()
    {
        // Play the timeline
        if (Input.GetKeyDown(KeyCode.G)) {
            CreatePlayableGraph();
        }

        // Interrupt the timeline
        if (Input.GetKeyDown(KeyCode.Space)) {
            // If the animation clip is currently active, disconnect and restart it
            if (_isClipConnected) { Disconnect(); }

            animator.SetTrigger("BackHeadHit");
        }

        // Finish interruption
        if (_isClipConnected && _clipFinished) { Disconnect(); }

        // If the timeline is finished, destroy it
        if (_playableGraph.IsValid() && !_playableGraph.IsPlaying())
        {
            _playableGraph.Destroy();
        }
    }

    private void Disconnect()
    {
        // Disconnect the TimelinePlayable from the MixerPlayable
        _mixerPlayable.SetInputWeight(1, 0f); // Set weight for the timeline to 0
        _mixerPlayable.SetInputWeight(0, 1f); // Restore weight for the animation clip
        _playableGraph.Disconnect(_mixerPlayable, 1); // Disconnect animation clip overlap with 

        _isClipConnected = false;
    }

    public void InterruptTimeline(AnimationClip clip)
    {
        if (!_playableGraph.IsValid()) { return; }
        _clipPlayable = AnimationClipPlayable.Create(_playableGraph, clip);

        // Connect the TimelinePlayable to the MixerPlayable
        //clipPlayable output1 gets wired into mixerPlayable input1
        _playableGraph.Connect(_clipPlayable, 0, _mixerPlayable, 1);
        
        _mixerPlayable.SetLayerAdditive(1, true);
        _mixerPlayable.SetInputWeight(1, 1f);

        _clipPlayable.Play(); // Start the timeline

        _isClipConnected = true;
        _clipFinished = false;
        StartCoroutine(CountDown(clip.length));
    }

    IEnumerator CountDown(float length)
    {
        // Start a timer which automatically disconnects the animation clip when the animation clip ends
        float seconds = 0f;
        while (seconds < length) { seconds += Time.deltaTime; yield return null; }
        _clipFinished = true;
        yield return null;
    }
    void OnDestroy()
    {
        // Destroy the PlayableGraph when the GameObject is destroyed
        _playableGraph.Destroy();
    }
}
