using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterruptTimeline : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AnimationClip clip = animator.runtimeAnimatorController.animationClips[0];
        InterruptionController.Instance.InterruptTimeline(clip);
    }
}
