using System.Text.Json;
using PKHeX.Core;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/", (ReqBody body) =>
{
  var loader = new PokemonDataLoader(body.file);
  var pkm = loader.LoadAs(body.extension);
  var data = new PokemonResponseData();

  var gameStrings = GameInfo.Strings;
  var summary = new PokemonSummary(pkm, gameStrings);

  data.nickname = pkm.Nickname;
  data.species = pkm.Species;
  data.heldItem = summary.HeldItem;
  data.gender = summary.Gender;
  data.nature = summary.Nature;
  data.ability = summary.Ability;
  data.isEgg = pkm.IsEgg;
  data.moves = [
    summary.Move1,
    summary.Move2,
    summary.Move3,
    summary.Move4,
  ];
  data.level = pkm.CurrentLevel;
  data.hp = summary.HP;
  data.atk = summary.ATK;
  data.def = summary.DEF;
  data.spa = summary.SPA;
  data.spd = summary.SPD;
  data.spe = summary.SPE;
  data.isShiny = pkm.IsShiny;

  return JsonSerializer.Serialize(data);
});

app.Run();

record ReqBody(string file, string extension);

class PokemonDataLoader
{
  byte[] file;

  public PokemonDataLoader(string base64)
  {
    this.file = Convert.FromBase64String(base64);
  }

  public PKM LoadAs(string extension)
  {
    switch (extension)
    {
      case "pk9":
        return new PK9(this.file);
      case "pk8":
        return new PK8(this.file);
      case "pk7":
        return new PK7(this.file);
      case "pk6":
        return new PK6(this.file);
      case "pk5":
        return new PK5(this.file);
      case "pk4":
        return new PK4(this.file);
      case "pk3":
        return new PK3(this.file);
      case "pk2":
        return new PK2(this.file);
      case "pk1":
        return new PK1(this.file);
      default:
        throw new Exception("Unknown file type provided, could not convert!");
    }
  }
}

class PokemonResponseData
{
  // Usually the species name, unless renamed, in which case
  // it would be the Pokémon Nickname.
  public string nickname { get; set; }

  // Pokémon species number in the national dex.
  public int species { get; set; }

  // Name of the Held Item.
  public string heldItem { get; set; }

  // 'M' for Male, 'F' for Female and '-' for neither.
  public string gender { get; set; }

  // Name of the Pokémon's Nature.
  public string nature { get; set; }

  // Name of the Pokémon's Ability.
  public string ability { get; set; }

  // If Pokémon is an egg or not.
  public bool isEgg { get; set; }

  // An array with the names of the Pokémon's 
  // learned moves.
  public string[] moves { get; set; }

  // Pokémon's current level.
  public int level { get; set; }

  // Pokémon's max HP.
  public int hp { get; set; }

  // Pokémon's ATK stat.
  public int atk { get; set; }

  // Pokémon's DEF stat.
  public int def { get; set; }

  // Pokémon's SPA stat.
  public int spa { get; set; }

  // Pokémon's SPD stat.
  public int spd { get; set; }

  // Pokémon's SPE stat.
  public int spe { get; set; }

  // If Pokémon is shiny or not.
  public bool isShiny { get; set; }
}

// Simplified copy of EntitySummary
public class PokemonSummary
{
  private readonly GameStrings Strings;
  private readonly ushort[] Stats;
  protected readonly PKM pk;

  public PokemonSummary(PKM p, GameStrings strings)
  {
    pk = p;
    Strings = strings;
    Stats = pk.GetStats(pk.PersonalInfo);
  }

  public string HeldItem => GetSpan(Strings.GetItemStrings(pk.Context), pk.HeldItem);
  public string Gender => Get(GameInfo.GenderSymbolASCII, pk.Gender);
  public string Nature => Get(Strings.natures, pk.StatNature);
  public string Ability => Get(Strings.abilitylist, pk.Ability);
  public string Move1 => Get(Strings.movelist, pk.Move1);
  public string Move2 => Get(Strings.movelist, pk.Move2);
  public string Move3 => Get(Strings.movelist, pk.Move3);
  public string Move4 => Get(Strings.movelist, pk.Move4);
  public int HP => Stats[0];
  public int ATK => Stats[1];
  public int DEF => Stats[2];
  public int SPA => Stats[4];
  public int SPD => Stats[5];
  public int SPE => Stats[3];
  private static string Get(IReadOnlyList<string> arr, int val) => (uint)val < arr.Count ? arr[val] : string.Empty;
  private static string GetSpan(ReadOnlySpan<string> arr, int val) => (uint)val < arr.Length ? arr[val] : string.Empty;
}