using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Chupacabras
{
// Preliminary clip player for steps 4–5; the production Timeline is step 17.
public sealed class SequencePreview : MonoBehaviour
{
    public Animator animator;
    public AnimationClip clip;
    public bool playing = true;
    [Range(0, 40)]
    public float seconds;
    private PlayableGraph graph;
    private AnimationClipPlayable playable;
    public const double Duration = 40;
    private double elapsed;
    private void OnEnable()
    {
        Initialize();
        Evaluate(0);
    }
    private void Initialize()
    {
        graph = PlayableGraph.Create("Blocking preview");
        graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        playable = AnimationClipPlayable.Create(graph, clip);
        playable.SetApplyFootIK(false);
        var output = AnimationPlayableOutput.Create(graph, "Imported FBX", animator);
        output.SetSourcePlayable(playable);
        animator.applyRootMotion = false;
        graph.Play();
    }
    public void Evaluate(double time)
    {
        if (!graph.IsValid())
            Initialize();
        elapsed = time;
        seconds = (float)(time % Duration);
        playable.SetTime(seconds);
        graph.Evaluate(0);
    }
    private void Update()
    {
        if (playing)
            Evaluate(elapsed + Time.deltaTime);
    }
    private void OnDisable()
    {
        if (graph.IsValid())
            graph.Destroy();
    }
}
}
