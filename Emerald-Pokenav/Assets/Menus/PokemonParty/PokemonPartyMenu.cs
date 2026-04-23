using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.IO;
using DG.Tweening;

public class PokemonPartyMenu : MonoBehaviour
{
    [SerializeField]
    string menuTitle;

    [SerializeField]
    string menuSubtitle;
    [SerializeField]
    Color subtitleColor;
    [Serializable]
    public class PokemonParty
    {
        public string[] ids;
    }
    private static PokemonParty pokemonParty = new PokemonParty();

    VisualElement root;
    VisualElement pokeballContainer;
    

    private void OnEnable()
    {
        // Intentar pillar los datos del JSON si todavía no los han pillado
        PokemonDatabase.populateDatabaseFromJSONFile("pokemonDatabase.json");
        
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
        for (int i = 0; i < pokemonParty.ids.Length; i++)
        {
            VisualElement pokeball = pokeballButton.Instantiate();

            // Set pokeball user data to be pokemon id in database
            pokeball.userData = pokemonParty.ids[i];

            // Subscribe to change menu data on click
            pokeball.RegisterCallback<ClickEvent>((ClickEvent e) => {
                string pkmnID = (e.target as VisualElement).parent.userData as string;
                if (pkmnID != null) updateDataDisplayed(pkmnID);
            });

            // Add pokeball button to container
            pokeballContainer.Insert(i, pokeball);

            pokeball.style.translate = new StyleTranslate(new Translate(500f, 0f));
            DOVirtual.Float(500f, 0f, 0.5f, val =>
            {
                pokeball.style.translate = new StyleTranslate(new Translate(val, 0f));
            }).SetDelay(i*0.1f);
        }

        // Set menu data for first pokemon given
        if(pokemonParty.ids.Length > 0)
        {
            updateDataDisplayed(pokemonParty.ids[0]);
        }

        VisualElement exitButton = root.Q<VisualElement>("Exit");
        exitButton.RegisterCallback<ClickEvent>((ClickEvent e) =>
        {
            SceneManager.LoadScene("SelectMenuScene");
        });
    }

    public static void SetPartyPokemon(string[] party)
    {
        pokemonParty.ids = party;
    }

    public static void SetPartyPokemonFromFile(string file)
    {
        pokemonParty = JsonUtility.FromJson<PokemonParty>(File.ReadAllText(file));
    }

    void changeSelectedPokemon(string newPokemonID)
    {
        VisualElement[] pokeballs = pokeballContainer.Children().ToArray<VisualElement>();
        // Recorremos solo el numero de pokemons que se esten mostrando (el ultimo hijo de la lista es el boton de salir)
        for(int i = 0; i < pokemonParty.ids.Length; i++)
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

        name.text = PokemonDatabase.get(newPokemonID).name;
        level.text = "Lv " + PokemonDatabase.get(newPokemonID).level;

        // Actualizamos la imagen del pokemon
        VisualElement image = root.Q<VisualElement>("PokemonImage");
        Sprite sprite = Resources.Load<Sprite>("PokemonIMG/" + PokemonDatabase.get(newPokemonID).imgFileName);
        image.style.backgroundImage = new StyleBackground(sprite);
        DOVirtual.Float(-300, 0f, 0.8f, val =>
        {
            image.style.translate = new StyleTranslate(new Translate(val, 0f));
        }).SetEase(Ease.OutCubic);

        // Actualizamos el radar chart con las nuevas stats
        RadarChart chart = root.Q<RadarChart>("RadarChart");
        chart.SetStats(
            PokemonDatabase.get(newPokemonID).stats.cool,
            PokemonDatabase.get(newPokemonID).stats.beauty,
            PokemonDatabase.get(newPokemonID).stats.cute,
            PokemonDatabase.get(newPokemonID).stats.smart,
            PokemonDatabase.get(newPokemonID).stats.tough
            );

        // Cambiamos el pokemon seleccionado visualmente
        changeSelectedPokemon(newPokemonID);
    }

    private void OnDisable()
    {
       
    }
}


