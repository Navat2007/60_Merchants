using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{

    public static UI_Manager instance;

    public event EventHandler on_next_move;
    public event EventHandler on_next_year;

    [SerializeField] GameObject merchant_list_container;
    [SerializeField] GameObject merchant_row_prefab;

    [SerializeField] Button next_move_btn;
    [SerializeField] Button next_year_btn;

    private void Start()
    {

        if (instance != null)
            Destroy(gameObject);

        instance = this;

        next_move_btn.onClick.AddListener(delegate 
        { 
            on_next_move?.Invoke(this, EventArgs.Empty);
            next_move_btn.interactable = false;
            next_year_btn.interactable = true;
        });

        next_year_btn.onClick.AddListener(delegate
        {
            on_next_year?.Invoke(this, EventArgs.Empty);
            next_move_btn.interactable = true;
            next_year_btn.interactable = false;
        });

    }

    public void create_merchant_list(Dictionary<int, Merchant> list)
    {

        foreach (Transform child in merchant_list_container.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (KeyValuePair<int, Merchant> item in list)
        {

            var row = Instantiate(merchant_row_prefab, Vector3.zero, Quaternion.identity, merchant_list_container.transform);

            Transform id = row.transform.Find("ID_Text");
            id.GetComponent<TextMeshProUGUI>().text = item.Value.get_id().ToString();

            Transform strategy = row.transform.Find("Strategy_Text");
            strategy.GetComponent<TextMeshProUGUI>().text = item.Value.get_strategy().ToString();

            Transform gold = row.transform.Find("Gold_Text");
            gold.GetComponent<TextMeshProUGUI>().text = $"Gold: {item.Value.get_gold().ToString()}";

            Transform deals = row.transform.Find("Deals_Text");
            deals.GetComponent<TextMeshProUGUI>().text = $"Deals: {item.Value.get_deals().ToString()}";

        }

    }

}
