using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuButtonHoverBlink : MonoBehaviour
{

    [SerializeField]
    private float winkSpeed = 0.12f;

    // Diccionario para almacenar los trabajos de parpadeo activos para cada bot車n.
    private Dictionary<VisualElement, IVisualElementScheduledItem> blinkJobs =
        new Dictionary<VisualElement, IVisualElementScheduledItem>();

    private UIDocument uiDocument;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("No se encontr車 un UIDocument en este GameObject.");
            return;
        }

        RefreshButtons();
    }

    private void OnDisable()
    {
        foreach (KeyValuePair<VisualElement, IVisualElementScheduledItem> pair in blinkJobs)
        {
            pair.Value.Pause();
        }

        blinkJobs.Clear();
    }

    // M谷todo para refrescar los botones y asegurarse de que tengan los callbacks de hover configurados.
    public void RefreshButtons()
    {
        if (uiDocument == null)
        {
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        List<VisualElement> buttonList = root.Query<VisualElement>(className: "menu-button").ToList();

        for (int i = 0; i < buttonList.Count; i++)
        {
            VisualElement button = buttonList[i];

            button.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            button.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);

            button.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            button.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }
    }

    private void OnPointerEnter(PointerEnterEvent evt)
    {
        VisualElement button = evt.currentTarget as VisualElement;

        if (button == null)
        {
            return;
        }

        // Iniciamos el parpadeo del bot車n al entrar el cursor.
        StartBlink(button);
    }

    private void OnPointerLeave(PointerLeaveEvent evt)
    {
        VisualElement button = evt.currentTarget as VisualElement;

        if (button == null)
        {
            return;
        }

        // Detenemos el parpadeo del bot車n al salir el cursor.
        StopBlink(button);
    }

    private void StartBlink(VisualElement button)
    {
        StopBlink(button);

        BlinkState blinkState = new BlinkState();
        blinkState.IsBright = false;

        button.AddToClassList("menu-button-blink-a");
        button.RemoveFromClassList("menu-button-blink-b");

        // Programamos un trabajo que alternar芍 el estado de parpadeo cada winkSpeed segundos.
        IVisualElementScheduledItem scheduledItem = button.schedule.Execute(() =>
        {
            blinkState.IsBright = !blinkState.IsBright;

            if (blinkState.IsBright)
            {
                button.AddToClassList("menu-button-blink-a");
                button.RemoveFromClassList("menu-button-blink-b");
            }
            else
            {
                button.AddToClassList("menu-button-blink-b");
                button.RemoveFromClassList("menu-button-blink-a");
            }
        }).Every((long)(winkSpeed * 1000));

        blinkJobs[button] = scheduledItem;
    }

    private void StopBlink(VisualElement button)
    {
        IVisualElementScheduledItem scheduledItem;

        if (blinkJobs.TryGetValue(button, out scheduledItem))
        {
            scheduledItem.Pause();
            blinkJobs.Remove(button);
        }

        button.RemoveFromClassList("menu-button-blink-a");
        button.RemoveFromClassList("menu-button-blink-b");
    }

    private class BlinkState
    {
        public bool IsBright;
    }
}