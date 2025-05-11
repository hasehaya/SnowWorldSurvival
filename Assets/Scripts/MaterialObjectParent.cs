using System.Collections.Generic;

using UnityEngine;

public class MaterialObjectParent :MonoBehaviour
{
    public MaterialType materialType;
    public int baseUnlockPrice = 75;
    public float unlockGrowthFactor = 1.1f;
    public int baseSellPrice = 5;
    [SerializeField] private Sprite orderInfoSprite;

    [SerializeField] private CounterTable counterTable1;
    [SerializeField] private CounterTable counterTable2;
    [SerializeField] private CounterTable counterTable3;
    [SerializeField] private ObjectStack objectStack;
    [SerializeField] private Activator activator;
    [SerializeField] private Unlockable officeHR;
    [SerializeField] private MaterialParent materialParent;
    [HideInInspector]
    public List<Unlockable> unlockables;

    private void Awake()
    {
        counterTable1.SetOrderIconSprite(orderInfoSprite);
        counterTable2.SetOrderIconSprite(orderInfoSprite);
        counterTable3.SetOrderIconSprite(orderInfoSprite);

        counterTable1.SetSellPrice(baseSellPrice);
        counterTable2.SetSellPrice(baseSellPrice);
        counterTable3.SetSellPrice(baseSellPrice);

        objectStack.MaterialType = materialType;

        activator.MaterialType = materialType;

        materialParent.MaterialType = materialType;

        unlockables = new List<Unlockable>
        {
            counterTable1,
            materialParent,
            counterTable2,
            materialParent,
            counterTable1,
            officeHR,
            counterTable2,
            materialParent,
            counterTable3,
            materialParent,
            counterTable1,
            materialParent,
            counterTable3,
            materialParent,
            counterTable2,
            counterTable3
        };
    }
}
