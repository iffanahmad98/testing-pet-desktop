using UnityEngine;

[DisallowMultipleComponent]
public class TutorialMonsterTargetMarker : MonoBehaviour
{
    [Tooltip("ID unik untuk marker ini. Dipakai di PlainTutorialStepConfig.monsterTargetId.")]
    public string id;
}
