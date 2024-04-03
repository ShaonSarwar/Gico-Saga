using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraViewportSetup : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Sprite sprite;
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _camera.enabled = true;
        _camera.transform.position = new Vector3(spriteRenderer.transform.position.x, spriteRenderer.transform.position.y, _camera.transform.position.z);
        sprite = spriteRenderer.sprite;
        _camera.orthographic = true;
        _camera.rect = sprite.rect;
        _camera.orthographicSize = spriteRenderer.size.x;
    }
}
