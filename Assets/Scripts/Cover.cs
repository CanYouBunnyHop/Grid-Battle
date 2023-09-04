using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Cover : MonoBehaviour
{
    [SerializeField] private GameObject Up;
    [SerializeField] private GameObject Down;
    [SerializeField] private GameObject Left;
    [SerializeField] private GameObject Right;
    public LayerMask coverLayer;
    private readonly Vector3 heightOffset = new Vector3(0, 0.25f, 0);
    void Update()
    {
         Up.SetActive(Physics.Raycast(transform.position + heightOffset , Vector3.forward, 1.1f, coverLayer));
         Down.SetActive(Physics.Raycast(transform.position + heightOffset, Vector3.back, 1.1f, coverLayer)); 
         Left.SetActive(Physics.Raycast(transform.position + heightOffset, Vector3.left, 1.1f, coverLayer)); 
         Right.SetActive(Physics.Raycast(transform.position + heightOffset, Vector3.right, 1.1f, coverLayer)); 
    }
}
