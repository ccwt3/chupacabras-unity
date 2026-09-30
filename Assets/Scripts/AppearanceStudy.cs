using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Chupacabras {
// Three lighting studies from existing Blender rigs, not the final 40 s
// sequence.
public sealed class AppearanceStudy : MonoBehaviour {
  public Transform visualRoot, cinemaRoot;
  public Camera cinema;
  public Renderer panel;
  public GameObject calm, contact;
  public Animator sheepAnimator, contactAnimator;
  public AnimationClip sheepClip, contactClip;
  public UniversalRenderPipelineAsset pipeline;
  public string ShotLabel { get; private set; }
  private RenderTexture texture;
  private PlayableGraph graph;
  private AnimationClipPlayable sheepPlayable, contactPlayable;
  private RenderPipelineAsset previousPipeline;
  private bool attached;

  public void Attach(Transform anchor) {
    visualRoot.SetParent(anchor, false);
    previousPipeline = QualitySettings.renderPipeline;
    if (Application.isPlaying)
      QualitySettings.renderPipeline = pipeline;
    RenderSettings.ambientMode = AmbientMode.Flat;
    RenderSettings.ambientLight = new Color(.23f, .27f, .36f);
    RenderSettings.skybox = null;
    RenderSettings.fog = false;
    texture = new RenderTexture(960, 540, 24) { name = "Appearance_960x540",
                                                antiAliasing = 1 };
    texture.Create();
    cinema.targetTexture = texture;
    panel.material.SetTexture("_BaseMap", texture);
    graph = PlayableGraph.Create("Existing rig lighting poses");
    graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    sheepPlayable = AnimationClipPlayable.Create(graph, sheepClip);
    contactPlayable = AnimationClipPlayable.Create(graph, contactClip);
    sheepPlayable.SetApplyFootIK(false);
    contactPlayable.SetApplyFootIK(false);
    AnimationPlayableOutput.Create(graph, "Sheep", sheepAnimator)
        .SetSourcePlayable(sheepPlayable);
    AnimationPlayableOutput.Create(graph, "Contact", contactAnimator)
        .SetSourcePlayable(contactPlayable);
    graph.Play();
    attached = true;
    Present(false, 0, true);
  }

  public void Present(bool visible, double seconds, bool renderEnabled) {
    if (!attached)
      return;
    int shot = (int)(seconds / 6) % 3;
    calm.SetActive(shot == 0);
    contact.SetActive(shot != 0);
    sheepPlayable.SetTime(1.8);
    contactPlayable.SetTime(shot == 1 ? 0 : 3);
    graph.Evaluate(0);
    Vector3 target =
        shot == 0 ? new Vector3(-.5f, 1.3f, 1.5f) : new Vector3(-.7f, .65f, 0);
    Vector3 eye = shot == 0   ? new Vector3(5, 3.2f, -9)
                  : shot == 1 ? new Vector3(4.7f, 2.7f, 5.5f)
                              : new Vector3(-6, 3, 5.4f);
    cinema.transform.localPosition = eye;
    cinema.transform.LookAt(cinemaRoot.TransformPoint(target));
    ShotLabel = shot == 0   ? "Pastoreo"
                : shot == 1 ? "Agarre"
                            : "Espinas y lana";
    cinema.enabled = visible && renderEnabled;
    panel.gameObject.SetActive(renderEnabled);
  }
  private void OnDestroy() { ReleaseResources(); }
  public void ReleaseResources() {
    if (graph.IsValid())
      graph.Destroy();
    if (attached && Application.isPlaying)
      QualitySettings.renderPipeline = previousPipeline;
    if (texture != null) {
      texture.Release();
      if (Application.isPlaying)
        Destroy(texture);
      else
        DestroyImmediate(texture);
    }
  }
}
}
