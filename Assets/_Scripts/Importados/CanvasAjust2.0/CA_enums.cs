public enum ChildPivoAling
{
    A,
    B,
    C
}
/// <summary>
/// A disposição dos filhos
/// </summary>
public enum AlingType
{
    /// <summary>
    /// A estrermidade esquerda/Cima para a direita/baixo
    /// </summary>
    A,
    /// <summary>
    /// No centro para os lados/vertical
    /// </summary>
    B,
    /// <summary>
    /// Na estremidade direita/baixo para a esquerda/cima
    /// </summary>
    C
}
/// <summary>
/// maneiras de guardar e fazer os filhos caberem no CA
/// </summary>
public enum fitType
{
    nenhum,
    somenteDentro,
    /// <summary>
    /// se o CA_item tiver um "image/RawImage" ele fará um fadeout, porem se ele não tiver, apenas irá desativar
    /// </summary>
    fadeParaOsDefora
}