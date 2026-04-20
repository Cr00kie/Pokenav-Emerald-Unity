using UnityEngine;
using UnityEngine.UIElements;

public class SearchResultMenu : MonoBehaviour
{
    [Header("Default Subtitle")]
    [SerializeField] private string defaultSubtitleText = "Party Pok¨¦mon";
    [SerializeField] private Color defaultSubtitleColor = Color.white;

    private VisualElement root;
    private VisualElement subtitleContainer;
    private Label subtitleLabel;

    private void OnEnable()
    {
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
            Debug.LogError("No se encontr¨® el Label 'SubtitleText'.");
            return;
        }

        ApplySubtitleData();
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