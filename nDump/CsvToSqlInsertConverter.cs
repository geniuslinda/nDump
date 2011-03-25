﻿using System.Text;

namespace nDump
{
    public class CsvToSqlInsertConverter : ICsvToSqlInsertConverter
    {
        private readonly int _numberOfRowsPerInsert;
        private readonly TokenJoiner _tokenJoiner;
        private readonly IEscapingStrategy _headerEscapingStrategy;
        private readonly IEscapingStrategy _valueEscapingStrategy;
        private const string InsertHeaderFormat = "insert {0} ({1}) values\n";
        private const string InsertFormat = "{0}({1})\n";
        private const string SetIdentityInsertFormatString = "set identity_insert {0} {1}\n";
        private const string Off = "off";
        private const string On = "on";

        public CsvToSqlInsertConverter(TokenJoiner tokenJoiner, IEscapingStrategy headerEscapingStrategy,
                                       IEscapingStrategy valueEscapingStrategy)
        {
            _tokenJoiner = tokenJoiner;
            _headerEscapingStrategy = headerEscapingStrategy;
            _valueEscapingStrategy = valueEscapingStrategy;
        }

        public CsvToSqlInsertConverter(int numberOfRowsPerInsert)
            : this(new TokenJoiner(), new ColumnHeaderKeywordEscapingStrategy(),
                   new ValueEscapingStrategy())
        {
            _numberOfRowsPerInsert = numberOfRowsPerInsert;
        }

        public void Convert(ICsvTable csvTable)
        {
            GenerateInserts(csvTable);
        }

        private string GenerateInserts(ICsvTable csvTable)
        {
            string insertHeader = string.Format(InsertHeaderFormat, _headerEscapingStrategy.Escape(csvTable.Name),
                                                _tokenJoiner.Join(
                                                    _headerEscapingStrategy.Escape(csvTable.GetColumnNames())));
            var builder = new StringBuilder();

            int i = 0;

            string separator = string.Empty;
            while (csvTable.ReadNextRow())
            {
                if (i%_numberOfRowsPerInsert == 0)
                {
                    EndFile(csvTable, builder);
                    builder = new StringBuilder();
                    TurnOnIdentityInsert(csvTable, builder);
                    builder.Append(insertHeader);
                    separator = string.Empty;
                }
                InsertRow(csvTable, builder, separator);
                separator = ",";
                i++;
            }
            EndFile(csvTable, builder);
            return builder.ToString();
        }

        private void EndFile(ICsvTable csvTable, StringBuilder builder)
        {
            TurnOffIdentityInsert(csvTable, builder);
            csvTable.Write(builder.ToString());
        }

        private void InsertRow(ICsvTable csvTable, StringBuilder builder, string separator)
        {
            string insertValues = _tokenJoiner.Join(_valueEscapingStrategy.Escape(csvTable.GetValues()));
            builder.AppendFormat(InsertFormat, separator, insertValues);
        }

        private void TurnOffIdentityInsert(ICsvTable csvTable, StringBuilder builder)
        {
            SetIdentityInsert(csvTable, builder, Off);
        }

        private void SetIdentityInsert(ICsvTable csvTable, StringBuilder builder, string value)
        {
            if (csvTable.HasIdentity)
            {
                builder.AppendFormat(SetIdentityInsertFormatString, csvTable.Name, value);
            }
        }

        private void TurnOnIdentityInsert(ICsvTable csvTable, StringBuilder builder)
        {
            SetIdentityInsert(csvTable, builder, On);
        }
    }
}