using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Strategy
{
    altruist,
    thrower,
    cunning,
    unpredictable,
    vindictive,
    quirky
}

enum Behavior
{
    cooperate,
    swindle
}

class Merchant
{

    float gold = 0;
    Behavior current_behavior;
    Strategy current_strategy;

    public Merchant()
    {



    }

}

public class Game : MonoBehaviour
{



    int max_merchants = 60;
    int min_deals = 5;
    int max_deals = 10;

    List<Merchant> all_merchants = new List<Merchant>();



}
