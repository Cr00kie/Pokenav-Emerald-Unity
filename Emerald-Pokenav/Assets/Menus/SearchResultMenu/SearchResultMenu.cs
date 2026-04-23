using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SearchResultMenu : MonoBehaviour
{
    [Header("Default Subtitle")]
    [SerializeField] private string defaultSubtitleText = "Party Pok¨¦mon";
    [SerializeField] private Color defaultSubtitleColor = Color.white;

    private VisualElement root;
    private VisualElement subtitleContainer;
    private Label subtitleLabel;
    private ListView searchResultList;

    public static EPokemonStats statUsedToFilterSearch;

    private void OnEnable()
    {
        // Intentar pillar los datos del JSON si todavía no los han pillado
        PokemonDatabase.populateDatabaseFromJSONFile("pokemonDatabase.json");

        UIDocument document = GetComponent<UIDocument>();

        if (document == null)
        {
            Debug.LogError("No se encontr¨® un UIDocument en este GameObject.");
            return;
        }

        root = document.rootVisualElement;

        // Buscamos el contenedor del subtitle
        subtitleContainer = root.Q<VisualElement>("Subtitle");

        // Buscamos el label del subtitle
        subtitleLabel = root.Q<Label>("SubtitleText");

        if (subtitleContainer == null)
        {
            Debug.LogError("No se encontr¨® el VisualElement 'Subtitle'.");
            return;
        }

        if (subtitleLabel == null)
        {
            Debug.LogError("No se encontr¨® el VisualElement 'SubtitleText'.");
            return;
        }


        // Buscamos la lista donde pondremos el resultado de la búsqueda
        searchResultList = root.Q<ListView>("SearchResultList");
        if (searchResultList == null)
        {
            Debug.LogError("No se encontró el ListView 'SearchResultList'.");
            return;
        }

        ApplySubtitleData();
        AddElementsToListView();
    }

    private void AddElementsToListView()
    {
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("Templates/SearchResultItem");
        List<Pokemon> pokemons = PokemonDatabase.getAllPokemons().OrderByDescending(GetOrderingLambda()).ToList();

        searchResultList.itemsSource = pokemons;

        searchResultList.makeItem = () => template.Instantiate();

        searchResultList.bindItem = (element, index) =>
        {
            Pokemon p = pokemons[index];
            element.Q<Label>("Col1").text = p.name;
            element.Q<Label>("Col2").text = "Lv " + p.level;
            element.Q<VisualElement>("ItemSelectedImage").style.visibility = Visibility.Hidden;

            element.RegisterCallback<MouseEnterEvent>(e =>
            {
                element.Q<VisualElement>("ItemSelectedImage").style.visibility = Visibility.Visible;
            });

            element.RegisterCallback<MouseLeaveEvent>(e =>
            {
                element.Q<VisualElement>("ItemSelectedImage").style.visibility = Visibility.Hidden;
            });

            element.RegisterCallback<ClickEvent>(e =>
            {
                PokemonPartyMenu.SetPartyPokemon(new string[] { p.key });
                // Cambiamos a la escena con los pokemon y estadísticas
                SceneManager.LoadScene(0);
            });
        };
    }

    private static Func<Pokemon, float> GetOrderingLambda()
    {
        switch (statUsedToFilterSearch)
        {
            case EPokemonStats.COOL: return p => p.stats.cool;
            case EPokemonStats.TOUGH: return p => p.stats.tough;
            case EPokemonStats.BEAUTY: return p => p.stats.beauty;
            case EPokemonStats.SMART: return p => p.stats.smart;
            case EPokemonStats.CUTE: return p => p.stats.cute;
            default: return p => p.level;
        }
    }

    private void ApplySubtitleData()
    {
        // Si venimos desde SelectMenu con datos guardados, usamos esos
        if (MenuNavigationData.HasPendingData)
        {
            subtitleLabel.text = MenuNavigationData.NextSubtitleText;
            subtitleContainer.style.unityBackgroundImageTintColor =
                new StyleColor(MenuNavigationData.NextSubtitleColor);

            // Limpiamos los datos para que no se arrastren a otras escenas
            MenuNavigationData.Clear();
        }
        else
        {
            // Si no hay datos, usamos los valores por defecto
            subtitleLabel.text = defaultSubtitleText;
            subtitleContainer.style.unityBackgroundImageTintColor =
                new StyleColor(defaultSubtitleColor);
        }
    }
}