using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Linq;

public static class PokemonDatabase
{
    private static string currentlyOpenFile;
    private static Dictionary<string, Pokemon> pkmnDatabase = new Dictionary<string, Pokemon>();

    public static void populateDatabaseFromJSONFile(string jsonFile)
    {
        if (jsonFile != currentlyOpenFile)
        {
            try
            {
                PokemonList pokemons = JsonUtility.FromJson<PokemonList>(File.ReadAllText(jsonFile));
                foreach (var pokemon in pokemons.pokemons)
                {
                    pkmnDatabase.Add(pokemon.key, pokemon); // use key field
                }
            }
            catch
            {
                Debug.LogError("Archivo " +  jsonFile + " no encontrado para poblar la base de datos Pokémon.");
            }
            
            currentlyOpenFile = jsonFile;
        }
    }

    public static Pokemon get(string key)
    {
        return pkmnDatabase[key];
    }

    public static List<Pokemon> getAllPokemons()
    {
        List<Pokemon> list = new List<Pokemon>();

        foreach (var pair in pkmnDatabase)
        {
            list.Add(pair.Value);
        }

        return list;
    }
}

[Serializable]
class PokemonList
{
    public List<Pokemon> pokemons;
}

[Serializable]
public struct Pokemon
{
    public Pokemon(string key, string name, int level, string imgFileName, float cool, float tough, float beauty, float smart, float cute)
    {
        this.key = key;
        this.name = name;
        this.level = level;
        this.imgFileName = imgFileName;
        stats = new PokemonStats(cool, tough, beauty, smart, cute);
    }
    public string key;
    public string name;
    public int level;
    public string imgFileName;
    public PokemonStats stats;

    [Serializable]
    public struct PokemonStats
    {
        public PokemonStats(float cool, float tough, float beauty, float smart, float cute)
        {
            this.cool = cool;
            this.tough = tough;
            this.beauty = beauty;
            this.smart = smart;
            this.cute = cute;
        }
        public float cool;
        public float tough;
        public float beauty;
        public float smart;
        public float cute;
    };
}

[Serializable]
public enum EPokemonStats
{
    COOL,
    TOUGH,
    BEAUTY,
    SMART,
    CUTE
}