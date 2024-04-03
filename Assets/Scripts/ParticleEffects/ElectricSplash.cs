using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class ElectricSplash : MonoBehaviour
{
    [SerializeField] private GridLogic gridLogic;
    [SerializeField] private GridLogicVisual gridLogicVisual;
    [SerializeField] private Gridpooler gridpooler;
    [SerializeField] private Transform electricRecieverPoint;

    private void Awake()
    {
        //gridLogic.OnElectricSplash += GridLogic_OnElectricSplash;
    }

    // Splash Only allowed for Cell,,,, None other items produces Splash 
    private void GridLogic_OnElectricSplash(object sender, EventArgs e)
    {
        GridItemPosition gridItemPosition = sender as GridItemPosition;
        if (!gridItemPosition.HasCell()) return;

        // take destroyed GridItem position for enable Splash Particle System  
        Vector3 splashPosition = gridItemPosition.GetWorldPosition();
        // Initialize Splash from pool 
        GameObject splashObject = gridpooler.GetPooledGridObject(PoolType.Splash, splashPosition + new Vector3(0.5f, 0.5f), Quaternion.identity);

        if (splashObject != null)
        {
            // Calculate direction towards Tower Object for Splash Particle System 
            Vector3 normal = (electricRecieverPoint.position - splashObject.transform.position).normalized;
            float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
            Quaternion rot = new Quaternion();
            rot.eulerAngles = new Vector3(0, 0, angle - 90);
            splashObject.transform.rotation = rot;

            // Cache all the insulator collider 
            //List<CircleCollider2D> colliders = gridLogicVisual.GetAllInsulatorCollider();
            ParticleSystem particleSystem = splashObject.GetComponent<ParticleSystem>();
            //foreach (CircleCollider2D circleCollider2D in colliders)
            //{
            //    particleSystem.trigger.AddCollider(circleCollider2D);
            //}

            particleSystem.Play();

            // object return to pool 
            FunctionTimer.Create(() =>
            {
                gridpooler.ReturnGridObjectToPool(PoolType.Splash, splashObject);
            }, 0.5f);
        }
    }
}
