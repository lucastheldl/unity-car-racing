using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public GameObject carLogoPanel;
    public int car;

    public bool raceStart;

    public float timer = 3f;
    private float currentTimer;

    public int maxLaps;
    public int currentLaps;
    public Text currentLapsText;

    public bool toLap;

    List<CarAiHandler> carList = new List<CarAiHandler>();
    public List <GameObject> finishedCars = new List<GameObject>();



    private void Start()
    {
        currentTimer = timer;
        foreach (CarAiHandler c in FindObjectsOfType<CarAiHandler>())
        {
            carList.Add(c);
        }

    }
    // Update is called once per frame
    void Update()
    {
        currentLapsText.text = currentLaps.ToString();


        if (finishedCars.Count == carList.Count && raceStart)
        {
            raceStart = false;
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            carLogoPanel.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Application.Quit();
        }
       
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (Time.time >= 10 && raceStart == false)
            {
                raceStart = true;
            }
        }
        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
        }
        if (currentTimer <= 0)
        {
            foreach (CarAiHandler c in carList)
            {
                int num = Random.Range(1, 6);
                c.maxSpeed = c.orignalMaximumSpeed + num;
            }
            currentTimer = timer;
        }

        if (car != 0) {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);

                Vector3 wordPos;
               // Ray ray = Camera.main.ScreenPointToRay(mousePos);
                //RaycastHit hit;
                //if (Physics.Raycast(ray, out hit, 1000f))
                //{
                   // wordPos = hit.point;
                //}
               // else
                //{
                    wordPos = Camera.main.ScreenToWorldPoint(mousePos);
                //}
                Instantiate(Resources.Load<GameObject>(car.ToString()), wordPos, Quaternion.identity);
                carList.Clear();

                foreach (CarAiHandler c in FindObjectsOfType<CarAiHandler>())
                {
                    carList.Add(c);
                }
                car = 0;
                //or for tandom rotarion use Quaternion.LookRotation(Random.insideUnitSphere)
            }
        }
    }
    public void ResetButton()
    {
        currentLaps = 0;
        raceStart = false;
        finishedCars.Clear();
        foreach (CarAiHandler car in FindObjectsOfType<CarAiHandler>())
        {
            Destroy(car.gameObject);
        }
    }
    public void BackBtn()
    {
        SceneManager.LoadScene("Menu");
    }public void StartBtn()
    {
        if (Time.time >= 10 && raceStart == false)
        {
            raceStart = true;
        }
    }
    public void CarsBtn()
    {
        carLogoPanel.SetActive(true);
    }
}
