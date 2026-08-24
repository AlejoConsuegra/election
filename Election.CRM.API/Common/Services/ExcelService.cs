using ClosedXML.Excel;

namespace Election.CRM.API.Common.Services;

public class ExcelService
{
    /// <summary>
    /// Lee un archivo Excel y devuelve las filas como diccionarios
    /// </summary>
    public List<Dictionary<string, string>> LeerExcel(Stream archivoExcel)
    {
        var filas = new List<Dictionary<string, string>>();

        using var workbook = new XLWorkbook(archivoExcel);
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RowsUsed().Skip(1); // Saltar encabezado

        // Obtener nombres de columnas del encabezado
        var encabezados = worksheet.Row(1).Cells()
            .Select(c => c.GetString().Trim())
            .ToList();

        foreach (var row in rows)
        {
            var fila = new Dictionary<string, string>();
            
            for (int i = 0; i < encabezados.Count; i++)
            {
                var valor = row.Cell(i + 1).GetString().Trim();
                fila[encabezados[i]] = valor;
            }

            filas.Add(fila);
        }

        return filas;
    }
}