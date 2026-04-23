using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Linq;

public static class TrainersDatabase
{
    private static string currentlyOpenFile;
    private static Dictionary<string, Trainer> trainerDatabase = new Dictionary<string, Trainer>();

    public static void populateDatabaseFromJSONFile(string jsonFile)
    {
        if (jsonFile != currentlyOpenFile)
        {
            try
            {
                TrainerList trainers = JsonUtility.FromJson<TrainerList>(File.ReadAllText(jsonFile));
                foreach (var trainer in trainers.trainers)
                {
                    trainerDatabase.Add(trainer.key, trainer); // use key field
                }
            }
            catch
            {
                Debug.LogError("Archivo " +  jsonFile + " no encontrado para poblar la base de datos Pokémon.");
            }
            
            currentlyOpenFile = jsonFile;
        }
    }

    public static Trainer get(string key)
    {
        return trainerDatabase[key];
    }

    public static List<Trainer> getAllTrainers()
    {
        List<Trainer> list = new List<Trainer>();

        foreach (var pair in trainerDatabase)
        {
            list.Add(pair.Value);
        }

        return list;
    }
}

[Serializable]
class TrainerList
{
    public List<Trainer> trainers;
}

[Serializable]
public struct Trainer
{
    public Trainer(string key, string name, string tag, string strategy, string pokemon, string selfintro, string imgFileName, string location)
    {
        this.key = key;
        this.name = name;
        this.tag = tag;
        this.strategy = strategy;
        this.pokemon = pokemon;
        this.selfintro = selfintro;
        this.imgFileName = imgFileName;
        this.location = location;
    }
    public string key;
    public string name;
    public string tag;
    public string strategy;
    public string pokemon;
    public string selfintro;
    public string imgFileName;
    public string location;
}