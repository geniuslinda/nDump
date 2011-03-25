﻿using System.Collections.Generic;
using System.Data;
using FileHelpers;

namespace nDump
{
    public class SqlDataExporter
    {
        private const string DontCare = "blah";
        private readonly ConsoleLogger _logger;
        private readonly string _destinationDirectory;
        private readonly QueryExecutor _queryExecutor;
        private readonly ISelectionFilteringStrategy _selectionFilteringStrategy;

        public SqlDataExporter(ConsoleLogger logger, string destinationDirectory, QueryExecutor queryExecutor,
                               ISelectionFilteringStrategy selectionFilteringStrategy)
        {
            _logger = logger;
            _queryExecutor = queryExecutor;
            _selectionFilteringStrategy = selectionFilteringStrategy;
            _destinationDirectory = destinationDirectory;
        }

        
        public void ExportToCsv(List<SqlTableSelect> setupScripts, List<SqlTableSelect> selects)
        {
            try
            {
                _selectionFilteringStrategy.TearDownFilterTables(setupScripts);
            }
            catch (TearDownException)
            {
            }
            _selectionFilteringStrategy.SetupFilterTables(setupScripts);
            GenerateCsvs(selects);
            _selectionFilteringStrategy.TearDownFilterTables(setupScripts);
        }

        public void GenerateCsvs(List<SqlTableSelect> selects)
        {
            _logger.Log("Generating Csv:");

            foreach (var table in selects)
            {
                _logger.Log("     " + table.TableName);
                var select = _selectionFilteringStrategy.GetFilteredSelectStatement(table);
                DataTable results =
                    _queryExecutor.ExecuteSelectStatement(select);
                foreach (var column in table.ExcludedColumns)
                {
                    results.Columns.Remove(column);
                }

                var csvOptions = new CsvOptions(DontCare, ',', results.Columns.Count) {DateFormat = "g"};
                CsvEngine.DataTableToCsv(results, _destinationDirectory + table.TableName.ToLower() + ".csv", csvOptions);
            }
        }
    }
}