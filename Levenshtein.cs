using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpPhoneNumberParser
{
    /// <summary>
    /// Computes the Levenshtein Edit Distance between two enumerables.
    /// </summary>
    /// <TYPEPARAM name="T">The type of the items in the enumerables.</TYPEPARAM>
    /// <PARAM name="x">The first enumerable.</PARAM>
    /// <PARAM name="y">The second enumerable.</PARAM>
    /// <RETURNS>The edit distance.</RETURNS>
    class Levenshtein
    {
        public static int EditDistance<T>(IEnumerable<T> x, IEnumerable<T> y) where T: IEquatable<T>
        {
            //validate parameters
            if (x == null)
                throw new ArgumentNullException("x");
            if (y == null)
                throw new ArgumentNullException("y");

            //convert parameters into IList instances
            IList<T> first = x as IList<T> ?? new List<T>(x);
            IList<T> second = y as IList<T> ?? new List<T>(y);

            //get the length of both.  If either is 0, return the length of the other, since that number of
            //insertions would be required.
            int n = first.Count, m = second.Count;
            if (n == 0)
                return m;
            if (m == 0)
                return n;

            //Rather than maintain an entire matrix (which would require O(n*m) space),
            //just store the current row and the next row, each of which has a length of m+1,
            //so just O(m) space.  Initilize the current row.
            int currentRow = 0, nextRow = 1;
            int[][] rows = new int[][] { new int[m+1], new int[m+1] };
            for (int i = 0; i <= m; ++i)
                rows[currentRow][i] = i;

            //foreach virtual row (since we only have physical storage for two)
            for(int j = 1; j <= n; ++j)
            {
                //fill the values of the row
                rows[nextRow][0] = j;
                for(int k = 1; k <= m; ++k)
                {
                    int dist0 = rows[currentRow][k] + 1;
                    int dist1 = rows[nextRow][k - 1] + 1;
                    int dist2 = rows[currentRow][k - 1] + (first[j - 1].Equals(second[k - 1]) ? 0 : 1);

                    rows[nextRow][k] = Math.Min(dist0, Math.Min(dist1, dist2));
                }

                //swap the current and next rows
                if (currentRow == 0)
                {
                    currentRow = 1;
                    nextRow = 0;
                }
                else
                {
                    currentRow = 0;
                    nextRow = 1;
                }
            }

            //return the computed edit distance
            return rows[currentRow][m];
        }
    }
}
