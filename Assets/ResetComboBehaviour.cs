using UnityEngine;

public class ResetComboBehaviour : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerCombat playerCombat = animator.GetComponent<PlayerCombat>();

        if (playerCombat != null)
        {
            // Reset combo step and unlock inputs
            playerCombat.ResetCombo();
        }

        // Clear any lingering buffered attacks so they don't auto-fire later!
        animator.ResetTrigger("Attack");
    }
}