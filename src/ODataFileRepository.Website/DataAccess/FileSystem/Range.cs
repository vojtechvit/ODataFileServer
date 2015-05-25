namespace ODataFileRepository.Website.DataAccess.FileSystem
{
    public struct Range
    {
        public Range(long from, long to)
        {
            From = from;
            To = to;
        }

        public long From { get; private set; }

        public long To { get; private set; }
    }
}