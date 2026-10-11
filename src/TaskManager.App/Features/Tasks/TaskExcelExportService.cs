using ClosedXML.Excel;
using TaskManager.App.Features.Tasks.Entities;

namespace TaskManager.App.Features.Tasks;

public sealed class TaskExcelExportService
{
    public byte[] Export(IEnumerable<WorkTask> tasks)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Tasks");

        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(1, 2).Value = "Title";
        sheet.Cell(1, 3).Value = "Company";
        sheet.Cell(1, 4).Value = "Project";
        sheet.Cell(1, 5).Value = "Priority";
        sheet.Cell(1, 6).Value = "Status";
        sheet.Cell(1, 7).Value = "DueDate";
        sheet.Cell(1, 8).Value = "PlannedHours";
        sheet.Cell(1, 9).Value = "ActualHours";
        sheet.Range(1, 1, 1, 9).Style.Font.Bold = true;

        var row = 2;
        foreach (var task in tasks)
        {
            sheet.Cell(row, 1).Value = task.Id;
            sheet.Cell(row, 2).Value = task.Title;
            sheet.Cell(row, 3).Value = task.Company?.Name;
            sheet.Cell(row, 4).Value = task.Project?.Name;
            sheet.Cell(row, 5).Value = task.Priority.ToString();
            sheet.Cell(row, 6).Value = task.Status.ToString();
            sheet.Cell(row, 7).Value = task.DueDate?.ToString("yyyy-MM-dd");
            sheet.Cell(row, 8).Value = task.PlannedHours;
            sheet.Cell(row, 9).Value = task.ActualHours;
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
