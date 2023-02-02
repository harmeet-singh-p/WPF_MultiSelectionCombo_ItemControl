using DemoApp.Messages;
using DemoApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp.Helper
{
    internal class CSVReaderHalper
    {
        public async Task<List<Person>> ReadCSVFile(CSVReadRequestMessage csvReadRequestMessage, CancellationToken token)
        {
            try
            {
                var persons = new List<Person>();
                await Task.Run(() =>
                {
                    FileInfo fileName = new FileInfo(csvReadRequestMessage.FilePath);
                    using (var sr = fileName.OpenText())
                    {
                        string[] rows = sr.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        sr.Close();

                        int startingIndex = 0;

                        if (rows.Length > 0)
                        {
                            var columnResult = SplitOutsideQuotes(rows[0], csvReadRequestMessage.Seperator, token, false, false, true);
                            int columnLength = columnResult.Length;

                            if (csvReadRequestMessage.HasHeader)
                            {
                                startingIndex = 1;
                            }
                            for (int row = startingIndex; row < rows.Length; row++)
                            {
                                if (!token.IsCancellationRequested)
                                {
                                    var rowValues = SplitOutsideQuotes(rows[row], csvReadRequestMessage.Seperator, token, false, false, true);

                                    var person = new Person
                                    { 
                                        Name = rowValues[0],
                                        Country = rowValues[1],
                                        Address = rowValues[2],
                                        PostalZip = rowValues[3],
                                        Email = rowValues[4],
                                        Phone = rowValues[5]
                                    };
                                    persons.Add(person);
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                }
                            }
                        }
                    }

                }, token);
                return persons;
            }
            catch
            {
                throw;
            }
        }

        private string[] SplitOutsideQuotes(string source, char separator, CancellationToken token, bool trimSplits = true, bool ignoreEmptyResults = true, bool preserveEscapeCharInQuotes = true)
        {
            if (source == null)
                return null;

            var result = new List<string>();
            var escapeFlag = false;
            var quotesOpen = false;
            var currentItem = new StringBuilder();
            foreach (var currentChar in source)
            {
                if (!token.IsCancellationRequested)
                {
                    if (escapeFlag)
                    {
                        currentItem.Append(currentChar);
                        escapeFlag = false;
                        continue;
                    }

                    if (separator == currentChar && !quotesOpen)
                    {
                        var currentItemString = trimSplits ? currentItem.ToString().Trim() : currentItem.ToString();
                        currentItem.Clear();
                        if (string.IsNullOrEmpty(currentItemString) && ignoreEmptyResults) continue;
                        result.Add(currentItemString);
                        continue;
                    }

                    switch (currentChar)
                    {
                        default:
                            currentItem.Append(currentChar);
                            break;
                        case '\\':
                            if (quotesOpen && preserveEscapeCharInQuotes) currentItem.Append(currentChar);
                            escapeFlag = true;
                            break;
                        case '"':
                            currentItem.Append(currentChar);
                            quotesOpen = !quotesOpen;
                            break;
                    }
                }
                else
                {
                    token.ThrowIfCancellationRequested();
                }
            }

            if (escapeFlag) currentItem.Append("\\");

            var lastCurrentItemString = trimSplits ? currentItem.ToString().Trim() : currentItem.ToString();
            if (!(string.IsNullOrEmpty(lastCurrentItemString) && ignoreEmptyResults))
            {
                result.Add(lastCurrentItemString);
            }
            return result.ToArray();
        }
    }
}
