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

    int id;
    int gold;
    int year_deals_count;    
    bool be_throw = false;

    List<int> trader_list;

    Behavior current_behavior;
    Strategy base_strategy;
    Strategy current_strategy;

    public Merchant(int id, Strategy strategy)
    {

        this.id = id;
        gold = 0;
        year_deals_count = 0;
        trader_list = new List<int>();
        base_strategy = strategy;

        set_strategy(strategy);        

    }

    public int get_id()
    {

        return id;

    }

    public Behavior get_behavior()
    {

        return current_behavior;

    }

    public void set_behavior(Behavior behavior)
    {

        current_behavior = behavior;

    }

    public Strategy get_strategy()
    {

        return current_strategy;

    }

    public void set_strategy(Strategy strategy)
    {

        current_strategy = strategy;

        switch (current_strategy)
        {

            case Strategy.altruist:
                current_behavior = Behavior.cooperate;
                break;

            case Strategy.thrower:
                current_behavior = Behavior.swindle;
                break;

            case Strategy.cunning:
                current_behavior = Behavior.cooperate;
                break;

            case Strategy.unpredictable:
                current_behavior = (Behavior)UnityEngine.Random.Range(0, 2);
                break;

            case Strategy.vindictive:
                current_behavior = Behavior.cooperate;
                break;

            case Strategy.quirky:
                current_behavior = Behavior.cooperate;
                break;

        }

    }

    public void reset_strategy()
    {

        set_strategy(base_strategy);

    }

    public int get_gold()
    {

        return gold;

    }

    public void add_gold(int value)
    {

        this.gold += value;

    }

    public int get_deals()
    {

        return year_deals_count;

    }

    public void reset()
    {

        this.gold = 0;
        this.year_deals_count = 0;
        trader_list.Clear();
        reset_strategy();

    }

    public void make_deal(Behavior behavior, int deal_count)
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

        year_deals_count++;

        if (behavior == Behavior.cooperate && current_behavior == Behavior.cooperate)
            add_gold(4);
        else if (behavior == Behavior.swindle && current_behavior == Behavior.cooperate)
            add_gold(1);
        else if (behavior == Behavior.cooperate && current_behavior == Behavior.swindle)
            add_gold(5);
        else
            add_gold(2);

        switch (current_strategy)
        {

            case Strategy.altruist:
                set_behavior(Behavior.cooperate);
                break;

            case Strategy.thrower:
                set_behavior(Behavior.swindle);
                break;

            case Strategy.cunning:
                set_behavior(behavior);
                break;

            case Strategy.unpredictable:
                set_behavior((Behavior)UnityEngine.Random.Range(0, 2));
                break;

            case Strategy.vindictive:
                if(behavior == Behavior.swindle)
                    set_behavior(behavior);
                break;

            case Strategy.quirky:

                if (behavior == Behavior.swindle)
                    this.be_throw = true;

                if(deal_count >= 5)
                {

                    if (be_throw)
                        set_strategy(Strategy.thrower);
                    else
                        set_strategy(Strategy.cunning);

                }

                break;

        }

    }

    public bool not_trade_yet(int id)
    {

        if(!trader_list.Contains(id))
        {

            trader_list.Add(id);
            return true;

        }

        return false;

    }

}

public class Game : MonoBehaviour
{

    [Header("Параметры: ")]
    [SerializeField] int max_merchants = 60;
    [SerializeField] int min_deals = 5;
    [SerializeField] int max_deals = 10;

    Dictionary<int, Merchant> all_merchants = new Dictionary<int, Merchant>();

    void Start()
    {

        init_merchants();

        UI_Manager.instance.create_merchant_list(all_merchants);

        UI_Manager.instance.on_next_move += next_move;
        UI_Manager.instance.on_next_year += next_year;

    }    

    void init_merchants()
    {

        int strategy_count = Enum.GetNames(typeof(Strategy)).Length;

        foreach (Strategy strategy in Enum.GetValues(typeof(Strategy)))
        {

            for (int i = 0; i < max_merchants/strategy_count; i++)
                create_merchant(strategy);

        }

    }

    void create_merchant(Strategy strategy)
    {

        int id = 1;

        if(all_merchants.Count > 0)
        {

            Merchant last = all_merchants[all_merchants.Keys.Max()];
            id = last.get_id() + 1;

        }

        all_merchants.Add(id, new Merchant(id, strategy));

    }

    void next_move(object sender, EventArgs e)
    {        

        foreach (KeyValuePair<int, Merchant> merch1 in all_merchants)
        {

            foreach (KeyValuePair<int, Merchant> merch2 in all_merchants)
            {

                if(
                    merch1.Key != merch2.Key && 
                    merch1.Value.not_trade_yet(merch2.Key) && 
                    merch2.Value.not_trade_yet(merch1.Key)
                    )
                    make_deals(merch1.Value, merch2.Value);                                

            }

        }

        update_merchants();

    }

    void make_deals(Merchant fisrt, Merchant second)
    {        

        int random_deals_count = UnityEngine.Random.Range(min_deals, max_deals + 1);
        int deal_count = 0;

        for (int i = 0; i < random_deals_count; i++)
        {

            deal_count++;
            Behavior merch1_behavior = fisrt.get_behavior();
            Behavior merch2_behavior = second.get_behavior();

            /*        
            В процессе сделки для каждого торговца существует 5% вероятность ошибиться 
            и принять неправильное решение: 
            сжульничать вместо того, чтобы сотрудничать, или наоборот.
            */

            if (get_random(5))
            {

                merch1_behavior = merch1_behavior == Behavior.cooperate ? Behavior.swindle : Behavior.cooperate;

            }

            if (get_random(5))
            {

                merch2_behavior = merch2_behavior == Behavior.cooperate ? Behavior.swindle : Behavior.cooperate;

            }

            fisrt.make_deal(merch2_behavior, deal_count);
            second.make_deal(merch1_behavior, deal_count);

        }

        fisrt.reset_strategy();
        second.reset_strategy();

    }

    void update_merchants()
    {

        all_merchants = (from entry in all_merchants orderby entry.Value.get_gold() descending select entry)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        UI_Manager.instance.create_merchant_list(all_merchants);

    }

    void next_year(object sender, EventArgs e)
    {

        /*
         * Мерило успеха в Гильдии - прибыль, которую торговец заработал за последний год. 
         * В конце каждого года 20% самых неуспешных торговцев с позором исключают из Гильдии, 
         * а их место занимает ровно столько же новых, 
         * которые копируют поведение 20% самых успешных членов Гильдии.
         */

        // Отбираем 20 самых неуспешных
        var poor_merchants = (from entry in all_merchants orderby entry.Value.get_gold() ascending select entry)
            .Take(20)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        // Отбираем 20 самых успешных
        var good_merchants = (from entry in all_merchants orderby entry.Value.get_gold() descending select entry)
            .Take(20)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        // Удаляем
        foreach (KeyValuePair<int, Merchant> merch in poor_merchants)
        {

            all_merchants.Remove(merch.Key);

        }

        // Очищаем
        foreach (KeyValuePair<int, Merchant> merch in all_merchants)
        {

            merch.Value.reset();

        }

        // Добавляем
        foreach (KeyValuePair<int, Merchant> merch in good_merchants)
        {

            create_merchant(merch.Value.get_strategy());

        }

        UI_Manager.instance.create_merchant_list(all_merchants);

    }

    bool get_random(int procent)
    {

        if (UnityEngine.Random.Range(1, 101) <= procent)
        {

            return true;

        }

        return false;

    }

}
