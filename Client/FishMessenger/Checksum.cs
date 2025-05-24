using System.Text;

namespace ChecksumGen
{
    internal class Checksum
    {
        public Checksum()
        {

        }

        public int Generate(string value)
        {
            byte[] CV = Encoding.UTF8.GetBytes(value);
            int cs = 0;
            try
            {
                for (int i = 0; i < CV.Length; i++)
                {
                    cs += CV[i] ^ CV.Length;
                    cs++;
                }
                return cs;
            }
            catch
            {
                return 72252;
            }
        }
    }
}
