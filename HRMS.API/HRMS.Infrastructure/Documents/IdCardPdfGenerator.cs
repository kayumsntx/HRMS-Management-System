using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.IdCards;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HRMS.Infrastructure.Documents;

public class IdCardPdfGenerator : IIdCardPdfGenerator
{
    static IdCardPdfGenerator()
    {
        // QuestPDF Community license (free for companies under the revenue limit stated on questpdf.com/license).
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateQrPng(string text)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        return new PngByteQRCode(data).GetGraphic(10);
    }

    public byte[] GeneratePdf(IdCardPdfModel m)
    {
        var qr = GenerateQrPng(m.QrText);

        return Document.Create(container =>
        {
            // ---------------- Front ----------------
            container.Page(page =>
            {
                page.Size(new PageSize(54, 85.6f, Unit.Millimetre));
                page.Margin(3, Unit.Millimetre);
                page.DefaultTextStyle(t => t.FontSize(7));

                page.Content().Column(col =>
                {
                    col.Spacing(2);

                    col.Item().AlignCenter().Text(m.CompanyName).Bold().FontSize(8);

                    if (m.PhotoBytes is { Length: > 0 })
                        col.Item().AlignCenter().Width(22, Unit.Millimetre).Height(26, Unit.Millimetre).Image(m.PhotoBytes).FitArea();
                    else
                        col.Item().AlignCenter().Width(22, Unit.Millimetre).Height(26, Unit.Millimetre).Border(0.5f)
                            .AlignMiddle().AlignCenter().Text("No photo");

                    col.Item().AlignCenter().Text(m.FullName).Bold().FontSize(9);
                    if (!string.IsNullOrWhiteSpace(m.DesignationName))
                        col.Item().AlignCenter().Text(m.DesignationName);

                    Line(col, "ID", m.EmployeeCode);
                    Line(col, "Dept", m.DepartmentName);
                    Line(col, "Blood", m.BloodGroup);
                    Line(col, "Joined", m.DateOfJoining.ToString("dd MMM yyyy"));
                    Line(col, "Valid till", m.ExpiryDate?.ToString("dd MMM yyyy"));
                });
            });

            // ---------------- Back ----------------
            container.Page(page =>
            {
                page.Size(new PageSize(54, 85.6f, Unit.Millimetre));
                page.Margin(3, Unit.Millimetre);
                page.DefaultTextStyle(t => t.FontSize(7));

                page.Content().Column(col =>
                {
                    col.Spacing(3);

                    col.Item().AlignCenter().Text("Card No: " + m.CardNo).Bold();
                    col.Item().AlignCenter().Width(30, Unit.Millimetre).Height(30, Unit.Millimetre).Image(qr).FitArea();

                    Line(col, "Mobile", m.MobileNumber);
                    Line(col, "Emergency", m.EmergencyNumber);

                    col.Item().AlignCenter().Text("This card is the property of " + m.CompanyName + ". If found, please return it to the HR department.")
                        .FontSize(6);
                });
            });
        }).GeneratePdf();
    }

    private static void Line(ColumnDescriptor col, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        col.Item().Row(row =>
        {
            row.ConstantItem(14, Unit.Millimetre).Text(label).Bold();
            row.RelativeItem().Text(value);
        });
    }
}
