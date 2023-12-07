using System;

namespace LethalExpansion.Utils
{
    internal static class ModUtils
    {
        public static T[] RemoveElementFromArray<T>(T[] originalArray, int indexToRemove)
        {
            if (indexToRemove < 0 || indexToRemove >= originalArray.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(indexToRemove));
            }

            T[] newArray = new T[originalArray.Length - 1];
            for (int i = 0, j = 0; i < originalArray.Length; i++)
            {
                if (i != indexToRemove)
                {
                    newArray[j] = originalArray[i];
                    j++;
                }
            }
            return newArray;
        }
    }
}
