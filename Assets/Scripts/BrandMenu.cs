using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrandMenu : MonoBehaviour
{
    public GameObject carsMenu;
    public bool isCar;
    public int car;

    GameMaster gm;
    

    private void Start()
    {
        gm = FindObjectOfType<GameMaster>();
    }

    public void CarsMenu()
    {
        carsMenu.SetActive(true);
        this.transform.parent.gameObject.SetActive(false);
    }
    public void SelectCar()
    {
        gm.car = car;
        this.transform.parent.gameObject.SetActive(false);
    }
}
