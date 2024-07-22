using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InterruptTimeline : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Look for the corresponding animation clip in the animator
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips) {
            if (stateInfo.IsName(clip.name)) {
                // Blend with the currently active timeline
                InterruptionController.Instance.InterruptTimeline(clip);
            }
        }
    }
}
