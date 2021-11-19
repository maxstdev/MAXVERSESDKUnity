using System;

[Serializable]
public struct RegexFilteringRule
{
    public string Comment;
    public string Pattern;
    public RegexFilteringOperation Operation;
}
