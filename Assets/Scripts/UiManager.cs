using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{

    public event EventHandler onNextMove;
    public event EventHandler onNextYear;

    [SerializeField] private GameObject _merchantListContainer;
    [SerializeField] private GameObject _merchantRowPrefab;

    [SerializeField] private Button _nextMoveBtn;
    [SerializeField] private Button _nextYearBtn;

    private void Start()
    {

        _nextMoveBtn.onClick.AddListener(delegate 
        { 
            onNextMove?.Invoke(this, EventArgs.Empty);
            _nextMoveBtn.interactable = false;
            _nextYearBtn.interactable = true;
        });

        _nextYearBtn.onClick.AddListener(delegate
        {
            onNextYear?.Invoke(this, EventArgs.Empty);
            _nextMoveBtn.interactable = true;
            _nextYearBtn.interactable = false;
        });

    }

    public void CreateMerchantList(Dictionary<int, Merchant> list)
    {

        foreach (Transform child in _merchantListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (KeyValuePair<int, Merchant> item in list)
        {

            var row = Instantiate(_merchantRowPrefab, Vector3.zero, Quaternion.identity, _merchantListContainer.transform);

            Transform id = row.transform.Find("ID_Text");
            id.GetComponent<TextMeshProUGUI>().text = item.Value.GetId().ToString();

            Transform strategy = row.transform.Find("Strategy_Text");
            strategy.GetComponent<TextMeshProUGUI>().text = item.Value.GetStrategy().ToString();

            Transform gold = row.transform.Find("Gold_Text");
            gold.GetComponent<TextMeshProUGUI>().text = $"Gold: {item.Value.GetGold().ToString()}";

            Transform deals = row.transform.Find("Deals_Text");
            deals.GetComponent<TextMeshProUGUI>().text = $"Deals: {item.Value.GetYearDeals().ToString()}";

        }

    }

}
