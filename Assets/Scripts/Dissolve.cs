using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    private Material itemMaterial;
    [SerializeField] private bool startDissolve;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float smoothSpeed; 
    float start, target=1; 
    // Start is called before the first frame update
    void Start()
    {
        itemMaterial = GetComponent<SpriteRenderer>().material; 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            startDissolve = true; 
        }
        if (startDissolve)
        {
            //target = target == 0 ? 1 : 0;
            start = Mathf.MoveTowards(start, target, smoothSpeed * Time.deltaTime);          
            itemMaterial.SetFloat("_dissolveAmount", Mathf.Lerp(0, 1, curve.Evaluate(start)));
           
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            startDissolve = false;
            start = 0;
            target = 1;
            itemMaterial.SetFloat("_dissolveAmount", 0); 
        }
    }
}
