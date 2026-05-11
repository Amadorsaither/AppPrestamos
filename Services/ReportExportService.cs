using AppPrestamos.ViewModels;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AppPrestamos.Services
{
    public class ReportExportService
    {
        public void ExportarPdf(string filePath, ReportesViewModel vm)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Reporte del Sistema de Préstamos")
                            .FontSize(20).Bold().FontColor(Colors.Blue.Medium);

                        col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Text("Resumen General").FontSize(14).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cd =>
                            {
                                cd.RelativeColumn();
                                cd.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Indicador").FontSize(10).Bold();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Valor").FontSize(10).Bold();
                            });

                            AddRow(table, "Total Clientes", vm.TotalClientes);
                            AddRow(table, "Total Préstamos", vm.TotalPrestamos);
                            AddRow(table, "Monto Desembolsado", vm.MontoDesembolsado);
                            AddRow(table, "Monto Cobrado", vm.MontoCobrado);
                            AddRow(table, "Saldo Pendiente", vm.SaldoPendiente);
                            AddRow(table, "Cuotas Vencidas", vm.ClientesEnMora);
                        });

                        col.Item().Text("Clientes en Mora").FontSize(14).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cd =>
                            {
                                cd.RelativeColumn(3);
                                cd.RelativeColumn();
                                cd.RelativeColumn();
                                cd.RelativeColumn();
                                cd.RelativeColumn();
                                cd.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Red.Medium).Padding(4).Text("Cliente").Style(headerStyle);
                                header.Cell().Background(Colors.Red.Medium).Padding(4).Text("Préstamo").Style(headerStyle);
                                header.Cell().Background(Colors.Red.Medium).Padding(4).Text("Cuota").Style(headerStyle);
                                header.Cell().Background(Colors.Red.Medium).Padding(4).Text("Monto").Style(headerStyle);
                                header.Cell().Background(Colors.Red.Medium).Padding(4).Text("Vencimiento").Style(headerStyle);
                                header.Cell().Background(Colors.Red.Medium).Padding(4).Text("Días").Style(headerStyle);
                            });

                            foreach (var c in vm.ClientesMora)
                            {
                                table.Cell().Padding(3).Text(c.Cliente).FontSize(8);
                                table.Cell().Padding(3).Text(c.PrestamoId.ToString()).FontSize(8);
                                table.Cell().Padding(3).Text(c.CuotaNumero.ToString()).FontSize(8);
                                table.Cell().Padding(3).Text($"${c.Monto:N2}").FontSize(8);
                                table.Cell().Padding(3).Text(c.Vencimiento).FontSize(8);
                                table.Cell().Padding(3).Text($"{c.DiasVencido}d").FontSize(8);
                            }
                        });

                        col.Item().Text("Resumen Mensual").FontSize(14).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cd =>
                            {
                                cd.RelativeColumn(2);
                                cd.RelativeColumn();
                                cd.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Medium).Padding(4).Text("Mes").Style(headerStyle);
                                header.Cell().Background(Colors.Blue.Medium).Padding(4).Text("Préstamos").Style(headerStyle);
                                header.Cell().Background(Colors.Blue.Medium).Padding(4).Text("Cobrado").Style(headerStyle);
                            });

                            foreach (var r in vm.ResumenAnual)
                            {
                                table.Cell().Padding(3).Text(r.Mes).FontSize(8);
                                table.Cell().Padding(3).Text(r.Prestamos.ToString()).FontSize(8);
                                table.Cell().Padding(3).Text($"${r.Cobrado:N2}").FontSize(8);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("AppPrestamos v1.0 — ").FontSize(8).FontColor(Colors.Grey.Medium);
                        t.CurrentPageNumber().FontSize(8);
                    });
                });
            }).GeneratePdf(filePath);
        }

        public void ExportarExcel(string filePath, ReportesViewModel vm)
        {
            using var workbook = new XLWorkbook();

            var hojaResumen = workbook.Worksheets.Add("Resumen");
            hojaResumen.Cell("A1").Value = "Reporte del Sistema de Préstamos";
            hojaResumen.Cell("A1").Style.Font.Bold = true;
            hojaResumen.Cell("A1").Style.Font.FontSize = 16;
            hojaResumen.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            hojaResumen.Cell("A2").Style.Font.FontSize = 10;

            hojaResumen.Cell("A4").Value = "Indicador";
            hojaResumen.Cell("B4").Value = "Valor";
            hojaResumen.Range("A4:B4").Style.Font.Bold = true;
            hojaResumen.Range("A4:B4").Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
            hojaResumen.Range("A4:B4").Style.Font.FontColor = XLColor.White;

            hojaResumen.Cell("A5").Value = "Total Clientes";
            hojaResumen.Cell("B5").Value = vm.TotalClientes;
            hojaResumen.Cell("A6").Value = "Total Préstamos";
            hojaResumen.Cell("B6").Value = vm.TotalPrestamos;
            hojaResumen.Cell("A7").Value = "Monto Desembolsado";
            hojaResumen.Cell("B7").Value = vm.MontoDesembolsado;
            hojaResumen.Cell("A8").Value = "Monto Cobrado";
            hojaResumen.Cell("B8").Value = vm.MontoCobrado;
            hojaResumen.Cell("A9").Value = "Saldo Pendiente";
            hojaResumen.Cell("B9").Value = vm.SaldoPendiente;
            hojaResumen.Cell("A10").Value = "Cuotas Vencidas";
            hojaResumen.Cell("B10").Value = vm.ClientesEnMora;

            hojaResumen.Columns().AdjustToContents();

            var hojaMora = workbook.Worksheets.Add("Clientes en Mora");
            hojaMora.Cell("A1").Value = "Cliente";
            hojaMora.Cell("B1").Value = "Préstamo";
            hojaMora.Cell("C1").Value = "Cuota";
            hojaMora.Cell("D1").Value = "Monto";
            hojaMora.Cell("E1").Value = "Vencimiento";
            hojaMora.Cell("F1").Value = "Días Vencido";
            hojaMora.Range("A1:F1").Style.Font.Bold = true;
            hojaMora.Range("A1:F1").Style.Fill.BackgroundColor = XLColor.Red;
            hojaMora.Range("A1:F1").Style.Font.FontColor = XLColor.White;

            int row = 2;
            foreach (var c in vm.ClientesMora)
            {
                hojaMora.Cell(row, 1).Value = c.Cliente;
                hojaMora.Cell(row, 2).Value = c.PrestamoId;
                hojaMora.Cell(row, 3).Value = c.CuotaNumero;
                hojaMora.Cell(row, 4).Value = c.Monto;
                hojaMora.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                hojaMora.Cell(row, 5).Value = c.Vencimiento;
                hojaMora.Cell(row, 6).Value = c.DiasVencido;
                row++;
            }

            hojaMora.Columns().AdjustToContents();

            var hojaMensual = workbook.Worksheets.Add("Resumen Mensual");
            hojaMensual.Cell("A1").Value = "Mes";
            hojaMensual.Cell("B1").Value = "Préstamos";
            hojaMensual.Cell("C1").Value = "Cobrado";
            hojaMensual.Range("A1:C1").Style.Font.Bold = true;
            hojaMensual.Range("A1:C1").Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
            hojaMensual.Range("A1:C1").Style.Font.FontColor = XLColor.White;

            row = 2;
            foreach (var r in vm.ResumenAnual)
            {
                hojaMensual.Cell(row, 1).Value = r.Mes;
                hojaMensual.Cell(row, 2).Value = r.Prestamos;
                hojaMensual.Cell(row, 3).Value = r.Cobrado;
                hojaMensual.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";
                row++;
            }

            hojaMensual.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }

        private static void AddRow(QuestPDF.Fluent.TableDescriptor table, string label, string value)
        {
            table.Cell().Padding(4).Text(label).FontSize(9);
            table.Cell().Padding(4).Text(value).FontSize(9).Bold();
        }
    }
}
