using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : MonoBehaviour
{
    [SerializeField] private GridLogic gridLogic;
    [SerializeField] private Material batteryProgressBarMaterial;
    [SerializeField] private ParticleSystem batteryEffect; 

    // Battery Fill Amount 
    private float fillAmount;

    // Cell Count to Collect 
    private int cellToCollect;

    // Fill Unit for Battery per Cell Collect 
    private float fillUnit; 

    private void Awake()
    {
        fillAmount = 0f;
        fillUnit = 0f; 
        gridLogic.OnLevelSet += GridLogic_OnLevelSet;
        gridLogic.OnOneLayerCellDestroyed += GridLogic_OnOneLayerCellDestroyed;
    }

    private void GridLogic_OnLevelSet(object sender, GridLogic.OnLevelSetEventArgs e)
    {
        cellToCollect = gridLogic.GetTargetedCellCountToCollect();
        fillUnit = (1f / cellToCollect); 
    }

    private void GridLogic_OnOneLayerCellDestroyed(object sender, System.EventArgs e)
    {
        fillAmount += fillUnit;
        if (fillAmount > 0.98f)
        {
            fillAmount = 1.0f; 
        }
        batteryEffect.Play(); 
    }

    // Update is called once per frame
    void Update()
    {
        batteryProgressBarMaterial.SetFloat("_Fill", fillAmount); 
    }
}
