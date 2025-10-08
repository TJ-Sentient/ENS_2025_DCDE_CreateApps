using UnityEngine;

namespace TJ.Utils {
  public class Singleton < T >: MonoBehaviour where T: Component {
    private static T instance;

    public static T Instance => instance;

    public virtual void Awake() {
      if (instance == null) {
        instance = this as T;
        DontDestroyOnLoad(this.gameObject);
      } else {
        Destroy(gameObject);
      }
    }
  }
}