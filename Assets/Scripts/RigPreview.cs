using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Chupacabras {
// Isolated rig acceptance samples; their length does not change the production
// timeline.
public sealed class RigPreview : MonoBehaviour {
  public Animator animator;
  public AnimationClip clip;
  private PlayableGraph graph;
  private AnimationClipPlayable playable;
  private double elapsed;
  private void OnEnable() {
    if (clip == null || animator == null)
      return;
    graph = PlayableGraph.Create("Rig acceptance preview");
    graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    playable = AnimationClipPlayable.Create(graph, clip);
    playable.SetApplyFootIK(false);
    AnimationPlayableOutput.Create(graph, "Rigs", animator)
        .SetSourcePlayable(playable);
    animator.applyRootMotion = false;
    elapsed = 0;
    graph.Play();
  }
  private void Update() {
    if (!graph.IsValid())
      return;
    elapsed = (elapsed + Time.deltaTime) % clip.length;
    playable.SetTime(elapsed);
    graph.Evaluate(0);
  }
  private void OnDisable() {
    if (graph.IsValid())
      graph.Destroy();
  }
}
}
