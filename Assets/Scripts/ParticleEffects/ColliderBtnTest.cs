using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderBtnTest : MonoBehaviour
{
    [SerializeField] private Camera mCamera;
    [SerializeField] private CircleCollider2D levelBtnCollider;

    private bool btnEnabled;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        btnEnabled = false; 
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 1 && !btnEnabled)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = GravitimeUtility.GetTouchPosition(mCamera, touch.position);
            Vector3 pos = new Vector3(touchPosition.x, touchPosition.y, 0f); 
            if (levelBtnCollider == Physics2D.OverlapPoint(pos))
            {
                Debug.Log("Level Btn Clicked! Generic");
                btnEnabled = true; 
            }
        }
        else
        {
            return; 
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Level Btn Clicked!"); 
    }

    public void Touch()
    {
       
    }
}
