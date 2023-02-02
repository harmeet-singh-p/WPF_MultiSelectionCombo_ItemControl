namespace DemoApp.Messages
{
    public class CSVReadRequestMessage
    {
        public string FilePath { get; set; }

        public char Seperator { get; set; }

        public bool HasHeader { get; set; }

        public CSVReadRequestMessage(string filePath, char seperator, bool hasHeader)
        {
            FilePath = filePath;
            Seperator = seperator;
            HasHeader = hasHeader;
        }
    }
}
