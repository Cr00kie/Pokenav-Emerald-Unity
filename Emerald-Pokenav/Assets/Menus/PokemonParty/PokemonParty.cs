using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PokemonParty : MonoBehaviour
{
    [SerializeField]
    string menuTitle;

    [SerializeField]
    string menuSubtitle;
    [SerializeField]
    Color subtitleColor;

    [SerializeField]
    string[] pokemonDisplayedIDs;


    VisualElement root;
    VisualElement pokeballContainer;

    private void OnEnable()
    {
        populateDatabaseFromJSONFile("pokemonDatabase.json");
        
        UIDocument document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        // Set title text and subtitle color and text
        Label title = root.Q<Label>("TitleText");
        title.text = menuTitle;
        Label subtitle = root.Q<Label>("SubtitleText");
        subtitle.text = menuSubtitle;
        VisualElement subtitleContainer = root.Q<VisualElement>("Subtitle");
        subtitleContainer.style.unityBackgroundImageTintColor = subtitleColor;

        pokeballContainer = root.Q<VisualElement>("PokemonSelector");
        VisualTreeAsset pokeballButton = Resources.Load<VisualTreeAsset>("Templates/Pokeball");
        // Add pokemon data
        for (int i = 0; i < pokemonDisplayedIDs.Length; i++)
        {
            VisualElement pokeball = pokeballButton.Instantiate();

            // Set pokeball user data to be pokemon id in database
            pokeball.userData = pokemonDisplayedIDs[i];

            // Subscribe to change menu data on click
            pokeball.RegisterCallback<ClickEvent>((ClickEvent e) => {
                string pkmnID = (e.target as VisualElement).parent.userData as string;
                if(pkmnID != null) updateDataDisplayed(pkmnID);
                });

            // Add pokeball button to container
            pokeballContainer.Insert(i, pokeball);
        }

        // Set menu data for first pokemon given
        if(pokemonDisplayedIDs.Length > 0)
        {
            updateDataDisplayed(pokemonDisplayedIDs[0]);
        }
    }

    void changeSelectedPokemon(string newPokemonID)
    {
        VisualElement[] pokeballs = pokeballContainer.Children().ToArray<VisualElement>();
        // Recorremos solo el numero de pokemons que se esten mostrando (el ultimo hijo de la lista es el boton de salir)
        for(int i = 0; i < pokemonDisplayedIDs.Length; i++)
        {
            if (pokeballs[i].userData as string == newPokemonID)
            {
                pokeballs[i].Children().ToList()[0].EnableInClassList("pokeball-active", true);
                //pokeballs[i].style.opacity = 1.0f;
            }
            else
            {
                pokeballs[i].Children().ToList()[0].EnableInClassList("pokeball-active", false);
                //pokeballs[i].style.opacity = 0.5f;
            }
        }
    }

    void updateDataDisplayed(string newPokemonID)
    {
        // Actualizamos el nombre y nivel del pokemon
        Label name = root.Q<Label>("PokemonName");
        Label level = root.Q<Label>("PokemonLevel");

        name.text = pkmnDatabase[newPokemonID].name;
        level.text = "Lv " + pkmnDatabase[newPokemonID].level;

        // Actualizamos la imagen del pokemon
        VisualElement image = root.Q<VisualElement>("PokemonImage");
        Sprite sprite = Resources.Load<Sprite>("PokemonIMG/" + pkmnDatabase[newPokemonID].imgFileName);
        image.style.backgroundImage = new StyleBackground(sprite);

        // Actualizamos el radar chart con las nuevas stats
        RadarChart chart = root.Q<RadarChart>("RadarChart");
        chart.SetStats(
            pkmnDatabase[newPokemonID].stats.cool,
            pkmnDatabase[newPokemonID].stats.beauty,
            pkmnDatabase[newPokemonID].stats.cute,
            pkmnDatabase[newPokemonID].stats.smart,
            pkmnDatabase[newPokemonID].stats.tough
            );

        // Cambiamos el pokemon seleccionado visualmente
        changeSelectedPokemon(newPokemonID);
    }

    void populateDatabaseFromJSONFile(string jsonFile)
    {
        pkmnDatabase = new Dictionary<string, Pokemon>();

        PokemonList pokemons = JsonUtility.FromJson<PokemonList>(File.ReadAllText(jsonFile));
        foreach (var pokemon in pokemons.pokemons)
        {
            pkmnDatabase.Add(pokemon.key, pokemon); // use key field
        }
    }

    Dictionary<string, Pokemon> pkmnDatabase;
}

[Serializable] class PokemonList
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
    public struct PokemonStats{
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
