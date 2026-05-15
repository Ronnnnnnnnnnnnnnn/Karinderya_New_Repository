using UnityEngine;

public class BuffetContainer : MonoBehaviour
{
    [Header("Stored Food")]
    public ItemData storedFood;

    [Header("Servings")]
    public int currentServings = 0;
    public int maxServings = 10;

    [Header("Visual")]
    public Transform foodPoint;

    private GameObject currentVisual;

    // =========================
    // ADD FOOD FROM PLAYER
    // =========================

    public bool AddFood(ItemData food)
    {
        // BUFFET ALREADY HAS FOOD
        if(storedFood != null)
        {
            Debug.Log("Buffet already has food!");
            return false;
        }

        storedFood = food;

        currentServings = maxServings;

        SpawnVisual(food.gameModel);

        Debug.Log(
            food.itemName +
            " added to buffet with " +
            currentServings +
            " servings"
        );

        return true;
    }

    // =========================
    // TAKE 1 SERVING
    // =========================

    public bool TakeServing(ItemData wantedFood)
    {
        // NO FOOD
        if(storedFood == null)
        {
            Debug.Log("Buffet empty!");
            return false;
        }

        // WRONG FOOD
        if(storedFood != wantedFood)
        {
            Debug.Log("Wrong food type!");
            return false;
        }

        currentServings--;

        Debug.Log(
            "Served 1 " +
            storedFood.itemName +
            ". Remaining: " +
            currentServings
        );

        // EMPTY
        if(currentServings <= 0)
        {
            ClearBuffet();
        }

        return true;
    }

    // =========================
    // CLEAR BUFFET
    // =========================

    void ClearBuffet()
    {
        Debug.Log("Buffet empty now!");

        storedFood = null;

        currentServings = 0;

        if(currentVisual != null)
        {
            Destroy(currentVisual);
        }
    }

    // =========================
    // VISUALS
    // =========================

    void SpawnVisual(GameObject model)
    {
        if(currentVisual != null)
        {
            Destroy(currentVisual);
        }

        currentVisual = Instantiate(
            model,
            foodPoint.position,
            foodPoint.rotation
        );

        currentVisual.transform.SetParent(foodPoint);

        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;
        currentVisual.transform.localScale = Vector3.one;
    }
}