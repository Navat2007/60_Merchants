using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Strategy
{
    altruist,
    thrower,
    cunning,
    unpredictable,
    vindictive,
    quirky
}

public enum Behavior
{
    cooperate,
    swindle
}

public class Merchant
{

    private int _id;
    private int _gold;
    private int _yearDealsCount;
    private bool _beThrow = false;

    private List<int> _traderList;

    private Behavior _currentBehavior;
    private Strategy _baseStrategy;
    private Strategy _currentStrategy;

    public Merchant(int id, Strategy strategy)
    {

        _id = id;
        _gold = 0;
        _yearDealsCount = 0;
        _traderList = new List<int>();
        _baseStrategy = strategy;

        SetStrategy(strategy);        

    }

    public int GetId()
    {

        return _id;

    }

    public Behavior GetBehavior()
    {

        return _currentBehavior;

    }

    public void SetBehavior(Behavior behavior)
    {

        _currentBehavior = behavior;

    }

    public Strategy GetStrategy()
    {

        return _currentStrategy;

    }

    public void SetStrategy(Strategy strategy)
    {

        _currentStrategy = strategy;

        switch (_currentStrategy)
        {

            case Strategy.altruist:
                _currentBehavior = Behavior.cooperate;
                break;

            case Strategy.thrower:
                _currentBehavior = Behavior.swindle;
                break;

            case Strategy.cunning:
                _currentBehavior = Behavior.cooperate;
                break;

            case Strategy.unpredictable:
                _currentBehavior = (Behavior)UnityEngine.Random.Range(0, 2);
                break;

            case Strategy.vindictive:
                _currentBehavior = Behavior.cooperate;
                break;

            case Strategy.quirky:
                _currentBehavior = Behavior.cooperate;
                break;

        }

    }

    public void ResetStrategy()
    {

        SetStrategy(_baseStrategy);

    }

    public int GetGold()
    {

        return _gold;

    }

    public void AddGold(int value)
    {

        this._gold += value;

    }

    public int GetYearDeals()
    {

        return _yearDealsCount;

    }

    public void Reset()
    {

        this._gold = 0;
        this._yearDealsCount = 0;
        _traderList.Clear();
        ResetStrategy();

    }

    public void MakeDeal(Behavior behavior, int dealCount)
    {

        /*
        При свершении сделки у каждого из торговцев есть 2 варианта поведения: 
        либо честно сотрудничать и выполнять все свои обязательства, 
        либо сжульничать. 
        От этого выбора зависит размер их прибыли:

        в том случае, если оба проводят сделку честно, оба зарабатывают по 4 золотых;
        если один торговец сжульничает, а другой продолжит честно сотрудничать, 
        то жулик получит 5 золотых, а честный торогвец - всего 1;
        если оба сжульничают, то каждый получит только по 2 золотых.
        */

        _yearDealsCount++;

        if (behavior == Behavior.cooperate && _currentBehavior == Behavior.cooperate)
            AddGold(4);
        else if (behavior == Behavior.swindle && _currentBehavior == Behavior.cooperate)
            AddGold(1);
        else if (behavior == Behavior.cooperate && _currentBehavior == Behavior.swindle)
            AddGold(5);
        else
            AddGold(2);

        switch (_currentStrategy)
        {

            case Strategy.altruist:
                SetBehavior(Behavior.cooperate);
                break;

            case Strategy.thrower:
                SetBehavior(Behavior.swindle);
                break;

            case Strategy.cunning:
                SetBehavior(behavior);
                break;

            case Strategy.unpredictable:
                SetBehavior((Behavior)UnityEngine.Random.Range(0, 2));
                break;

            case Strategy.vindictive:
                if(behavior == Behavior.swindle)
                    SetBehavior(behavior);
                break;

            case Strategy.quirky:

                if (behavior == Behavior.swindle)
                    this._beThrow = true;

                if(dealCount >= 5)
                {

                    if (_beThrow)
                        SetStrategy(Strategy.thrower);
                    else
                        SetStrategy(Strategy.cunning);

                }

                break;

        }

    }

    public bool NotTradeYet(int id)
    {

        if(!_traderList.Contains(id))
        {

            _traderList.Add(id);
            return true;

        }

        return false;

    }

}

public class Game : MonoBehaviour
{

    [SerializeField] private UiManager _ui;

