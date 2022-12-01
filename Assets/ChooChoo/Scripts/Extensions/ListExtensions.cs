using System;
using System.Collections.Generic;

namespace ChooChoo
{
    public static class ListExtensions
    {
        private static Random rng = new Random();  

        public static void Shuffle<T>(this IList<T> list)  
        {  
            int listCount = list.Count;  
            while (listCount > 1) {  
                listCount--;  
                int index = rng.Next(listCount + 1);  
                (list[index], list[listCount]) = (list[listCount], list[index]);
            }  
        }
    }
}