/// <summary>
/// チュートリアルヒントの定義（ID + 文言）。
/// ID は PlayerPrefs の既読キーとして使うため、一度リリースしたら変更しないこと。
/// </summary>
public class TutorialHint
{
    public readonly string Id;
    public readonly string Message;

    public TutorialHint(string id, string message)
    {
        Id = id;
        Message = message;
    }
}

public static class TutorialHints
{
    public static readonly TutorialHint Intro = new TutorialHint(
        "intro",
        "敵をゴールに到達させないように防衛しよう！\n空き地をクリックしてタワーを建てる。\n敵を倒すとお金が貯まり、新しいタワーが建てられる。");

    public static readonly TutorialHint Hero = new TutorialHint(
        "hero",
        "左下のヒーローボタンを押してから地面をクリックすると、\nヒーローを移動できる。");

    public static readonly TutorialHint GuardianMove = new TutorialHint(
        "guardian_move",
        "兵士タワーは「移動」ボタンで兵士の配置場所を変えられる。\n道を塞いで敵を足止めしよう。");

    public static readonly TutorialHint Flying = new TutorialHint(
        "flying",
        "飛行する敵が近づいている！\n攻撃できるタワーは限られているので注意。");

    public static readonly TutorialHint Fusion = new TutorialHint(
        "fusion",
        "タワーをドラッグして別のタワーに重ねると合成できる。\n緑色に光ったら合成可能のサイン。");

    public static readonly TutorialHint Skill = new TutorialHint(
        "skill",
        "このタワーはスキルを購入して強化できる。");

    public static readonly TutorialHint[] All =
    {
        Intro, Hero, GuardianMove, Flying, Fusion, Skill,
    };
}
