using System.Collections.Generic;
using System.Linq;
using System;

using UnityEngine;

public class Spawner : MonoBehaviour
{
    //public List<GameObject> Enemies = new();
    public GameObject MeleeEnemyPrefab;
    public GameObject RangedEnemyPrefab;
    public GameObject BullEnemyPrefab;

    public List<Round> Rounds = new()
    // Because Unity decided I should kms, I now have to program these in manually
    {
        new Round() { Melee = 1 },
        new Round() { Ranged = 1 },
        new Round() { Bulls = 1 },
        new Round() { Melee = 1, Ranged = 1, Bulls = 1 },
        new Round() { Melee = 3 },
        new Round() { Ranged = 3 },
        new Round() { Melee = 2, Ranged = 2 },
        new Round() { Melee = 2, Ranged = 2, Bulls = 1 },
        new Round() { Melee = 3, Ranged = 3},
        new Round() { Melee = 5, Ranged = 3, Bulls = 1 },
        new Round() { Bulls = 3 },
    };
    public int RoundNumber = 0;
    public GameObject Gate1;
    public GameObject Gate2;

    public bool SkipCurrentRound = false;
    public bool AllRoundsComplete = false;

    // Stores their health for checking
    public List<EnemyAIEngine> CurrentRoundEnemies = new();

    void Start()
    {
        // Spawns the initial round
        SpawnRound();
    }

    void Update()
    {
        if (SkipCurrentRound)
        {
            print("Skipped current round");
            SkipCurrentRound = false;

            CurrentRoundEnemies.ForEach(Enemy => Enemy.Health = 0);
        }

        // Due to the mesh slicing, the references to the AI engines can become null
        if (!AllRoundsComplete && CurrentRoundEnemies.All(Enemy => Enemy.IsDead || Enemy == null))
        {
            print($"Round {RoundNumber} finished. Spawning next round.");

            SpawnRound();
        }
    }

    void SpawnRound()
    {
        try
        {
            CurrentRoundEnemies.Clear();
            Round ThisRound = Rounds[RoundNumber];
            print($"This round: Melee: {ThisRound.Melee}. Ranged: {ThisRound.Ranged}. Bulls: {ThisRound.Bulls}");
            // Not the most dynamic but it's only three
            int GateSwitch = 0;
            SpawnEnemies(ref GateSwitch, ThisRound.Melee, MeleeEnemyPrefab);
            SpawnEnemies(ref GateSwitch, ThisRound.Ranged, RangedEnemyPrefab);
            SpawnEnemies(ref GateSwitch, ThisRound.Bulls, BullEnemyPrefab);
            RoundNumber++;
        }
        catch (ArgumentOutOfRangeException)
        {
            print("Finished all rounds");
            AllRoundsComplete = true;
        }

        //Round ThisRound = Rounds.Take(1).ToArray()[0];
        
    }

    void SpawnEnemies(ref int GateSwitch, int EnemyCount, GameObject EnemyPrefab)
    {
        while (EnemyCount > 0)
        {
            GameObject GO;
            // Spawn at gate 1
            if (GateSwitch % 2 == 0)
                GO = Instantiate(EnemyPrefab, Gate1.transform.position, Quaternion.identity);
            // Spawn at gate 2
            else
                GO = Instantiate(EnemyPrefab, Gate2.transform.position, Quaternion.identity);

            GO.SetActive(true);
            CurrentRoundEnemies.Add(GO.GetComponent<EnemyAIEngine>());

            EnemyCount--;
            GateSwitch++;
        }
    }

    public struct Round
    {
        public int Melee;
        public int Ranged;
        public int Bulls;
    }
}
