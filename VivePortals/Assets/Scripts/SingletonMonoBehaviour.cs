using UnityEngine;
/// <summary>
/// Singleton implementation of MonoBehavior.
/// </summary>
/// <typeparam name="TSelf">Type that should point to the class that derives from SingletonMonobehaviour.</typeparam>
public abstract class SingletonMonoBehaviour<TSelf> : MonoBehaviour where TSelf : SingletonMonoBehaviour<TSelf>
{
    private static TSelf instance;
    /// <summary>
    /// Singleton instance of this component.
    /// </summary>
    public static TSelf Instance
    {
        get
        {
            // Use static function to define self, because "this" is non-static
            if (instance == null)
            {
                instance = FindObjectOfType<TSelf>();
            }
            return instance;
        }
    }
    // First Unity method that gets called in the activity stack
    void Awake()
    {
        // If singleton instance does not exist, define self as the instance
        if (instance == null)
        {
            instance = this as TSelf;
        }
        // Else if the instance then exists and is not this, destroy self (to maintain singleton)
        else if (instance != this)
        {
            Destroy(this);
        }
    }
}
