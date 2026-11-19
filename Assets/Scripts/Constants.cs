public static class Constants
{
    /// <summary>
    /// ゲーム内で使用するタグ名をまとめたクラス。
    /// TagManager に登録されている文字列と一致させてください。
    /// </summary>
    public static class Tags
    {
        public const string Enemy = "Enemy";
        public const string Hero = "Hero";
        public const string Clickable = "Clickable";
    }

    /// <summary>
    /// Animator パラメータ名をまとめたクラス。
    /// 各 AnimatorController に登録されているパラメータ名と一致させてください。
    /// （Hero と Guardian/Enemy で命名規則が異なるため、使用先をコメントで明記している）
    /// </summary>
    public static class AnimatorParams
    {
        // Hero
        public const string HeroIsDead = "IsDead";
        public const string HeroIsRunning = "IsRunning";
        public const string HeroAttack = "Attack";

        // Guardian / Enemy 共通
        public const string IsWalking = "isWalking";
        public const string IsAttacking = "isAttacking";
        public const string Die = "Die";

        // タワースキル
        public const string IsSkillActive = "isSkillActive";
    }
}
