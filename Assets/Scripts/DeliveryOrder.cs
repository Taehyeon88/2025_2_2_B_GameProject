using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//간단한 배달 주문
[System.Serializable]
public class DeliveryOrder
{
    public int orderId;
    public string restaurantName;
    public string customerName;
    public Building restaurantBuilding;
    public Building customerBuilding;
    public float orderTime;
    public float timeLimit;
    public float reward;
    public OrderState state;

    public DeliveryOrder(int id, Building restaurant, Building customer, float rewardAmount)
    {
        orderId = id;
        restaurantBuilding = restaurant;
        customerBuilding = customer;
        restaurantName = restaurant.buildingName;
        customerName = customer.buildingName;
        orderTime = Time.time;
        timeLimit = Random.Range(60f, 120f);
        reward = rewardAmount;
        state = OrderState.WaitingPickUp;
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0f, timeLimit - (Time.time - orderTime));          //남은 시간 리턴
    }

    public bool IsExpired()
    {
        return GetRemainingTime() <= 0f;
    }
}