    [Header("Параметры: ")]
    [SerializeField] private int _maxMerchants = 60;
    [SerializeField] private int _minDeals = 5;
    [SerializeField] private int _maxDeals = 10;

    private Dictionary<int, Merchant> _allMerchants = new Dictionary<int, Merchant>();

    private void Start()
    {

        InitMerchants();

        _ui.CreateMerchantList(_allMerchants);

        _ui.onNextMove += NextMove;
        _ui.onNextYear += NextYear;

    }

    private void InitMerchants()
    {

        int strategy_count = Enum.GetNames(typeof(Strategy)).Length;

        foreach (Strategy strategy in Enum.GetValues(typeof(Strategy)))
        {

            for (int i = 0; i < _maxMerchants/strategy_count; i++)
                CreateMerchant(strategy);

        }

    }

    private void CreateMerchant(Strategy strategy)
    {

        int id = 1;

        if(_allMerchants.Count > 0)
        {

            Merchant last = _allMerchants[_allMerchants.Keys.Max()];
            id = last.GetId() + 1;

        }

        _allMerchants.Add(id, new Merchant(id, strategy));

    }

    private void NextMove(object sender, EventArgs e)
    {        

        foreach (KeyValuePair<int, Merchant> merch1 in _allMerchants)
        {

            foreach (KeyValuePair<int, Merchant> merch2 in _allMerchants)
            {

                if(
                    merch1.Key != merch2.Key && 
                    merch1.Value.NotTradeYet(merch2.Key) && 
                    merch2.Value.NotTradeYet(merch1.Key)
                    )
                    MakeDeals(merch1.Value, merch2.Value);                                

            }

        }

        UpdateMerchants();

    }

    private void MakeDeals(Merchant fisrt, Merchant second)
    {        

        int randomDealsCount = UnityEngine.Random.Range(_minDeals, _maxDeals + 1);
        int dealCount = 0;

        for (int i = 0; i < randomDealsCount; i++)
        {

            dealCount++;
            Behavior merch1Behavior = fisrt.GetBehavior();
            Behavior merch2Behavior = second.GetBehavior();

            /*        
            В процессе сделки для каждого торговца существует 5% вероятность ошибиться 
            и принять неправильное решение: 
            сжульничать вместо того, чтобы сотрудничать, или наоборот.
            */

            if (GetRandom(5))
            {

                merch1Behavior = merch1Behavior == Behavior.cooperate ? Behavior.swindle : Behavior.cooperate;

            }

            if (GetRandom(5))
            {

                merch2Behavior = merch2Behavior == Behavior.cooperate ? Behavior.swindle : Behavior.cooperate;

            }

            fisrt.MakeDeal(merch2Behavior, dealCount);
            second.MakeDeal(merch1Behavior, dealCount);

        }

        fisrt.ResetStrategy();
        second.ResetStrategy();

    }

    private void UpdateMerchants()
    {

        _allMerchants = (from entry in _allMerchants orderby entry.Value.GetGold() descending select entry)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        _ui.CreateMerchantList(_allMerchants);

    }

    private void NextYear(object sender, EventArgs e)
    {

        /*
         * Мерило успеха в Гильдии - прибыль, которую торговец заработал за последний год. 
         * В конце каждого года 20% самых неуспешных торговцев с позором исключают из Гильдии, 
         * а их место занимает ровно столько же новых, 
         * которые копируют поведение 20% самых успешных членов Гильдии.
         */

        // Отбираем 20 самых неуспешных
        var _poorMerchants = (from entry in _allMerchants orderby entry.Value.GetGold() ascending select entry)
            .Take(20)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        // Отбираем 20 самых успешных
        var _goodMerchants = (from entry in _allMerchants orderby entry.Value.GetGold() descending select entry)
            .Take(20)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        // Удаляем
        foreach (KeyValuePair<int, Merchant> merch in _poorMerchants)
        {

            _allMerchants.Remove(merch.Key);

        }

        // Очищаем
        foreach (KeyValuePair<int, Merchant> merch in _allMerchants)
        {

            merch.Value.Reset();

        }

        // Добавляем
        foreach (KeyValuePair<int, Merchant> merch in _goodMerchants)
        {

            CreateMerchant(merch.Value.GetStrategy());

        }

        _ui.CreateMerchantList(_allMerchants);

    }

    private bool GetRandom(int procent)
    {

        if (UnityEngine.Random.Range(1, 101) <= procent)
        {

            return true;

        }

        return false;

    }

}