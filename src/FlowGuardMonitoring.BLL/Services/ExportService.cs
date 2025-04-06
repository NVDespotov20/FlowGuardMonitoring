using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlowGuardMonitoring.BLL.Models;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FlowGuardMonitoring.BLL.Services;

public class ExportService
{
    private readonly StatisticsService statisticsService;

    public ExportService(StatisticsService statisticsService)
    {
        this.statisticsService = statisticsService;
    }

    public byte[] GeneratePdf(
        string sensorName,
        DateTime startDate,
        DateTime endDate,
        List<MeasurementExportViewModel> data)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var statistics = this.statisticsService.CalculateStatistics(data);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(container =>
                    ComposeContent(container, sensorName, startDate, endDate, data, statistics));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                    x.Span($" - Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                });
            });
        });

        using var memoryStream = new MemoryStream();
        document.GeneratePdf(memoryStream);
        return memoryStream.ToArray();

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"FlowGuard Monitoring").Bold().FontSize(14);
                    column.Item().Text($"Sensor Measurements Report").FontSize(12);
                    column.Item().Height(5);
                });
            });
        }

        void ComposeContent(
            IContainer container,
            string name,
            DateTime start,
            DateTime end,
            List<MeasurementExportViewModel> measurements,
            MeasurementStatistics stats)
        {
            container.Column(column =>
            {
                // Report details
                column.Item().PaddingBottom(10).Text(text =>
                {
                    text.Span("Sensor: ").Bold();
                    text.Span(name);
                    text.Span(" | Period: ").Bold();
                    text.Span($"{start:yyyy-MM-dd HH:mm} to {end:yyyy-MM-dd HH:mm}");
                });

                // Statistics section
                column.Item().PaddingBottom(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Latest Value").Bold();
                        header.Cell().Text("Average").Bold();
                        header.Cell().Text("Min").Bold();
                        header.Cell().Text("Max").Bold();
                    });

                    table.Cell().Text(stats.Latest.ToString("F2"));
                    table.Cell().Text(stats.Average.ToString("F2"));
                    table.Cell().Text(stats.Min.ToString("F2"));
                    table.Cell().Text(stats.Max.ToString("F2"));
                });

                // Chart visualization note
                column.Item().PaddingBottom(10)
                    .Text("Note: For detailed chart visualization, please refer to the web interface or Excel export.")
                    .Italic();

                // Measurements table
                column.Item().Table(table =>
                {
                    // Define columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                    });

                    // Add header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Timestamp").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Value").Bold();
                    });

                    // Add data rows (limited to prevent huge PDF files)
                    var displayMeasurements = measurements.Count > 1000
                        ? measurements.Where((x, i) => i % ((measurements.Count / 1000) + 1) == 0).ToList()
                        : measurements;

                    foreach (var measurement in displayMeasurements)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(2)
                            .Text(measurement.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(2)
                            .Text(measurement.Value);
                    }

                    if (measurements.Count > 1000)
                    {
                        table.Cell().ColumnSpan(2).Padding(5).AlignCenter()
                            .Text($"Note: Showing {displayMeasurements.Count} of {measurements.Count} records")
                            .Italic();
                    }
                });
            });
        }
    }

    public byte[] GenerateExcel(
        string sensorName,
        DateTime startDate,
        DateTime endDate,
        List<MeasurementExportViewModel> data)
    {
        ExcelPackage.License.SetNonCommercialPersonal("Nikolay Despotov");
        using var package = new ExcelPackage();

        // Add a worksheet for the data
        var worksheet = package.Workbook.Worksheets.Add("Sensor Data");

        // Style for headers
        var headerStyle = worksheet.Workbook.Styles.CreateNamedStyle("HeaderStyle");
        headerStyle.Style.Font.Bold = true;
        headerStyle.Style.Font.Size = 12;
        headerStyle.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        headerStyle.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        headerStyle.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
        headerStyle.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        headerStyle.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

        // Style for title
        var titleStyle = worksheet.Workbook.Styles.CreateNamedStyle("TitleStyle");
        titleStyle.Style.Font.Bold = true;
        titleStyle.Style.Font.Size = 14;
        titleStyle.Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
        titleStyle.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        // Style for data cells
        var dataStyle = worksheet.Workbook.Styles.CreateNamedStyle("DataStyle");
        dataStyle.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        dataStyle.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        dataStyle.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        dataStyle.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        dataStyle.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        dataStyle.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

        // Style for statistics section
        var statsHeaderStyle = worksheet.Workbook.Styles.CreateNamedStyle("StatsHeaderStyle");
        statsHeaderStyle.Style.Font.Bold = true;
        statsHeaderStyle.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        statsHeaderStyle.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        statsHeaderStyle.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

        // Add title and metadata
        worksheet.Cells[1, 5].Value = "FlowGuard Sensor Measurements";
        worksheet.Cells[1, 5].StyleName = "TitleStyle";
        worksheet.Cells[1, 5, 1, 6].Merge = true;

        // Add metadata with styled cells
        worksheet.Cells[2, 5].Value = "Sensor:";
        worksheet.Cells[2, 5].Style.Font.Bold = true;
        worksheet.Cells[2, 6].Value = sensorName;

        worksheet.Cells[3, 5].Value = "Period:";
        worksheet.Cells[3, 5].Style.Font.Bold = true;
        worksheet.Cells[3, 6].Value = $"{startDate:yyyy-MM-dd HH:mm} to {endDate:yyyy-MM-dd HH:mm}";

        worksheet.Cells[4, 5].Value = "Generated:";
        worksheet.Cells[4, 5].Style.Font.Bold = true;
        worksheet.Cells[4, 6].Value = DateTime.Now;
        worksheet.Cells[4, 6].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

        // Add border to metadata section
        var metadataRange = worksheet.Cells[2, 5, 4, 6];
        metadataRange.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);

        // Calculate statistics
        var statistics = this.statisticsService.CalculateStatistics(data);

        // Add statistics with styled cells
        worksheet.Cells[6, 5].Value = "Statistics";
        worksheet.Cells[6, 5].StyleName = "StatsHeaderStyle";
        worksheet.Cells[6, 5, 6, 6].Merge = true;

        worksheet.Cells[7, 5].Value = "Latest Value";
        worksheet.Cells[7, 5].Style.Font.Bold = true;
        worksheet.Cells[7, 6].Value = statistics.Latest;

        worksheet.Cells[8, 5].Value = "Average";
        worksheet.Cells[8, 5].Style.Font.Bold = true;
        worksheet.Cells[8, 6].Value = statistics.Average;
        worksheet.Cells[8, 6].Style.Numberformat.Format = "0.00";

        worksheet.Cells[9, 5].Value = "Min";
        worksheet.Cells[9, 5].Style.Font.Bold = true;
        worksheet.Cells[9, 6].Value = statistics.Min;
        worksheet.Cells[9, 6].Style.Numberformat.Format = "0.00";

        worksheet.Cells[10, 5].Value = "Max";
        worksheet.Cells[10, 5].Style.Font.Bold = true;
        worksheet.Cells[10, 6].Value = statistics.Max;
        worksheet.Cells[10, 6].Style.Numberformat.Format = "0.00";

        // Add border to statistics section
        var statsRange = worksheet.Cells[7, 5, 10, 6];
        statsRange.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);

        // Headers for data table
        worksheet.Cells[1, 1].Value = "Timestamp";
        worksheet.Cells[1, 2].Value = "Value";
        worksheet.Cells[1, 3].Value = "Raw Value";
        worksheet.Cells[1, 1, 1, 3].StyleName = "HeaderStyle";

        // Populate data
        for (int i = 0; i < data.Count; i++)
        {
            worksheet.Cells[i + 2, 1].Value = data[i].Timestamp;
            worksheet.Cells[i + 2, 2].Value = data[i].Value;
            worksheet.Cells[i + 2, 3].Value = data[i].RawValue;
            worksheet.Cells[i + 2, 1, i + 2, 3].StyleName = "DataStyle";
        }

        // Format timestamp column
        worksheet.Column(1).Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

        // Format Value and RawValue columns (2 decimal places)
        worksheet.Cells[2, 2, data.Count + 1, 3].Style.Numberformat.Format = "0.00";

        // Alternate row colors for better readability
        for (int i = 2; i <= data.Count + 1; i += 2)
        {
            worksheet.Cells[i, 1, i, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[i, 1, i, 3].Style.Fill.BackgroundColor
                .SetColor(System.Drawing.Color.FromArgb(240, 240, 240));
        }

        // Auto-size columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Create daily averages for the chart
        var dailyAverages = data
            .GroupBy(m => m.Timestamp.Date)
            .Select(g => new
            {
                Date = g.Key,
                AverageValue = g.Average(m => m.RawValue),
            })
            .OrderBy(x => x.Date)
            .ToList();

        // Create a worksheet for daily averages
        var dailyWorksheet = package.Workbook.Worksheets.Add("Daily Averages");
        dailyWorksheet.Cells[1, 1].Value = "Date";
        dailyWorksheet.Cells[1, 2].Value = "Average Value";
        dailyWorksheet.Cells[1, 1, 1, 2].StyleName = "HeaderStyle";

        // Populate daily averages data
        for (int i = 0; i < dailyAverages.Count; i++)
        {
            dailyWorksheet.Cells[i + 2, 1].Value = dailyAverages[i].Date;
            dailyWorksheet.Cells[i + 2, 2].Value = dailyAverages[i].AverageValue;
            dailyWorksheet.Cells[i + 2, 1, i + 2, 2].StyleName = "DataStyle";
        }

        // Format date column
        dailyWorksheet.Column(1).Style.Numberformat.Format = "yyyy-mm-dd";

        // Format average value column (2 decimal places)
        dailyWorksheet.Cells[2, 2, dailyAverages.Count + 1, 2].Style.Numberformat.Format = "0.00";

        // Auto-size columns
        dailyWorksheet.Cells[dailyWorksheet.Dimension.Address].AutoFitColumns();

        // Create a chart for daily averages
        var chart = dailyWorksheet.Drawings.AddChart("DailyAveragesChart", eChartType.Line);
        var series = chart.Series.Add(
            dailyWorksheet.Cells[2, 2, dailyAverages.Count + 1, 2],
            dailyWorksheet.Cells[2, 1, dailyAverages.Count + 1, 1]);

        series.Header = "Daily Average";

        // Style the line chart
        chart.SetPosition(2, 0, 4, 0);
        chart.SetSize(800, 400);
        chart.Title.Text = $"{sensorName} - Daily Average Measurements";
        chart.Title.Font.Size = 14;
        chart.Title.Font.Bold = true;

        chart.XAxis.Title.Text = "Date";
        chart.XAxis.Title.Font.Size = 12;
        chart.XAxis.Title.Font.Bold = true;

        chart.YAxis.Title.Text = "Average Value";
        chart.YAxis.Title.Font.Size = 12;
        chart.YAxis.Title.Font.Bold = true;

        // Add the same chart to the main worksheet
        var mainChart = worksheet.Drawings.AddChart("MainDailyAveragesChart", eChartType.Line);
        var mainSeries = mainChart.Series.Add(
            dailyWorksheet.Cells[2, 2, dailyAverages.Count + 1, 2],
            dailyWorksheet.Cells[2, 1, dailyAverages.Count + 1, 1]);

        mainSeries.Header = "Daily Average";

        // Style the main worksheet chart
        mainChart.SetPosition(13, 0, 4, 0);
        mainChart.SetSize(800, 400);
        mainChart.Title.Text = $"{sensorName} - Daily Average Measurements";
        mainChart.Title.Font.Size = 14;
        mainChart.Title.Font.Bold = true;

        mainChart.XAxis.Title.Text = "Date";
        mainChart.XAxis.Title.Font.Size = 12;
        mainChart.XAxis.Title.Font.Bold = true;

        mainChart.YAxis.Title.Text = "Average Value";
        mainChart.YAxis.Title.Font.Size = 12;
        mainChart.YAxis.Title.Font.Bold = true;

        // Set the daily averages sheet as active
        package.Workbook.View.ActiveTab = 0;

        using var stream = new MemoryStream();
        package.SaveAs(stream);
        return stream.ToArray();
    }
}