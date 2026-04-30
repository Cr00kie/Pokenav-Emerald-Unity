using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class HoennMapMenu : MonoBehaviour
{
    // referencia al uidocument que contiene todo el arbol visual del menu
    private UIDocument uiDocument;

    // referencias a los elementos principales del uxml
    private VisualElement root;
    private VisualElement baseContainer;
    private VisualElement mapViewport;
    private VisualElement mapContent;
    private VisualElement villagesContainer;
    private VisualElement locationInfoContainer;
    private VisualElement rightTitles;
    private VisualElement returnButton;

    // label donde se muestra el nombre del pueblo seleccionado o en hover
    private Label villageLabel;

    // guarda el nombre del pueblo actualmente seleccionado con click
    private string selectedVillageName = string.Empty;

    [Header("Map Zoom")]
    // nivel de zoom que se aplica al mapa cuando se selecciona un pueblo
    [SerializeField] private float selectedZoom = 1.5f;

    [Header("Scenes")]
    // nombre de la escena a la que vuelve el boton return
    [SerializeField] private string returnSceneName = "MainMenuScene";

    private void OnEnable()
    {
        // obtenemos el uidocument del mismo gameobject
        uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("No se encontro un UIDocument en este GameObject.");
            return;
        }

        // guardamos la raiz del arbol visual
        root = uiDocument.rootVisualElement;

        // buscamos referencias a todos los elementos del uxml
        CacheReferences();

        // configuramos que elementos pueden bloquear o recibir input
        ConfigurePicking();

        // configuramos transiciones suaves para el panel de info y el zoom del mapa
        ConfigureTransitions();

        // registramos los eventos de click y hover
        RegisterCallbacks();

        // nos aseguramos de que los paneles de la derecha queden delante visualmente
        BringPanelsToFront();

        // al entrar en la escena, ocultamos la ficha de informacion
        HideLocationInfo();

        // y dejamos el mapa sin zoom
        ResetMapZoom();
    }

    private void OnDisable()
    {
        if (baseContainer != null)
        {
            baseContainer.UnregisterCallback<ClickEvent>(OnBaseClicked);
        }

        if (returnButton != null)
        {
            returnButton.UnregisterCallback<ClickEvent>(OnReturnClicked);
        }

        if (villagesContainer != null)
        {
            foreach (VisualElement village in villagesContainer.Children())
            {
                village.UnregisterCallback<ClickEvent>(OnVillageClicked);
                village.UnregisterCallback<PointerEnterEvent>(OnVillagePointerEnter);
                village.UnregisterCallback<PointerLeaveEvent>(OnVillagePointerLeave);
            }
        }
    }

    private void CacheReferences()
    {
        // buscamos cada elemento por su name 
        baseContainer = root.Q<VisualElement>("Base");
        mapViewport = root.Q<VisualElement>("MapViewport");
        mapContent = root.Q<VisualElement>("MapContent");
        villagesContainer = root.Q<VisualElement>("Villages");
        locationInfoContainer = root.Q<VisualElement>("LocationInfo");
        rightTitles = root.Q<VisualElement>("RightTitles");
        returnButton = root.Q<VisualElement>("ReturnButton");
        villageLabel = root.Q<Label>("village");

        // comprobaciones
        if (baseContainer == null) Debug.LogError("No se encontro 'Base'.");
        if (mapViewport == null) Debug.LogError("No se encontro 'MapViewport'.");
        if (mapContent == null) Debug.LogError("No se encontro 'MapContent'.");
        if (villagesContainer == null) Debug.LogError("No se encontro 'Villages'.");
        if (locationInfoContainer == null) Debug.LogError("No se encontro 'LocationInfo'.");
        if (rightTitles == null) Debug.LogError("No se encontro 'RightTitles'.");
        if (returnButton == null) Debug.LogError("No se encontro 'ReturnButton'.");
        if (villageLabel == null) Debug.LogError("No se encontro el Label 'village'.");
    }

    private void ConfigurePicking()
    {
        // el contenedor grande de villages no debe bloquear clicks por si mismo
        // solo deben recibir clicks sus hijos individuales
        if (villagesContainer != null)
        {
            villagesContainer.pickingMode = PickingMode.Ignore;
        }

        // locationinfo es solo visual, asi que tampoco debe bloquear input
        if (locationInfoContainer != null)
        {
            locationInfoContainer.pickingMode = PickingMode.Ignore;
        }

        // el label de texto tampoco necesita input
        if (villageLabel != null)
        {
            villageLabel.pickingMode = PickingMode.Ignore;
        }

        // el panel derecho como bloque tampoco debe bloquear
        if (rightTitles != null)
        {
            rightTitles.pickingMode = PickingMode.Ignore;
        }

        // el boton return si debe recibir clicks
        if (returnButton != null)
        {
            returnButton.pickingMode = PickingMode.Position;
        }
    }

    private void ConfigureTransitions()
    {
        // configuramos una transicion suave para el translate de locationinfo
        if (locationInfoContainer != null)
        {
            locationInfoContainer.style.transitionProperty = new StyleList<StylePropertyName>(
                new List<StylePropertyName>
                {
                    new StylePropertyName("translate")
                }
            );

            locationInfoContainer.style.transitionDuration = new StyleList<TimeValue>(
                new List<TimeValue>
                {
                    new TimeValue(0.18f, TimeUnit.Second)
                }
            );
        }

        // configuramos una transicion suave para el desplazamiento y el zoom del mapa
        if (mapContent != null)
        {
            mapContent.style.transitionProperty = new StyleList<StylePropertyName>(
                new List<StylePropertyName>
                {
                    new StylePropertyName("translate"),
                    new StylePropertyName("scale")
                }
            );

            mapContent.style.transitionDuration = new StyleList<TimeValue>(
                new List<TimeValue>
                {
                    new TimeValue(0.22f, TimeUnit.Second),
                    new TimeValue(0.22f, TimeUnit.Second)
                }
            );
        }
    }

    private void RegisterCallbacks()
    {
        // si se hace click en la base, se cierra la ficha y se resetea el zoom
        if (baseContainer != null)
        {
            baseContainer.RegisterCallback<ClickEvent>(OnBaseClicked);
        }

        // el boton return vuelve a otra escena
        if (returnButton != null)
        {
            returnButton.RegisterCallback<ClickEvent>(OnReturnClicked);
        }

        if (villagesContainer == null)
        {
            return;
        }

        // cada pueblo recibe click y hover por separado
        foreach (VisualElement village in villagesContainer.Children())
        {
            village.pickingMode = PickingMode.Position;
            village.RegisterCallback<ClickEvent>(OnVillageClicked);
            village.RegisterCallback<PointerEnterEvent>(OnVillagePointerEnter);
            village.RegisterCallback<PointerLeaveEvent>(OnVillagePointerLeave);
        }
    }

    private void BringPanelsToFront()
    {
        // traemos el panel derecho y el boton return al frente
        // para que no queden por debajo de otras capas visuales
        if (rightTitles != null)
        {
            rightTitles.BringToFront();
        }

        if (returnButton != null)
        {
            returnButton.BringToFront();
        }
    }

    private void OnVillageClicked(ClickEvent evt)
    {
        // recuperamos el village que ha sido pulsado
        VisualElement clickedVillage = evt.currentTarget as VisualElement;

        if (clickedVillage == null)
        {
            return;
        }

        // evitamos que el click siga subiendo hasta base
        evt.StopPropagation();

        // guardamos el nombre del pueblo seleccionado
        selectedVillageName = FormatVillageName(clickedVillage.name);

        // actualizamos el texto visible
        villageLabel.text = selectedVillageName;

        // mostramos la ficha de informacion
        ShowLocationInfo();

        // acercamos el mapa al pueblo seleccionado
        ZoomToVillageCenter(clickedVillage);
    }

    private void OnVillagePointerEnter(PointerEnterEvent evt)
    {
        // cuando el raton entra en un pueblo, mostramos su nombre en la ficha
        // pero no cambiamos ni el zoom ni la seleccion actual
        VisualElement hoveredVillage = evt.currentTarget as VisualElement;

        if (hoveredVillage == null || villageLabel == null)
        {
            return;
        }

        villageLabel.text = FormatVillageName(hoveredVillage.name);
    }

    private void OnVillagePointerLeave(PointerLeaveEvent evt)
    {
        // cuando el raton sale del pueblo, restauramos el texto:
        // si habia un pueblo seleccionado, vuelve ese nombre
        // si no habia ninguno, dejamos el texto vacio
        if (villageLabel == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(selectedVillageName))
        {
            villageLabel.text = selectedVillageName;
        }
        else
        {
            villageLabel.text = string.Empty;
        }
    }

    private void OnBaseClicked(ClickEvent evt)
    {
        // click en cualquier parte libre del fondo:
        // quitamos seleccion, ocultamos la ficha y reseteamos el zoom
        selectedVillageName = string.Empty;
        HideLocationInfo();
        ResetMapZoom();
    }

    private void OnReturnClicked(ClickEvent evt)
    {
        // evitamos que el click del boton se propague a la base
        evt.StopPropagation();

        // si se ha configurado un nombre de escena, cambiamos a ella
        if (!string.IsNullOrEmpty(returnSceneName))
        {
            SceneManager.LoadScene(returnSceneName);
        }
    }

    private void ShowLocationInfo()
    {
        if (locationInfoContainer == null)
        {
            return;
        }

        // movemos la ficha a su posicion visible
        // solo tocamos translate, como querias
        locationInfoContainer.style.translate = new Translate(
            new Length(0, LengthUnit.Pixel),
            new Length(0, LengthUnit.Percent)
        );
    }

    private void HideLocationInfo()
    {
        if (locationInfoContainer == null || villageLabel == null)
        {
            return;
        }

        // vaciamos el texto de la ficha al ocultarla
        villageLabel.text = string.Empty;

        // movemos la ficha hacia abajo para esconderla
        // solo tocamos translate
        locationInfoContainer.style.translate = new Translate(
            new Length(0, LengthUnit.Pixel),
            new Length(85, LengthUnit.Percent)
        );
    }

    private void ResetMapZoom()
    {
        if (mapContent == null)
        {
            return;
        }

        // devolvemos el mapa a su escala normal
        mapContent.style.scale = new Scale(new Vector3(1f, 1f, 1f));

        // y a su posicion original
        mapContent.style.translate = new Translate(
            new Length(0, LengthUnit.Pixel),
            new Length(0, LengthUnit.Pixel)
        );
    }

    private void ZoomToVillageCenter(VisualElement village)
    {
        if (mapViewport == null || mapContent == null || villagesContainer == null || village == null)
        {
            return;
        }

        // rect del pueblo dentro del contenedor villages
        Rect villageRect = village.layout;

        // rect del contenedor villages dentro de mapcontent
        Rect villagesRect = villagesContainer.layout;

        // calculamos el centro del pueblo en coordenadas locales de mapcontent
        float villageCenterX = villagesRect.x + villageRect.x + (villageRect.width * 0.5f);
        float villageCenterY = villagesRect.y + villageRect.y + (villageRect.height * 0.5f);

        // punto visible del viewport al que queremos llevar el pueblo
        // x al centro
        // y algo mas abajo para que quede mejor encuadrado con la ui de la derecha
        float viewportCenterX = mapViewport.resolvedStyle.width * 0.5f;
        float viewportCenterY = mapViewport.resolvedStyle.height * 0.7f;

        // aplicamos zoom al mapa
        mapContent.style.scale = new Scale(new Vector3(selectedZoom, selectedZoom, 1f));

        // calculamos la traslacion necesaria para llevar el centro del pueblo
        // al punto visible deseado del viewport
        float translateX = viewportCenterX - (villageCenterX * selectedZoom);
        float translateY = viewportCenterY - (villageCenterY * selectedZoom);

        mapContent.style.translate = new Translate(
            new Length(translateX, LengthUnit.Pixel),
            new Length(translateY, LengthUnit.Pixel)
        );
    }

    private string FormatVillageName(string rawName)
    {
        // convierte nombres tipo mossdeep_city en mossdeep city
        if (string.IsNullOrEmpty(rawName))
        {
            return string.Empty;
        }

        return rawName.Replace("_", " ");
    }
}