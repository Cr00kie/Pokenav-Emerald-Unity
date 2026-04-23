using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MatchCallMenu : MonoBehaviour
{
    private VisualElement root;
    private Label locationLabel;
    private ListView searchResultList;
    private VisualElement trainerDetails;
    private VisualElement trainerImage;
    private VisualElement matchCallData;

    // Trainer details elements
    private Label trainerName;
    private Label trainerTag;
    private Label trainerStrategy;
    private Label trainerPokemon;
    private Label trainerSelfintro;
    private Tween arrowTween;
    private VisualElement selectedTrainerArrow = null;

    private VisualElement titleDetailsBlock;
    private VisualElement strategyBlock;
    private VisualElement pokemonBlock;
    private VisualElement selfintroBlock;

    [SerializeField]
    private int noRegistered;
    [SerializeField]
    private int noBattles;

    private void OnEnable()
    {
        // Intentar pillar los datos del JSON si todavía no los han pillado
        TrainersDatabase.populateDatabaseFromJSONFile("trainersDatabase.json");

        UIDocument document = GetComponent<UIDocument>();

        root = document.rootVisualElement;

        // Buscamos el label del subtitle
        locationLabel = root.Q<Label>("LocationText");


        // Buscamos la lista donde pondremos el resultado de la búsqueda
        searchResultList = root.Q<ListView>("SearchResultList");

        trainerDetails = root.Q<VisualElement>("TrainerDetails");
        trainerImage = root.Q<VisualElement>("TrainerImage");
        matchCallData = root.Q<VisualElement>("MatchCallData");

        trainerTag = root.Q<Label>("DetailsTitleTagText");
        trainerName = root.Q<Label>("DetailsTitleNameText");
        trainerStrategy = root.Q<Label>("StrategyText");
        trainerPokemon = root.Q<Label>("PokemonText");
        trainerSelfintro = root.Q<Label>("SelfIntroductionText");

        titleDetailsBlock = root.Q<VisualElement>("DetailsTitle");
        strategyBlock = root.Q<VisualElement>("Strategy");
        pokemonBlock = root.Q<VisualElement>("Pokemon");
        selfintroBlock = root.Q<VisualElement>("SelfIntroduction");

        AddElementsToListView();

        DOVirtual.Float(0f, 10f, 0.5f, val =>
        {
            if (selectedTrainerArrow != null) selectedTrainerArrow.style.translate = new StyleTranslate(new Translate(val, 0f));
        })
        .SetEase(Ease.InOutCubic)
        .SetLoops(-1, LoopType.Yoyo);

        Label noRegisteredText = root.Q<Label>("NoRegisteredData");
        Label noBattlesText = root.Q<Label>("NoBattlesData");

        DOVirtual.Int(0, noRegistered, 0.5f, val => { noRegisteredText.text = val.ToString(); });
        DOVirtual.Int(0, noBattles, 0.5f, val => { noBattlesText.text = val.ToString(); });
    }

    private void OnDisable()
    {
        arrowTween?.Kill();
    }

    private void showTrainerDetails(bool showDetails)
    {
        searchResultList.style.display = showDetails ? DisplayStyle.None: DisplayStyle.Flex;
        matchCallData.style.display = showDetails ? DisplayStyle.None : DisplayStyle.Flex;
        trainerDetails.style.display = showDetails ? DisplayStyle.Flex : DisplayStyle.None;
        trainerImage.style.display = showDetails ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void AddElementsToListView()
    {
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("Templates/SearchResultItem");
        List<Trainer> trainers = TrainersDatabase.getAllTrainers();

        searchResultList.itemsSource = trainers;

        searchResultList.makeItem = () => template.Instantiate();

        searchResultList.bindItem = (element, index) =>
        {
            Trainer t = trainers[index];
            element.Q<Label>("Col1").text = t.tag;
            element.Q<Label>("Col2").text = t.name;
            VisualElement arrow = element.Q<VisualElement>("ItemSelectedImage");
            arrow.style.visibility = Visibility.Hidden;

            element.RegisterCallback<MouseEnterEvent>(e =>
            {
                element.Q<VisualElement>("ItemSelectedImage").style.visibility = Visibility.Visible;
                locationLabel.text = t.location;
                selectedTrainerArrow = arrow;
            });

            element.RegisterCallback<MouseLeaveEvent>(e =>
            {
                element.Q<VisualElement>("ItemSelectedImage").style.visibility = Visibility.Hidden;
                selectedTrainerArrow = null;
            });

            element.RegisterCallback<ClickEvent>(e =>
            {
                locationLabel.text = t.location;
                trainerName.text = t.name;
                trainerTag.text = t.tag;
                trainerStrategy.text = t.strategy;
                trainerPokemon.text = t.pokemon;
                trainerSelfintro.text = t.selfintro;
                Sprite trainerSprite = Resources.Load<Sprite>("TrainerIMG/" + t.imgFileName);
                trainerImage.style.backgroundImage = new StyleBackground(trainerSprite);
                showTrainerDetails(true);

                DOVirtual.Float(-500f, 0f, 0.5f, val =>
                {
                    trainerImage.style.translate = new StyleTranslate(new Translate(val, 0f));
                });

                titleDetailsBlock.style.translate = new StyleTranslate(new Translate(0f, -1000f));
                DOVirtual.Float(-1000f, 0f, 0.5f, val =>
                {
                    titleDetailsBlock.style.translate = new StyleTranslate(new Translate(0f, val));
                });
                strategyBlock.style.translate = new StyleTranslate(new Translate(0f, -1000f));
                DOVirtual.Float(-1000f, 0f, 0.5f, val =>
                {
                    strategyBlock.style.translate = new StyleTranslate(new Translate(0f, val));
                }).SetDelay(0.1f);
                pokemonBlock.style.translate = new StyleTranslate(new Translate(0f, -1000f));
                DOVirtual.Float(-1000f, 0f, 0.5f, val =>
                {
                    pokemonBlock.style.translate = new StyleTranslate(new Translate(0f, val));
                }).SetDelay(0.2f);
                selfintroBlock.style.translate = new StyleTranslate(new Translate(0f, -1000f));
                DOVirtual.Float(-1000f, 0f, 0.5f, val =>
                {
                    selfintroBlock.style.translate = new StyleTranslate(new Translate(0f, val));
                }).SetDelay(0.3f);
            });

            element.style.translate = new StyleTranslate(new Translate(1000f, 0f));
            DOVirtual.Float(1000f, 0f, 0.5f, val =>
            {
                element.style.translate = new StyleTranslate(new Translate(val, 0f));
            }).SetDelay(index * 0.05f);
        };
    }
}