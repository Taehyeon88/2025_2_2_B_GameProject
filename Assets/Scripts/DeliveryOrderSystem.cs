using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryOrderSystem : MonoBehaviour
{
    [Header("�ֹ� ����")]
    public float ordergenrateInterval = 15f;     //�ֹ� ���� �ð�
    public int maxActiveOrders = 8;              //�ִ� �ֹ� ����

    [Header("���� ����")]
    public int totalOrdersGenerated = 0;
    public int completedOrders = 0;
    public int expiredOrders = 0;

    //�ֹ� ����Ʈ
    private List<DeliveryOrder> currentOrders = new List<DeliveryOrder>();

    //Building ����
    private List<Building> restaurants = new List<Building>();
    private List<Building> customers = new List<Building>();

    //Event �ý���
    [System.Serializable]
    public class OrderSystemEvents
    {
        public UnityEvent<DeliveryOrder> OnNewOrderAdded;
        public UnityEvent<DeliveryOrder> OnOrderPickUp;
        public UnityEvent<DeliveryOrder> OnOrderCompleted;
        public UnityEvent<DeliveryOrder> OnOrderExpired;
    }
    public OrderSystemEvents orderEvents;
    private DeliveryDriver driver;

    void Start()
    {
        driver = FindObjectOfType<DeliveryDriver>();
        FindAllBuilding();        //�ǹ� �ʱ� ����

        //�ʱ� �ֹ� ����
        StartCoroutine(GenerateInitialOrders());
        //�ֱ��� �ֹ� ����
        StartCoroutine(OrderGeneratorOrderGenerator());

        StartCoroutine(ExpiredOrderChecker());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FindAllBuilding()
    {
        Building[] allBuildings = FindObjectsOfType<Building>();
        
        foreach (Building building in allBuildings)
        {
            if (building.buildingType == BuildingType.Restaurant)
            {
                restaurants.Add(building);
            }
            else if(building.buildingType == BuildingType.Coustomer)
            {
                customers.Add(building);
            }
        }

        Debug.Log($"������ {restaurants.Count}��, �� {customers.Count} �� �߰�");
    }

    void CreateNewOrder()
    {
        if (restaurants.Count == 0 || customers.Count == 0) return;

        //���� �������� �� ����
        Building randomRestaurant = restaurants[Random.Range(0, restaurants.Count)];
        Building randomCustomer = customers[Random.Range(0, customers.Count)];

        //���� �ǹ��̸� �ٽ� ����
        if (randomRestaurant == randomCustomer)
        {
            randomCustomer = customers[Random.Range(0, customers.Count)];
        }

        float reward = Random.Range(3000f, 8000f);

        DeliveryOrder newOrder = new DeliveryOrder(

            ++totalOrdersGenerated,
            randomRestaurant,
            randomCustomer,
            reward
        );

        currentOrders.Add(newOrder);
        orderEvents.OnNewOrderAdded?.Invoke(newOrder);
    }

    void PickUpOrder(DeliveryOrder order)     //�Ⱦ� �Լ�
    {
        order.state = OrderState.PickedUp;
        orderEvents.OnOrderPickUp?.Invoke(order);
    }

    void CompleteOrder(DeliveryOrder order)  //��� �Ϸ��Լ�
    {
        order.state = OrderState.Completed;
        completedOrders++;

        //���� ����
        if (driver != null)
        {
            driver.AddMoney(order.reward);
        }

        //�Ϸ�� �ֹ� ����
        currentOrders.Remove(order);
        orderEvents.OnOrderCompleted?.Invoke(order);
    }

    void ExpireOrder(DeliveryOrder order)     //�ֹ� ��� �Ҹ�
    {
        order.state = OrderState.Expired;
        expiredOrders++;

        currentOrders.Remove(order);
        orderEvents.OnOrderExpired?.Invoke(order);
    }

    //UI ���� ����
    public List<DeliveryOrder> GetCurrentOrders()
    {
        return new List<DeliveryOrder>(currentOrders);
    }

    public int GetPickWaitingCount()
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrders)
        {
            if(order.state == OrderState.WaitingPickUp) count++;
        }
        return count;
    }

    public int GetDeliveryWaitingCount()
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.state == OrderState.PickedUp) count++;
        }
        return count;
    }

    DeliveryOrder FindOrderForPickUp(Building restaurant)  //�ֹ� ã���ִ� �Լ�
    {
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.restaurantBuilding == restaurant && order.state == OrderState.WaitingPickUp)
            {
                return order;
            }
        }
        return null;
    }

    DeliveryOrder FindOrderForDelivery(Building customer)  //�ֹ� ã���ִ� �Լ�
    {
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.customerBuilding == customer && order.state == OrderState.PickedUp)
            {
                return order;
            }
        }
        return null;
    }

    //Event ó��
    public void OnDeliverEnteredRestaurant(Building restaurant)
    {
        DeliveryOrder orderToPickUp = FindOrderForPickUp(restaurant);

        if (orderToPickUp != null)
        {
            PickUpOrder(orderToPickUp);
        }
    }

    public void OnDeliverEnteredCustomer(Building customer)
    {
        DeliveryOrder orderToDelivery = FindOrderForDelivery(customer);

        if (orderToDelivery != null)
        {
            CompleteOrder(orderToDelivery);
        }
    }

    IEnumerator GenerateInitialOrders()
    {
        yield return new WaitForSeconds(1f);

        //������ ��, 3�� �ֹ� ����
        for (int i = 0; i < 3; i++)
        {
            CreateNewOrder();
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator OrderGeneratorOrderGenerator()
    {
        while (true)
        {
            yield return new WaitForSeconds(ordergenrateInterval);
            if (currentOrders.Count < maxActiveOrders)
            {
                CreateNewOrder();
            }
        }
    }

    IEnumerator ExpiredOrderChecker()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            List<DeliveryOrder> expiredOrders = new List<DeliveryOrder>();
            foreach (DeliveryOrder order in currentOrders)
            {
                if (order.IsExpired() && order.state != OrderState.Completed)
                {
                    expiredOrders.Add(order);
                }
            }

            foreach (DeliveryOrder expired in expiredOrders)
            {
                ExpireOrder(expired);
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 50, 400, 1300));

        GUILayout.Label("===��� �ֹ�===");
        GUILayout.Label($"Ȱ�� �ֹ�: {currentOrders.Count}��");
        GUILayout.Label($"�Ⱦ� ���: {GetPickWaitingCount()}��");
        GUILayout.Label($"��� ���: {GetDeliveryWaitingCount()}��");
        GUILayout.Label($"�Ϸ�: {completedOrders}�� | ����: {expiredOrders}");

        GUILayout.Space(10);

        foreach (DeliveryOrder order in currentOrders)
        {
            string status = order.state == OrderState.WaitingPickUp ? "�Ⱦ����" : "��޴��";
            float timeLeft = order.GetRemainingTime();

            GUILayout.Label($"{order.orderId}:{order.restaurantName} -> {order.customerName}");
            GUILayout.Label($"{status} | {timeLeft:F0} �� ����");
        }

        GUILayout.EndArea();
    }
}
