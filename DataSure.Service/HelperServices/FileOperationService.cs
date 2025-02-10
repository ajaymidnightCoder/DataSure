using DataSure.Contracts.HelperServices;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;

namespace DataSure.Service.HelperServices
{
    public class FileOperationService : IFileOperationService
    {

        public async Task<List<string>> GetCsvHeadersAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);
            var headerLine = await reader.ReadLineAsync();
            return headerLine?.Split(',').Select(h => h.Trim()).ToList() ?? new List<string>();
        }

        public async Task<List<string>> GetExcelHeadersAsync(Stream stream)
        {
            var headers = new List<string>();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var spreadsheetDocument = SpreadsheetDocument.Open(memoryStream, false);
            var workbookPart = spreadsheetDocument.WorkbookPart;
            var sheet = workbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);

            using var reader = OpenXmlReader.Create(worksheetPart);
            while (reader.Read())
            {
                if (reader.ElementType == typeof(Row))
                {
                    reader.ReadFirstChild();  // Move to the first cell in the row
                    do
                    {
                        if (reader.ElementType == typeof(Cell))
                        {
                            var cell = (Cell)reader.LoadCurrentElement();
                            headers.Add(GetCellValue(cell, workbookPart));
                        }
                    } while (reader.ReadNextSibling());  // Move to the next cell in the row

                    break;  // Exit after reading the first row
                }
            }
            return headers;
        }

        public DataTable ConvertToDataTable(Stream stream, string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                ".csv" => ConvertCsvToDataTable(stream),
                ".xlsx" => ConvertExcelToDataTable(stream),
                _ => throw new InvalidOperationException("Unsupported file format")
            };
        }

        public Stream DataTableToCsvStream(DataTable dataTable)
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
            {
                // Write column headers
                var columnNames = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                writer.WriteLine(string.Join(",", columnNames));

                // Write rows
                foreach (DataRow row in dataTable.Rows)
                {
                    var fields = row.ItemArray.Select(field => field.ToString().Replace(",", " ")); // Replace commas in data
                    writer.WriteLine(string.Join(",", fields));
                }
            }

            memoryStream.Position = 0; // Reset the position for reading
            return memoryStream;
        }

        public Stream DataTableToExcelStream(DataTable dataTable)
        {
            var memoryStream = new MemoryStream();

            using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook, true))
            {
                // Create Workbook and Worksheet
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" });

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add header row
                var headerRow = new Row();
                foreach (DataColumn column in dataTable.Columns)
                {
                    var cell = new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(column.ColumnName)
                    };
                    headerRow.AppendChild(cell);
                }
                sheetData.AppendChild(headerRow);

                // Add data rows
                foreach (DataRow row in dataTable.Rows)
                {
                    var dataRow = new Row();
                    foreach (var cellValue in row.ItemArray)
                    {
                        var cell = new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(cellValue.ToString())
                        };
                        dataRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(dataRow);
                }
            }

            memoryStream.Position = 0; // Reset position to the beginning for reading
            return memoryStream;
        }

        #region support methods

        public string GetCellValue(Cell cell, WorkbookPart workbookPart)
        {
            if (cell?.CellValue == null) return string.Empty;
            var value = cell.CellValue.InnerText;

            // Handle shared string table (for string values in Excel)
            return cell.DataType?.Value == CellValues.SharedString
                ? workbookPart.SharedStringTablePart.SharedStringTable.ChildElements[int.Parse(value)].InnerText
                : value;
        }

        private DataTable ConvertCsvToDataTable(Stream stream)
        {
            var dataTable = new DataTable();
            using var reader = new StreamReader(stream);
            var headers = reader.ReadLine()?.Split(',').Select(header => header.Trim()).ToList();

            if (headers == null) throw new Exception("CSV file is empty or has invalid headers.");
            headers.ForEach(header => dataTable.Columns.Add(header));

            while (!reader.EndOfStream)
            {
                var rows = reader.ReadLine()?.Split(',').Select(cell => cell.Trim()).ToArray();
                if (rows != null) dataTable.Rows.Add(rows);
            }
            return dataTable;
        }

        private DataTable ConvertExcelToDataTable(Stream stream)
        {
            var dataTable = new DataTable();
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            using var document = SpreadsheetDocument.Open(memoryStream, false);
            var workbookPart = document.WorkbookPart;
            var sheet = workbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
            var rows = worksheetPart.Worksheet.Descendants<Row>();

            bool isHeaderRow = true;
            foreach (var row in rows)
            {
                var dataRow = dataTable.NewRow();
                int cellIndex = 0;

                foreach (var cell in row.Elements<Cell>())
                {
                    var cellValue = GetCellValue(cell, workbookPart);
                    if (isHeaderRow)
                    {
                        dataTable.Columns.Add(cellValue ?? $"Column{cellIndex + 1}");
                    }
                    else
                    {
                        dataRow[cellIndex] = cellValue;
                    }
                    cellIndex++;
                }

                if (!isHeaderRow) dataTable.Rows.Add(dataRow);
                isHeaderRow = false;
            }

            return dataTable;
        }

        #endregion support methods - end

    }
}
