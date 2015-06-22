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

        public override int GetHashCode()
        {
            unchecked
            {
                return From.GetHashCode() ^ To.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Range))
            {
                return false;
            }

            return Equals((Range)obj);
        }

        public bool Equals(Range other)
        {
            return From == other.From && To == other.To;
        }

        public static bool operator ==(Range range1, Range range2)
        {
            return range1.Equals(range2);
        }

        public static bool operator !=(Range range1, Range range2)
        {
            return !range1.Equals(range2);
        }
    }
}