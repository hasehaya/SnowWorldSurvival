using UnityEngine;

public class CounterTableParent :MonoBehaviour
{
    [SerializeField] private Sprite orderInfoSprite;
    [SerializeField] private MaterialType materialType;

    private CounterTable[] counterTables = new CounterTable[0];
    private ObjectStack objectStack;

    private void Awake()
    {
        counterTables = GetComponentsInChildren<CounterTable>();
        foreach (var counterTable in counterTables)
        {
            counterTable.CustomerPrefab.OrderInfo.IconImage.sprite = orderInfoSprite;
        }

        objectStack = GetComponentInChildren<ObjectStack>();
        objectStack.MaterialType = materialType;
    }
}
