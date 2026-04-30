using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class HoennMapMenu : MonoBehaviour
{
    // documento ui del menu
    private UIDocument uiDocument;

    // elementos principales del uxml
    private VisualElement root;
    private VisualElement baseContainer;
    private VisualElement mapViewport;
    private VisualElement mapContent;
    private VisualElement villagesContainer;
    private VisualElement locationInfoContainer;
    private VisualElement rightTitles;
    private VisualElement returnButton;
    private VisualElement villageImage;

    // texto del pueblo en la ficha
    private Label villageLabel;

    // nombre del pueblo seleccionado
    private string selectedVillageName = string.Empty;

    // village seleccionado
    private VisualElement selectedVillageElement;

    // villages con hover ahora mismo
    private HashSet<VisualElement> hoveredVillages = new HashSet<VisualElement>();

    // trabajos activos del parpadeo
    private Dictionary<VisualElement, IVisualElementScheduledItem> blinkJobs =
        new Dictionary<VisualElement, IVisualElementScheduledItem>();

    [Header("map zoom")]
    // cuanto se acerca el mapa al seleccionar un pueblo
    [SerializeField] private float selectedZoom = 1.5f;

    [Header("transitions")]
    // tiempo del panel location info al entrar y salir
    [SerializeField] private float locationInfoTransitionTime = 0.18f;

    // tiempo del movimiento y zoom del mapa
    [SerializeField] private float mapTransitionTime = 0.22f;

    [Header("blink")]
    // velocidad del parpadeo al hacer hover
    [SerializeField] private float blinkSpeed = 0.12f;

    [Header("selected village")]
    // color y grosor del borde del seleccionado
    [SerializeField] private Color selectedBorderColor = Color.white;
    [SerializeField] private float selectedBorderWidth = 3f;

    [Header("scenes")]
    // escena a la que vuelve el boton return
    [SerializeField] private string returnSceneName = "MainMenuScene";

    private void OnEnable()
    {
        // buscamos el documento ui del objeto
        uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("No se encontro un UIDocument en este GameObject.");
            return;
        }

        // guardamos la raiz del arbol visual
        root = uiDocument.rootVisualElement;

        // configuracion inicial del menu
        CacheReferences();
        ConfigurePicking();
        ConfigureTransitions();
        RegisterCallbacks();
        BringPanelsToFront();

        // dejamos la ficha oculta y el mapa normal al entrar
        HideLocationInfo();
        ResetMapZoom();
        ClearVillageImage();
    }

    private void OnDisable()
    {
        // quitamos callbacks globales
        if (baseContainer != null)
        {
            baseContainer.UnregisterCallback<ClickEvent>(OnBaseClicked);
        }

        if (returnButton != null)
        {
            returnButton.UnregisterCallback<ClickEvent>(OnReturnClicked);
        }

        // quitamos callbacks de todos los villages
        if (villagesContainer != null)
        {
            foreach (VisualElement village in villagesContainer.Children())
            {
                village.UnregisterCallback<ClickEvent>(OnVillageClicked);
                village.UnregisterCallback<PointerEnterEvent>(OnVillagePointerEnter);
                village.UnregisterCallback<PointerLeaveEvent>(OnVillagePointerLeave);
            }
        }

        // paramos todos los parpadeos activos
        StopAllVillageBlink();
    }

    private void CacheReferences()
    {
        // buscamos todos los elementos que usaremos desde codigo
        baseContainer = root.Q<VisualElement>("Base");
        mapViewport = root.Q<VisualElement>("MapViewport");
        mapContent = root.Q<VisualElement>("MapContent");
        villagesContainer = root.Q<VisualElement>("Villages");
        locationInfoContainer = root.Q<VisualElement>("LocationInfo");
        rightTitles = root.Q<VisualElement>("RightTitles");
        returnButton = root.Q<VisualElement>("ReturnButton");
        villageLabel = root.Q<Label>("village");
        villageImage = root.Q<VisualElement>("villageImage");

        // mensajes utiles si algun name no coincide con el uxml
        if (baseContainer == null) Debug.LogError("No se encontro 'Base'.");
        if (mapViewport == null) Debug.LogError("No se encontro 'MapViewport'.");
        if (mapContent == null) Debug.LogError("No se encontro 'MapContent'.");
        if (villagesContainer == null) Debug.LogError("No se encontro 'Villages'.");
        if (locationInfoContainer == null) Debug.LogError("No se encontro 'LocationInfo'.");
        if (rightTitles == null) Debug.LogError("No se encontro 'RightTitles'.");
        if (returnButton == null) Debug.LogError("No se encontro 'ReturnButton'.");
        if (villageLabel == null) Debug.LogError("No se encontro el label 'village'.");
        if (villageImage == null) Debug.LogError("No se encontro 'villageImage'.");
    }

    private void ConfigurePicking()
    {
        // estos contenedores no deben bloquear input
        if (villagesContainer != null)
        {
            villagesContainer.pickingMode = PickingMode.Ignore;
        }

        if (locationInfoContainer != null)
        {
            locationInfoContainer.pickingMode = PickingMode.Ignore;
        }

        if (villageLabel != null)
        {
            villageLabel.pickingMode = PickingMode.Ignore;
        }

        if (villageImage != null)
        {
            villageImage.pickingMode = PickingMode.Ignore;
        }

        if (rightTitles != null)
        {
            rightTitles.pickingMode = PickingMode.Ignore;
        }

        // el boton return si debe poder pulsarse
        if (returnButton != null)
        {
            returnButton.pickingMode = PickingMode.Position;
        }
    }

    private void ConfigureTransitions()
    {
        // esta transicion mueve la ficha al mostrarse y ocultarse
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
                    new TimeValue(locationInfoTransitionTime, TimeUnit.Second)
                }
            );
        }

        // esta transicion suaviza el zoom y el desplazamiento del mapa
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
                    new TimeValue(mapTransitionTime, TimeUnit.Second),
                    new TimeValue(mapTransitionTime, TimeUnit.Second)
                }
            );
        }
    }

    private void RegisterCallbacks()
    {
        // click fuera de los pueblos
        if (baseContainer != null)
        {
            baseContainer.RegisterCallback<ClickEvent>(OnBaseClicked);
        }

        // click del boton return
        if (returnButton != null)
        {
            returnButton.RegisterCallback<ClickEvent>(OnReturnClicked);
        }

        if (villagesContainer == null)
        {
            return;
        }

        // cada pueblo tiene click y hover propios
        foreach (VisualElement village in villagesContainer.Children())
        {
            village.pickingMode = PickingMode.Position;
            village.RegisterCallback<ClickEvent>(OnVillageClicked);
            village.RegisterCallback<PointerEnterEvent>(OnVillagePointerEnter);
            village.RegisterCallback<PointerLeaveEvent>(OnVillagePointerLeave);

            // dejamos el estado base preparado
            ApplyVillageNormalStyle(village);
        }
    }

    private void BringPanelsToFront()
    {
        // estos elementos deben quedar por delante del mapa
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
        VisualElement clickedVillage = evt.currentTarget as VisualElement;

        if (clickedVillage == null)
        {
            return;
        }

        // evitamos que el click siga hasta base
        evt.StopPropagation();

        // si habia otro seleccionado, le quitamos su estado fijo
        if (selectedVillageElement != null && selectedVillageElement != clickedVillage)
        {
            ApplyVillageNormalStyle(selectedVillageElement);

            // si el raton sigue encima, vuelve a parpadear
            if (hoveredVillages.Contains(selectedVillageElement))
            {
                StartVillageBlink(selectedVillageElement);
            }
        }

        // guardamos la nueva seleccion
        selectedVillageElement = clickedVillage;
        selectedVillageName = FormatVillageName(clickedVillage.name);
        villageLabel.text = selectedVillageName;

        // actualizamos la imagen usando el nombre exacto del village
        UpdateVillageImage(clickedVillage.name);

        // el seleccionado no parpadea
        StopVillageBlink(clickedVillage);
        ApplyVillageSelectedStyle(clickedVillage);

        // mostramos la ficha y acercamos el mapa
        ShowLocationInfo();
        ZoomToVillageCenter(clickedVillage);
    }

    private void OnVillagePointerEnter(PointerEnterEvent evt)
    {
        VisualElement hoveredVillage = evt.currentTarget as VisualElement;

        if (hoveredVillage == null || villageLabel == null)
        {
            return;
        }

        // guardamos este village como hover activo
        hoveredVillages.Add(hoveredVillage);

        // si no esta seleccionado, parpadea
        if (hoveredVillage != selectedVillageElement)
        {
            StartVillageBlink(hoveredVillage);
        }

        // en hover mostramos su nombre
        villageLabel.text = FormatVillageName(hoveredVillage.name);

        // en hover cambiamos tambien la imagen
        UpdateVillageImage(hoveredVillage.name);
    }

    private void OnVillagePointerLeave(PointerLeaveEvent evt)
    {
        VisualElement hoveredVillage = evt.currentTarget as VisualElement;

        if (hoveredVillage == null || villageLabel == null)
        {
            return;
        }

        // deja de contar como hover
        hoveredVillages.Remove(hoveredVillage);

        // si no era el seleccionado, deja de parpadear
        if (hoveredVillage != selectedVillageElement)
        {
            StopVillageBlink(hoveredVillage);
        }

        // restauramos texto e imagen segun el estado actual
        if (!string.IsNullOrEmpty(selectedVillageName) && selectedVillageElement != null)
        {
            villageLabel.text = selectedVillageName;
            UpdateVillageImage(selectedVillageElement.name);
        }
        else
        {
            villageLabel.text = string.Empty;
            ClearVillageImage();
        }
    }

    private void ClearVillageImage()
    {
        if (villageImage == null)
        {
            return;
        }

        villageImage.style.backgroundImage = StyleKeyword.None;
    }

    private void OnBaseClicked(ClickEvent evt)
    {
        // quitamos la seleccion actual
        if (selectedVillageElement != null)
        {
            ApplyVillageNormalStyle(selectedVillageElement);

            // si sigue en hover, vuelve al estado de parpadeo
            if (hoveredVillages.Contains(selectedVillageElement))
            {
                StartVillageBlink(selectedVillageElement);
            }

            selectedVillageElement = null;
        }

        selectedVillageName = string.Empty;

        // limpiamos la imagen
        ClearVillageImage();

        // ocultamos ficha y reseteamos el mapa
        HideLocationInfo();
        ResetMapZoom();
    }

    private void OnReturnClicked(ClickEvent evt)
    {
        // evitamos que el click suba a base
        evt.StopPropagation();

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

        // translate mueve la ficha a su sitio visible
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

        villageLabel.text = string.Empty;

        // translate baja la ficha para ocultarla
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

        // dejamos el mapa sin zoom ni desplazamiento
        mapContent.style.scale = new Scale(new Vector3(1f, 1f, 1f));
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

        Rect villageRect = village.layout;
        Rect villagesRect = villagesContainer.layout;

        // centro del village dentro del mapa
        float villageCenterX = villagesRect.x + villageRect.x + (villageRect.width * 0.5f);
        float villageCenterY = villagesRect.y + villageRect.y + (villageRect.height * 0.5f);

        // punto del viewport donde queremos enfocar
        float viewportCenterX = mapViewport.resolvedStyle.width * 0.5f;
        float viewportCenterY = mapViewport.resolvedStyle.height * 0.7f;

        // el scale acerca el mapa
        mapContent.style.scale = new Scale(new Vector3(selectedZoom, selectedZoom, 1f));

        // el translate coloca el pueblo en la zona visible deseada
        float translateX = viewportCenterX - (villageCenterX * selectedZoom);
        float translateY = viewportCenterY - (villageCenterY * selectedZoom);

        mapContent.style.translate = new Translate(
            new Length(translateX, LengthUnit.Pixel),
            new Length(translateY, LengthUnit.Pixel)
        );
    }

    private void StartVillageBlink(VisualElement village)
    {
        if (village == null)
        {
            return;
        }

        // el seleccionado no parpadea
        if (village == selectedVillageElement)
        {
            return;
        }

        // si ya tiene un trabajo activo, no lo repetimos
        if (blinkJobs.ContainsKey(village))
        {
            return;
        }

        bool isBright = true;
        ApplyVillageBlinkState(village, isBright);

        // este trabajo alterna dos estados visuales
        // asi se consigue el efecto de parpadeo
        IVisualElementScheduledItem blinkJob = village.schedule.Execute(() =>
        {
            isBright = !isBright;
            ApplyVillageBlinkState(village, isBright);
        }).Every((long)(blinkSpeed * 1000f));

        blinkJobs[village] = blinkJob;
    }

    private void StopVillageBlink(VisualElement village)
    {
        if (village == null)
        {
            return;
        }

        // paramos el trabajo si existia
        if (blinkJobs.TryGetValue(village, out IVisualElementScheduledItem blinkJob))
        {
            blinkJob.Pause();
            blinkJobs.Remove(village);
        }

        // si no esta seleccionado, vuelve a su estado normal
        if (village != selectedVillageElement)
        {
            ApplyVillageNormalStyle(village);
        }
    }

    private void StopAllVillageBlink()
    {
        // paramos todos los trabajos activos
        foreach (KeyValuePair<VisualElement, IVisualElementScheduledItem> pair in blinkJobs)
        {
            pair.Value.Pause();

            if (pair.Key != null)
            {
                ApplyVillageNormalStyle(pair.Key);
            }
        }

        blinkJobs.Clear();
        hoveredVillages.Clear();
        selectedVillageElement = null;
    }

    private void ApplyVillageBlinkState(VisualElement village, bool isBright)
    {
        if (village == null)
        {
            return;
        }

        // alternamos entre un estado mas fuerte y otro mas suave
        if (isBright)
        {
            village.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 1f, 1f, 1f));
            village.style.opacity = 1f;
        }
        else
        {
            village.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 1f, 1f, 0.75f));
            village.style.opacity = 0.85f;
        }
    }

    private void ApplyVillageSelectedStyle(VisualElement village)
    {
        if (village == null)
        {
            return;
        }

        // estado fijo del seleccionado
        village.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 1f, 1f, 1f));
        village.style.opacity = 1f;
        village.style.scale = new Scale(new Vector3(1.08f, 1.08f, 1f));

        village.style.borderTopWidth = selectedBorderWidth;
        village.style.borderRightWidth = selectedBorderWidth;
        village.style.borderBottomWidth = selectedBorderWidth;
        village.style.borderLeftWidth = selectedBorderWidth;

        village.style.borderTopColor = new StyleColor(selectedBorderColor);
        village.style.borderRightColor = new StyleColor(selectedBorderColor);
        village.style.borderBottomColor = new StyleColor(selectedBorderColor);
        village.style.borderLeftColor = new StyleColor(selectedBorderColor);
    }

    private void ApplyVillageNormalStyle(VisualElement village)
    {
        if (village == null)
        {
            return;
        }

        // estado base sin borde ni animacion
        village.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 1f, 1f, 1f));
        village.style.opacity = 1f;
        village.style.scale = new Scale(new Vector3(1f, 1f, 1f));

        village.style.borderTopWidth = 0f;
        village.style.borderRightWidth = 0f;
        village.style.borderBottomWidth = 0f;
        village.style.borderLeftWidth = 0f;
    }

    private void UpdateVillageImage(string villageResourceName)
    {
        if (villageImage == null || string.IsNullOrEmpty(villageResourceName))
        {
            return;
        }

        // los cargamos como sprite
        Sprite sprite = Resources.Load<Sprite>("CityIMG/" + villageResourceName);

        if (sprite != null)
        {
            villageImage.style.backgroundImage = new StyleBackground(sprite);
            return;
        }

        Debug.LogWarning("no se encontro imagen para el village: " + villageResourceName);
    }

    private string FormatVillageName(string rawName)
    {
        // cambiamos guiones bajos por espacios
        if (string.IsNullOrEmpty(rawName))
        {
            return string.Empty;
        }

        return rawName.Replace("_", " ");
    }
}