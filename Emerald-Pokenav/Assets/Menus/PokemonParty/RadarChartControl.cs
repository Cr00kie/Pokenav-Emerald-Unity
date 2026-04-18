using UnityEngine;
using UnityEngine.UIElements;

public class RadarChartTraits : VisualElement.UxmlTraits{
    UxmlFloatAttributeDescription Cool = new UxmlFloatAttributeDescription { name = "Cool", defaultValue = 0.5f};
    UxmlFloatAttributeDescription Beauty = new UxmlFloatAttributeDescription { name = "Beauty", defaultValue = 0.5f};
    UxmlFloatAttributeDescription Cute = new UxmlFloatAttributeDescription { name = "Cute", defaultValue = 0.5f};
    UxmlFloatAttributeDescription Smart = new UxmlFloatAttributeDescription { name = "Smart", defaultValue = 0.5f};
    UxmlFloatAttributeDescription Tough = new UxmlFloatAttributeDescription { name = "Tough", defaultValue = 0.5f};

    public override void Init(VisualElement v, IUxmlAttributes bag, CreationContext cc)
    {
        base.Init(v, bag, cc);
        RadarChart radarChart = v as RadarChart;
        radarChart.SetStats(
            Cool.GetValueFromBag(bag,cc),
            Beauty.GetValueFromBag(bag,cc),
            Cute.GetValueFromBag(bag,cc),
            Smart.GetValueFromBag(bag,cc),
            Tough.GetValueFromBag(bag,cc)
            );
    }
}

public class RadarChart : VisualElement
{
    public new class UxmlFactory : UxmlFactory<RadarChart, RadarChartTraits> { }

    // Stats with range from 0 to 1, order: Cool, Beauty, Cute, Smart, Tough
    private float[] stats;
    private Color fillColor = new Color(0.2f, 0.9f, 0.3f, 0.6f);
    private Color outlineColor = new Color(0.36f, 0.36f, 0.36f, 1f);
    private Color pentagonColor = new Color(1f, 1f, 1f, 0.6f);
    private Color innerLinesColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    public RadarChart()
    {
        // Override generate visual content event
        generateVisualContent += OnGenerateVisualContent;
    }

    public void SetStats(float cool, float beauty, float cute, float smart, float tough)
    {
        // Update stats and mark to redraw visual element
        stats = new float[] { cool, beauty, cute, smart, tough };
        MarkDirtyRepaint();
    }

    private void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        float cx = contentRect.width / 2f;
        float cy = contentRect.height / 2f;
        float radius = Mathf.Min(cx, cy) * 0.75f;

        // Pentagon background
        DrawPolygon(ctx, cx, cy, radius, pentagonColor, new float[] { 1, 1, 1, 1, 1 });
        // Spoke lines from center to each vertex
        DrawInnerLines(ctx, cx, cy, radius);
        // Stat fill
        DrawPolygon(ctx, cx, cy, radius, fillColor, stats);
        // Pentagon outline lines
        DrawOutlineLine(ctx, cx, cy, radius);
    }

    // Draws a filled polygon where each vertex is scaled by statValues[i]
    private void DrawPolygon(MeshGenerationContext ctx, float cx, float cy, float radius, Color color, float[] scale)
    {
        int sides = 5;
        var painter = ctx.painter2D;
        painter.fillColor = color;
        painter.BeginPath();

        for (int i = 0; i < sides; i++)
        {
            float angle = GetAngle(i);
            float r = radius * Mathf.Clamp01(scale[i]);
            float x = cx + r * Mathf.Sin(angle);
            float y = cy - r * Mathf.Cos(angle);

            if (i == 0) painter.MoveTo(new Vector2(x, y));
            else painter.LineTo(new Vector2(x, y));
        }

        painter.ClosePath();
        painter.Fill();
    }

    // Draws the outer pentagon
    private void DrawOutlineLine(MeshGenerationContext ctx, float cx, float cy, float radius)
    {
        int sides = 5;
        var painter = ctx.painter2D;
        painter.strokeColor = outlineColor;
        painter.lineWidth = 10f;

        painter.BeginPath();
        for (int i = 0; i <= sides; i++)
        {
            float angle = GetAngle(i % sides);
            float r = radius;
            float x = cx + r * Mathf.Sin(angle);
            float y = cy - r * Mathf.Cos(angle);

            if (i == 0) painter.MoveTo(new Vector2(x, y));
            else painter.LineTo(new Vector2(x, y));
        }
        painter.Stroke();
    }

    private void DrawInnerLines(MeshGenerationContext ctx, float cx, float cy, float radius)
    {
        var painter = ctx.painter2D;
        painter.strokeColor = innerLinesColor;
        painter.lineWidth = 5f;

        for (int i = 0; i < 5; i++)
        {
            float angle = GetAngle(i);
            float x = cx + radius * Mathf.Sin(angle);
            float y = cy - radius * Mathf.Cos(angle);

            painter.BeginPath();
            painter.MoveTo(new Vector2(cx, cy));
            painter.LineTo(new Vector2(x, y));
            painter.Stroke();
        }
    }

    // Evenly spaces 5 points around a circle, starting from top
    private float GetAngle(int index)
    {
        return index * 2f * Mathf.PI / 5f;
    }
}