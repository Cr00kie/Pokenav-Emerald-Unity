using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class SelectMenu : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] private string menuTitle = "MAIN MENU";
    [SerializeField] private Color titleBackgroundColor = new Color(0.71f, 0.68f, 0.48f);
    [SerializeField] private Color titleTextColor = Color.white;

    [Header("Background")]
    // Imagen de fondo del menú completo.
    [SerializeField] private Sprite menuBackgroundImage;

    [Header("Bottom Text Box")]
    // Texto que aparece en la caja inferior.
    [TextArea(2, 4)]
    [SerializeField] private string bottomText = "Check the map of the HOENN region.";

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
            footerLabel.text = bottomText;
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

        // Ejecutamos el evento configurado en el Inspector
        buttonData.onClick.Invoke();
    }
}

[Serializable]
public class MenuButtonData
{
    [Header("Visual")]
    public string buttonText = "New Button";
    public Color buttonColor = Color.white;

    [Header("Action")]
    public UnityEvent onClick = new UnityEvent();
}