using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SelectMenu : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] private string menuTitle = "MAIN MENU";
    [SerializeField] private Color titleBackgroundColor = new Color(0.71f, 0.68f, 0.48f);
    [SerializeField] private Color titleTextColor = Color.white;

    [Header("Background")]
    // Imagen de fondo del menú completo.
    [SerializeField] private Sprite menuBackgroundImage;

    [Header("Footer")]
    // Texto por defecto del footer.
    // Este texto vuelve cuando el ratón deja de estar sobre un botón.
    [TextArea(2, 4)]
    [SerializeField] private string defaultBottomText = "Select a button to show its function";

    [Header("Buttons")]
    // Template UXML para cada botón.
    [SerializeField] private VisualTreeAsset buttonTemplate;

    // Lista de botones
    [SerializeField] private List<MenuButtonData> buttons = new List<MenuButtonData>();

    // Referencias a elementos del UXML principal
    private VisualElement root;
    private VisualElement baseContainer;
    private VisualElement titleContainer;
    private VisualElement buttonsContainer;
    private VisualElement footerContainer;

    private Label titleLabel;
    private Label footerLabel;

    private void OnEnable()
    {
        UIDocument document = GetComponent<UIDocument>();

        if (document == null)
        {
            Debug.LogError("No se encontró un UIDocument en este GameObject.");
            return;
        }

        root = document.rootVisualElement;

        // Buscamos y guardamos referencias
        CacheReferences();

        // Aplicamos datos configurados desde el Inspector a los elementos del menú
        ApplyMenuData();

        // Creamos los botones 
        BuildButtons();
    }

    // Método para buscar y guardar referencias a los elementos del UXML
    private void CacheReferences()
    {

        baseContainer = root.Q<VisualElement>("Base");

        titleContainer = root.Q<VisualElement>("Title");

        buttonsContainer = root.Q<VisualElement>("Buttons");

        footerContainer = root.Q<VisualElement>("Footer");

        // buscamos el primer Label dentro de cada contenedor.
        if (titleContainer != null)
        {
            titleLabel = titleContainer.Q<Label>();
        }

        if (footerContainer != null)
        {
            footerLabel = footerContainer.Q<Label>();
        }

        if (baseContainer == null)
        {
            Debug.LogError("No se encontró el VisualElement 'Base'.");
        }

        if (titleContainer == null)
        {
            Debug.LogError("No se encontró el VisualElement 'Title'.");
        }

        if (buttonsContainer == null)
        {
            Debug.LogError("No se encontró el VisualElement 'Buttons'.");
        }

        if (footerContainer == null)
        {
            Debug.LogError("No se encontró el VisualElement 'Footer'.");
        }

        if (titleLabel == null)
        {
            Debug.LogError("No se encontró el Label dentro de 'Title'.");
        }

        if (footerLabel == null)
        {
            Debug.LogError("No se encontró el Label dentro de 'Footer'.");
        }
    }

    private void ApplyMenuData()
    {
        // Aplicamos la imagen de fondo del menú si se ha asignado desde el Inspector
        if (baseContainer != null && menuBackgroundImage != null)
        {
            baseContainer.style.backgroundImage = new StyleBackground(menuBackgroundImage);
        }

        // Aplicamos el texto y colores del título
        if (titleContainer != null && titleLabel != null)
        {
            titleContainer.style.unityBackgroundImageTintColor = new StyleColor(titleBackgroundColor);
            titleLabel.text = menuTitle;
            titleLabel.style.color = new StyleColor(titleTextColor);
        }

        // Aplicamos el texto de la caja inferior
        if (footerLabel != null)
        {
            footerLabel.text = defaultBottomText;
        }
    }

    private void BuildButtons()
    {
        if (buttonsContainer == null)
        {
            return;
        }

        if (buttonTemplate == null)
        {
            Debug.LogError("No has asignado un Button Template en el Inspector.");
            return;
        }

        // Borramos los hijos que habia anteriormente en el contenedor
        buttonsContainer.Clear();

        for (int i = 0; i < buttons.Count; i++)
        {
            MenuButtonData buttonData = buttons[i];

            // Instanciamos el template del botón
            TemplateContainer buttonInstance = buttonTemplate.Instantiate();

            // Buscamos la raíz del botón y su label dentro del template
            VisualElement buttonRoot = buttonInstance.Q<VisualElement>("ButtonRoot");
            Label buttonLabel = buttonInstance.Q<Label>("ButtonLabel");

            if (buttonRoot == null)
            {
                Debug.LogError("No se encontró 'ButtonRoot' dentro del template del botón.");
                continue;
            }

            if (buttonLabel == null)
            {
                Debug.LogError("No se encontró 'ButtonLabel' dentro del template del botón.");
                continue;
            }

            // Guardamos la data del botón 
            buttonRoot.userData = buttonData;

            // Aplicamos el texto del botón
            buttonLabel.text = buttonData.buttonText;

            // Aplicamos el color
            buttonRoot.style.unityBackgroundImageTintColor = new StyleColor(buttonData.buttonColor);

            // Registramos el click
            buttonRoot.RegisterCallback<ClickEvent>(OnMenuButtonClicked);
            buttonRoot.RegisterCallback<PointerEnterEvent>(OnMenuButtonPointerEnter);
            buttonRoot.RegisterCallback<PointerLeaveEvent>(OnMenuButtonPointerLeave);

            // Añadimos el botón instanciado al contenedor
            buttonsContainer.Add(buttonInstance);
        }

        //  Refrescamos callbacks para hover
        MenuButtonHoverBlink hoverBlink = GetComponent<MenuButtonHoverBlink>();
        if (hoverBlink != null)
        {
            hoverBlink.RefreshButtons();
        }
    }

    private void OnMenuButtonClicked(ClickEvent evt)
    {
        VisualElement buttonRoot = evt.currentTarget as VisualElement;

        if (buttonRoot == null)
        {
            return;
        }

        MenuButtonData buttonData = buttonRoot.userData as MenuButtonData;

        if (buttonData == null)
        {
            return;
        }

        // Cuando se hace click en el botón, guardamos su texto y color en MenuNavigationData
        MenuNavigationData.SetSubtitleData(
            buttonData.buttonText,
            buttonData.buttonColor
        );

        // Ejecutamos el evento asociado
        buttonData.onClick.Invoke();

        // Si el botón tiene una escena asignada, la cargamos.
        if (!string.IsNullOrEmpty(buttonData.sceneToLoad))
        {
            SceneManager.LoadScene(buttonData.sceneToLoad);
            return;
        }

    }


    private void OnMenuButtonPointerEnter(PointerEnterEvent evt)
    {
        VisualElement buttonRoot = evt.currentTarget as VisualElement;

        if (buttonRoot == null)
        {
            return;
        }

        MenuButtonData buttonData = buttonRoot.userData as MenuButtonData;

        if (buttonData == null)
        {
            return;
        }

        if (footerLabel == null)
        {
            return;
        }

        // Cuando el ratón entra en el botón,
        // cambiamos el texto del footer al texto de hover de ese botón.
        footerLabel.text = buttonData.footerHoverText;
    }

    private void OnMenuButtonPointerLeave(PointerLeaveEvent evt)
    {
        if (footerLabel == null)
        {
            return;
        }

        // Cuando el ratón sale del botón,
        // restauramos el texto por defecto del footer.
        footerLabel.text = defaultBottomText;
    }

    // Lo usamos en el menu de condition cuando el usuario presiona el boton de Party Pokemon
    public void SetPokemonPartyFromFile()
    {
        PokemonPartyMenu.SetPartyPokemonFromFile("partyPokemon.json");
    }
}

[Serializable]
public class MenuButtonData
{
    [Header("Visual")]
    public string buttonText = "New Button";
    public Color buttonColor = Color.white;

    [Header("Footer")]
    [TextArea(2, 4)]
    public string footerHoverText = "Button description";

    [Header("Scene")]
    //Al pulsar el botón se cargará esta escena.
    public string sceneToLoad = "";

    [Header("Action")]
    public UnityEvent onClick = new UnityEvent();
}