    using System.Collections;
using System.Collections.Generic;
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

        pkmnDatabase = new Dictionary<string, Pokemon>();
        populateDatabase();

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

    // Crea una base de datos para testear
    void populateDatabase()
    {
        pkmnDatabase.Add("rayquaza", new Pokemon("Rayquaza", 75, "rayquaza", 0.7f, 0.10f, 0.2f, 0.6f, 0.8f));
        pkmnDatabase.Add("pidgeot", new Pokemon("Pidgeot", 25, "pidgeot", 0.3f, 0.12f, 0.3f, 0.4f, 0.1f));
        pkmnDatabase.Add("gligar", new Pokemon("Gligar", 23, "gligar", 0.2f, 0.6f, 0.8f, 0.1f, 0.5f));
        pkmnDatabase.Add("treecko", new Pokemon("Treecko", 34, "treecko", 0.8f, 0.11f, 0.1f, 0.4f, 0.8f));
        pkmnDatabase.Add("poke4", new Pokemon("Pokemon 4", 76, "", 0.1f, 0.13f, 0.5f, 0.3f, 0.5f));
        pkmnDatabase.Add("poke5", new Pokemon("Pokemon 5", 12, "", 0.4f, 0.2f, 0.8f, 0.7f, 0.4f));
        pkmnDatabase.Add("poke6", new Pokemon("Pokemon 6", 45, "", 0.2f, 0.6f, 0.1f, 0.1f, 0.3f));
        pkmnDatabase.Add("poke7", new Pokemon("Pokemon 7", 23, "", 0.9f, 0.10f, 0.2f, 0.4f, 0.9f));
    }

    Dictionary<string, Pokemon> pkmnDatabase;
}

public struct Pokemon
{
    public Pokemon(string name, int level, string imgFileName, float cool, float tough, float beauty, float smart, float cute)
    {
        this.name = name;
        this.level = level;
        this.imgFileName = imgFileName;
        stats = new PokemonStats(cool, tough, beauty, smart, cute);
    }
    public string name;
    public int level;
    public string imgFileName;
    public PokemonStats stats;

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
