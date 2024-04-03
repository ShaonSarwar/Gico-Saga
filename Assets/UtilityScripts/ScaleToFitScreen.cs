using UnityEngine;

[ExecuteInEditMode]
public class ScaleToFitScreen : MonoBehaviour
{
    [SerializeField]private SpriteRenderer sr;
    public bool executeInEditor; 

    private void Awake()
    {
        FitScreenWithSprite(); 
    }

    private void FitScreenWithSprite()
    {
        //sr = GetComponent<SpriteRenderer>();

        // world height is always camera's orthographicSize * 2
        float worldScreenHeight = Camera.main.orthographicSize * 2;

        // world width is calculated by diving world height with screen heigh
        // then multiplying it with screen width
        float worldScreenWidth = worldScreenHeight / (Screen.height * Screen.width);

        // to scale the game object we divide the world screen width with the
        // size x of the sprite, and we divide the world screen height with the
        // size y of the sprite
        transform.localScale = new Vector3(
            Mathf.Clamp(worldScreenWidth / sr.sprite.bounds.size.x, 0.1f, float.MaxValue),
            Mathf.Clamp(worldScreenHeight / sr.sprite.bounds.size.y, 0.1f, float.MaxValue), 1);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (executeInEditor) FitScreenWithSprite(); 
    }
#endif
} // class
