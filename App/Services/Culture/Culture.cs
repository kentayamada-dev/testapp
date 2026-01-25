using System.Collections.Generic;

namespace App.Services.Culture;

public sealed class Culture(string code)
{
  public static readonly Culture En = new("en-US");
  public static readonly Culture Jp = new("ja-JP");

  private static readonly List<Culture> AllCultures = [En, Jp];

  public string Code { get; } = code;

  public static IReadOnlyList<Culture> GetAll()
  {
    return AllCultures;
  }
}
