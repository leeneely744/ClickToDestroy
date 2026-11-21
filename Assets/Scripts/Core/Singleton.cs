using UnityEngine;

/// <summary>
/// シーン単位のシングルトン MonoBehaviour の共通基底。
/// Instance の代入・重複破棄・OnDestroy でのクリアを一元化する。
/// 派生クラスは Awake の代わりに OnSingletonAwake をオーバーライドして初期化処理を書く。
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = (T)this;
        OnSingletonAwake();
    }

    /// <summary>このインスタンスが Instance として採用されたときに呼ばれる初期化フック。</summary>
    protected virtual void OnSingletonAwake() { }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
