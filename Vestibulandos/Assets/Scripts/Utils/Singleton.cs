public class Singleton<T> where T: new()
{
    private static T s_instance = default;

    /// <summary>
    /// Obter atual instancia.
    /// </summary>
    /// <returns></returns>
    public static T GetInstance()
    {
        if (s_instance == null)
            s_instance = new T();

        return s_instance;
    }

    /// <summary>
    /// Destruir a instance.
    /// </summary>
    public static void Destroy()
    {
        // GC collection
        s_instance = default;
    }

    /// <summary>
    /// Verificar se a isntancia já foi criada.
    /// </summary>
    /// <returns></returns>
    public static bool IsCreated()
    {
        return s_instance != null;
    }
}
