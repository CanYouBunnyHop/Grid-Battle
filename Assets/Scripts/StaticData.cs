using UnityEngine;

public class StaticData : MonoBehaviour
{
    public static Material[] lineMats;
    [SerializeField] private Material[] materials;

    private void Awake()
    {
        lineMats = materials;
    }
}
