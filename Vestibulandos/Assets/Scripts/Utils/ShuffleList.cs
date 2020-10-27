using System.Collections.Generic;

public abstract class ShuffleList
{
    public static List<E> ShuffleListItems<E>(List<E> inputList)
    {
        List<E> originalList = new List<E>();

        originalList.AddRange(inputList);
        List<E> randomList = new List<E>();

        System.Random r = new System.Random();

        int randomIndex = 0;

        while (originalList.Count > 0)
        {
            randomIndex = r.Next(0, originalList.Count);
            randomList.Add(originalList[randomIndex]);
            originalList.RemoveAt(randomIndex);
        }

        return randomList;
    }
}
