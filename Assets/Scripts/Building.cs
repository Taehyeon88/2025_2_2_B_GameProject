using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Building : MonoBehaviour
{
    [Header("건물 정보")]
    public BuildingType buildingType;
    public string buildingName = "건물";

    [System.Serializable]
    public class BuildingEvents
    {
        public UnityEvent<string> OnDriverEntered;
        public UnityEvent<string> OnDriverExited;
        public UnityEvent<BuildingType> OnServiceUsed;
    }

    public BuildingEvents buildingEvents;

    void Start()
    {
        SetupBuilding();
    }

    void Update()
    {
        
    }

    void SetupBuilding()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            switch (buildingType)
            {
                case BuildingType.Restaurant:
                    mat.color = Color.red;
                    buildingName = "음식점";
                    break;

                case BuildingType.Coustomer:
                    mat.color = Color.green;
                    buildingName = "고객집";
                    break;

                case BuildingType.ChargingStation:
                    mat.color = Color.blue;
                    buildingName = "충전소";
                    break;
            }
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        DeliveryDriver driver = other.GetComponent<DeliveryDriver>();
        if (driver != null)
        {
            buildingEvents.OnDriverEntered?.Invoke(buildingName);
            HandleDriverService(driver);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DeliveryDriver driver = other.GetComponent<DeliveryDriver>();
        if (driver != null)
        {
            buildingEvents.OnDriverExited?.Invoke(buildingName);
            Debug.Log($"{buildingName}을 떠났습니다.");
        }
    }

    void HandleDriverService(DeliveryDriver driver)
    {
        switch (buildingType)
        {
            case BuildingType.Restaurant:
                Debug.Log($"{buildingName}에서 음식을 픽업했습니다");
            break;

            case BuildingType.Coustomer:
                Debug.Log($"{buildingName}에 배달완료");
                driver.CompleteDelivery();
            break;

            case BuildingType.ChargingStation:
                Debug.Log($"{buildingName}에서 배터리를 충전했습니다");
                driver.ChargeBattery();
            break;
        }
    }
}
