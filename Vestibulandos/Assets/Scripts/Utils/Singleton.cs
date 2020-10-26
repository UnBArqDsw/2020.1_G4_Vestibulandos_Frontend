public class Singleton<T> where T : new()
{
    private static T s_instance;

    //---------------------------------------------------------------------------------------------------
    public static T GetInstance()
    {
        if (s_instance == null)
            s_instance = new T();

        return s_instance;
    }

    //---------------------------------------------------------------------------------------------------
    public static void Destroy()
    {
        s_instance = default;
    }

    //---------------------------------------------------------------------------------------------------
    public static bool IsCreated()
    {
        return (s_instance != null);
    }
}