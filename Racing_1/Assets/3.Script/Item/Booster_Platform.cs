using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster_Platform : MonoBehaviour
{
    [SerializeField] private MeshRenderer arrow_renderer;
    
    // Update is called once per frame
    void Update()
    {
        arrow_renderer.material.mainTextureOffset = new Vector2(arrow_renderer.material.mainTextureOffset.x, (arrow_renderer.material.mainTextureOffset.y - Time.deltaTime)) ;
    }
}
