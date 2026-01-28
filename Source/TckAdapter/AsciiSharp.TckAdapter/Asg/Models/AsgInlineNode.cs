namespace AsciiSharp.TckAdapter.Asg.Models;

/// <summary>
/// インライン要素の基底クラス。
/// </summary>
/// <remarks>
/// インライン要素の Type は派生クラスで定義される。
/// 例: text は "string" となる。
/// </remarks>
public abstract class AsgInlineNode : AsgNode
{
}
