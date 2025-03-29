using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AstronautGoalSeeker : MonoBehaviour
{
    Goal[] mGoals;
    Action[] mActions;
    Action mChangeOverTime;
    const float TICK_LENGTH = 4.0f;
    public TMP_Text stats;
    public TMP_Text action;
    public TMP_Text lowTitle;
    public GameObject explosionEffect;

    private bool hasDied = false;

    void Start()
    {
        mGoals = new Goal[3];
        mGoals[0] = new Goal("Oxygen Depletion", 5);
        mGoals[1] = new Goal("Exhaustion", 4);
        mGoals[2] = new Goal("Insanity", 3);

        mActions = new Action[9];

        // Initialize actions
        mActions[0] = new Action("replenish oxygen");
        mActions[1] = new Action("take a nap");
        mActions[2] = new Action("tell jokes to mission control");
        mActions[3] = new Action("stare into the abyss of space");
        mActions[4] = new Action("conduct scientific experiments on myself");
        mActions[5] = new Action("float around aimlessly");
        mActions[6] = new Action("try to high-five an alien");
        mActions[7] = new Action("play zero-gravity tag with myself");
        mActions[8] = new Action("argue with the stars");

        // Assign effects of each action
        mActions[0].targetGoals.Add(new Goal("Oxygen Depletion", -5f));
        mActions[0].targetGoals.Add(new Goal("Exhaustion", -1f));
        mActions[0].targetGoals.Add(new Goal("Insanity", -2f));

        mActions[1].targetGoals.Add(new Goal("Oxygen Depletion", -1f));
        mActions[1].targetGoals.Add(new Goal("Exhaustion", -4f));
        mActions[1].targetGoals.Add(new Goal("Insanity", -2f));

        mActions[2].targetGoals.Add(new Goal("Oxygen Depletion", 0f));
        mActions[2].targetGoals.Add(new Goal("Exhaustion", -1f));
        mActions[2].targetGoals.Add(new Goal("Insanity", -3f));

        mActions[3].targetGoals.Add(new Goal("Oxygen Depletion", +2f));
        mActions[3].targetGoals.Add(new Goal("Exhaustion", +1f));
        mActions[3].targetGoals.Add(new Goal("Insanity", +4f));

        mActions[4].targetGoals.Add(new Goal("Oxygen Depletion", +2f));
        mActions[4].targetGoals.Add(new Goal("Exhaustion", +3f));
        mActions[4].targetGoals.Add(new Goal("Insanity", +6f));

        mActions[5].targetGoals.Add(new Goal("Oxygen Depletion", +1f));
        mActions[5].targetGoals.Add(new Goal("Exhaustion", +1f));
        mActions[5].targetGoals.Add(new Goal("Insanity", +3f));

        mActions[6].targetGoals.Add(new Goal("Oxygen Depletion", +2f));
        mActions[6].targetGoals.Add(new Goal("Exhaustion", +1f));
        mActions[6].targetGoals.Add(new Goal("Insanity", +4f));

        mActions[7].targetGoals.Add(new Goal("Oxygen Depletion", +3f));
        mActions[7].targetGoals.Add(new Goal("Exhaustion", +2f));
        mActions[7].targetGoals.Add(new Goal("Insanity", +5f));

        mActions[8].targetGoals.Add(new Goal("Oxygen Depletion", +7f));
        mActions[8].targetGoals.Add(new Goal("Exhaustion", +6f));
        mActions[8].targetGoals.Add(new Goal("Insanity", +2f));

        mChangeOverTime = new Action("tick");
        mChangeOverTime.targetGoals.Add(new Goal("Oxygen Depletion", +2f));
        mChangeOverTime.targetGoals.Add(new Goal("Exhaustion", +4f));
        mChangeOverTime.targetGoals.Add(new Goal("Insanity", +10f));

        lowTitle.text = "1 day will pass every " + TICK_LENGTH + " seconds";
        InvokeRepeating("Tick", 0f, TICK_LENGTH);
    }

    void Tick()
    {
        foreach (Goal goal in mGoals)
        {
            goal.value += mChangeOverTime.GetGoalChange(goal);
            goal.value = Mathf.Max(goal.value, 0);
        }
        CheckForDeath(); 
        PrintGoals();
    }

    void PrintGoals()
    {
        string goalString = "";
        foreach (Goal goal in mGoals)
        {
            goalString += goal.name + ": " + goal.value + "; \n";
        }
        //goalString += "Discontentment: " + CurrentDiscontentment();
        stats.text = goalString;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Action bestAction = ChooseAction(mActions, mGoals);
            if (bestAction == null)
                return;

            action.text = "I think I will " + bestAction.name;

            foreach (Goal goal in mGoals)
            {
                goal.value += bestAction.GetGoalChange(goal);
                goal.value = Mathf.Max(goal.value, 0);
            }

            CheckForDeath();
            PrintGoals();
        }
    }

    void CheckForDeath()
    {
        Goal OxygenDepletionGoal = System.Array.Find(mGoals, g => g.name == "Oxygen Depletion");
        Goal exhaustionGoal = System.Array.Find(mGoals, g => g.name == "Exhaustion");

        if (OxygenDepletionGoal.value >= 100 && exhaustionGoal.value >= 100)
        {
            Die();
        }
    }

    void Die()
    {
        if (hasDied) return;
        hasDied = true;

        if (explosionEffect != null)
        {
            GameObject explosionInstance = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosionInstance, 2f);
        }

        Destroy(gameObject);
    }

    Action ChooseAction(Action[] actions, Goal[] goals)
    {
        Action bestAction = null;
        float bestValue = float.PositiveInfinity;
        List<Action> candidates = new List<Action>();

        Goal insanityGoal = System.Array.Find(goals, g => g.name == "Insanity");

        if (insanityGoal != null && insanityGoal.value >= 75)
        {
            List<Action> insanityActions = new List<Action>();

            foreach (Action action in actions)
            {
                float insanityChange = action.GetGoalChange(insanityGoal);
                if (insanityChange > 0)
                {
                    insanityActions.Add(action);
                }
            }

            if (insanityActions.Count > 0)
            {
                return insanityActions[Random.Range(0, insanityActions.Count)];
            }
        }

        foreach (Action action in actions)
        {
            float thisValue = Discontentment(action, goals);
            if (thisValue < bestValue)
            {
                bestValue = thisValue;
                candidates.Clear();
                candidates.Add(action);
            }
            else if (thisValue == bestValue)
            {
                candidates.Add(action);
            }
        }

        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }

        return bestAction;
    }

    float Discontentment(Action action, Goal[] goals)
    {
        float discontentment = 0f;
        foreach (Goal goal in goals)
        {
            float newValue = goal.value + action.GetGoalChange(goal);
            newValue = Mathf.Max(newValue, 0);
            discontentment += goal.GetDiscontentment(newValue);
        }
        return discontentment;
    }

    float CurrentDiscontentment()
    {
        float total = 0f;
        foreach (Goal goal in mGoals)
        {
            total += (goal.value * goal.value);
        }
        return total;
    }
}