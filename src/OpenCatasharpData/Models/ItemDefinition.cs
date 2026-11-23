using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>Typed common properties shared by all CDDA ITEM definitions.</summary>
public sealed class ItemDefinition : CddaDefinition
{
    [JsonPropertyName("subtypes")]
    public List<string>? Subtypes { get; set; }

    [JsonPropertyName("name")]
    public CddaText? Name { get; set; }

    [JsonPropertyName("description")]
    public CddaText? Description { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("weight")]
    public CddaMass? Weight { get; set; }

    [JsonPropertyName("volume")]
    public CddaVolume? Volume { get; set; }

    [JsonPropertyName("price")]
    public CddaScalar? Price { get; set; }

    [JsonPropertyName("price_postapoc")]
    public CddaScalar? PostApocalypticPrice { get; set; }

    [JsonPropertyName("material")]
    [JsonConverter(typeof(CddaMaterialListConverter))]
    public List<CddaMaterialPart>? Material { get; set; }

    [JsonPropertyName("flags")]
    public List<string>? Flags { get; set; }

    [JsonPropertyName("qualities")]
    [JsonConverter(typeof(CddaQualitiesJsonConverter))]
    public List<CddaQuality>? Qualities { get; set; }

    // Shared so composite items retain all equipment facets as typed values.
    [JsonPropertyName("armor")]
    public List<CddaArmorSection>? Armor { get; set; }

    [JsonPropertyName("pocket_data")]
    public List<CddaPocket>? PocketData { get; set; }

    // Common properties intentionally belong to the base class: in CDDA an
    // ITEM may have multiple subtypes, or no declared subtype at all.
    [JsonPropertyName("looks_like")]
    public string? LooksLike { get; set; }

    [JsonPropertyName("longest_side")]
    public CddaLength? LongestSide { get; set; }

    [JsonPropertyName("barrel_length")]
    public CddaLength? BarrelLength { get; set; }

    [JsonPropertyName("display_type")]
    public string? DisplayType { get; set; }

    [JsonPropertyName("ascii_picture")]
    public string? AsciiPicture { get; set; }

    [JsonPropertyName("phase")]
    public string? Phase { get; set; }

    [JsonPropertyName("variant_type")]
    public string? VariantType { get; set; }

    [JsonPropertyName("variants")]
    public List<CddaItemVariant>? Variants { get; set; }

    [JsonPropertyName("melee_damage")]
    public CddaMeleeDamage? MeleeDamage { get; set; }

    [JsonPropertyName("to_hit")]
    public CddaToHit? ToHit { get; set; }

    [JsonPropertyName("use_action")]
    [JsonConverter(typeof(CddaUseActionsJsonConverter))]
    public List<CddaUseAction>? UseActions { get; set; }

    [JsonPropertyName("ranged_damage")]
    [JsonConverter(typeof(CddaDamageListJsonConverter))]
    public List<CddaDamage>? RangedDamage { get; set; }

    // Thrown weapons and explosive effects are shared across ITEM subtypes.
    [JsonPropertyName("thrown_damage")]
    [JsonConverter(typeof(CddaDamageListJsonConverter))]
    public List<CddaDamage>? ThrownDamage { get; set; }

    [JsonPropertyName("explode_in_fire")]
    public bool? ExplodeInFire { get; set; }

    [JsonPropertyName("explosion")]
    public CddaExplosion? Explosion { get; set; }

    [JsonPropertyName("weapon_category")]
    public List<string>? WeaponCategories { get; set; }

    [JsonPropertyName("conditional_names")]
    public List<CddaConditionalName>? ConditionalNames { get; set; }

    [JsonPropertyName("faults")]
    public List<CddaFault>? Faults { get; set; }

    [JsonPropertyName("tool_ammo")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? ToolAmmo { get; set; }

    [JsonPropertyName("charges")]
    public int? Charges { get; set; }

    [JsonPropertyName("environmental_protection")]
    public decimal? EnvironmentalProtection { get; set; }

    [JsonPropertyName("replace_materials")]
    public Dictionary<string, string>? ReplaceMaterials { get; set; }

    [JsonPropertyName("techniques")]
    public List<string>? Techniques { get; set; }

    [JsonPropertyName("revert_to")]
    public string? RevertTo { get; set; }

    [JsonPropertyName("tick_action")]
    [JsonConverter(typeof(CddaUseActionsJsonConverter))]
    public List<CddaUseAction>? TickActions { get; set; }

    [JsonPropertyName("drop_action")]
    [JsonConverter(typeof(CddaUseActionsJsonConverter))]
    public List<CddaUseAction>? DropActions { get; set; }

    [JsonPropertyName("power_draw")]
    public CddaPower? PowerDraw { get; set; }

    [JsonPropertyName("integral_volume")]
    public CddaVolume? IntegralVolume { get; set; }

    [JsonPropertyName("integral_weight")]
    public CddaMass? IntegralWeight { get; set; }

    [JsonPropertyName("min_skills")]
    [JsonConverter(typeof(CddaNamedAmountsConverter))]
    public List<CddaNamedAmount>? MinimumSkills { get; set; }

    [JsonPropertyName("sealed")]
    public bool? Sealed { get; set; }

    [JsonPropertyName("ammo_type")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? AmmoTypes { get; set; }
    // Remaining core ITEM metadata. These are source properties, so nullable
    // members remain null when omitted rather than applying game-side defaults.
    // Fields with mixed JSON shapes use dedicated eager converters.

    [JsonPropertyName("snippet_category")]
    public CddaSnippetCategory? SnippetCategory { get; set; }

    [JsonPropertyName("container")]
    public string? Container { get; set; }

    [JsonPropertyName("proficiencies")]
    public List<CddaItemProficiency>? Proficiencies { get; set; }

    [JsonPropertyName("countdown_interval")]
    public CddaDuration? CountdownInterval { get; set; }

    [JsonPropertyName("repairs_with")]
    public List<string>? RepairsWith { get; set; }

    [JsonPropertyName("light")]
    public decimal? Light { get; set; }

    [JsonPropertyName("charged_qualities")]
    [JsonConverter(typeof(CddaQualitiesJsonConverter))]
    public List<CddaQuality>? ChargedQualities { get; set; }

    [JsonPropertyName("countdown_action")]
    public CddaItemAction? CountdownAction { get; set; }

    [JsonPropertyName("martial_art")]
    public string? MartialArt { get; set; }

    // CDDA writes these as data-size strings (for example "128 MB").
    // Retain source units until binary/decimal data-size semantics are fixed.
    [JsonPropertyName("ememory_size")]
    public CddaScalar? ElectronicMemorySize { get; set; }

    [JsonPropertyName("etransfer_rate")]
    public CddaScalar? ElectronicTransferRate { get; set; }

    [JsonPropertyName("stackable")]
    public bool? Stackable { get; set; }

    [JsonPropertyName("environmental_protection_with_filter")]
    public decimal? FilterEnvironmentalProtection { get; set; }

    [JsonPropertyName("sub")]
    public string? Subcategory { get; set; }

    [JsonPropertyName("insulation")]
    public decimal? Insulation { get; set; }

    [JsonPropertyName("repairs_like")]
    public string? RepairsLike { get; set; }

    [JsonPropertyName("sight_dispersion")]
    public decimal? SightDispersion { get; set; }

    [JsonPropertyName("emits")]
    public List<string>? Emits { get; set; }

    [JsonPropertyName("expand_snippets")]
    public bool? ExpandSnippets { get; set; }

    // Arbitrary item-variable/property dictionaries preserve values without
    // inventing a homogeneous data type for independent mod-defined keys.
    [JsonPropertyName("variables")]
    public Dictionary<string, JsonElement>? Variables { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, JsonElement>? Properties { get; set; }

    [JsonPropertyName("generic")]
    public bool? Generic { get; set; }

    [JsonPropertyName("e_port")]
    public string? ElectronicPort { get; set; }

    [JsonPropertyName("consumption_per_use")]
    public List<CddaFiringCost>? ConsumptionPerUse { get; set; }

    [JsonPropertyName("source_monster")]
    public string? SourceMonster { get; set; }

    [JsonPropertyName("transform_into")]
    public CddaItemTransformation? TransformInto { get; set; }

    [JsonPropertyName("revert_msg")]
    public CddaText? RevertMessage { get; set; }

    [JsonPropertyName("drop")]
    public string? DropItem { get; set; }

    [JsonPropertyName("charge_factor")]
    public decimal? ChargeFactor { get; set; }

    [JsonPropertyName("nanofab_template_group")]
    public string? NanofabricatorTemplateGroup { get; set; }

    [JsonPropertyName("memory_card")]
    public CddaMemoryCard? MemoryCard { get; set; }

    [JsonPropertyName("e_ports_banned")]
    public List<string>? BannedElectronicPorts { get; set; }

    [JsonPropertyName("max_energy_shield_hp")]
    public decimal? MaximumEnergyShieldHitPoints { get; set; }

    [JsonPropertyName("template_requirements")]
    public string? TemplateRequirements { get; set; }

    [JsonPropertyName("fall_damage_reduction")]
    public decimal? FallDamageReduction { get; set; }

    [JsonPropertyName("weight_multiplier")]
    public decimal? WeightMultiplier { get; set; }

    [JsonPropertyName("min_str_required_mod_if_prone")]
    public decimal? MinimumStrengthModifierWhenProne { get; set; }

    [JsonPropertyName("fuel_efficiency")]
    public decimal? FuelEfficiency { get; set; }

    // Eagerly deserialised subtype components. Slots are populated once,
    // by the compositional handler, and never lazily parsed on access.
    [JsonIgnore]
    private Dictionary<Type, object>? _subtypeData;

    [JsonIgnore] public ArmorData? ArmorData => GetSubtype<ArmorData>();
    [JsonIgnore] public ComestibleData? ComestibleData => GetSubtype<ComestibleData>();
    [JsonIgnore] public ToolData? ToolData => GetSubtype<ToolData>();
    [JsonIgnore] public GunData? GunData => GetSubtype<GunData>();
    [JsonIgnore] public AmmoData? AmmoData => GetSubtype<AmmoData>();
    [JsonIgnore] public MagazineData? MagazineData => GetSubtype<MagazineData>();
    [JsonIgnore] public BookData? BookData => GetSubtype<BookData>();
    [JsonIgnore] public GunModData? GunModData => GetSubtype<GunModData>();
    [JsonIgnore] public BionicItemData? BionicItemData => GetSubtype<BionicItemData>();
    [JsonIgnore] public SeedData? SeedData => GetSubtype<SeedData>();
    [JsonIgnore] public BrewableData? BrewableData => GetSubtype<BrewableData>();
    [JsonIgnore] public CompostableData? CompostableData => GetSubtype<CompostableData>();
    [JsonIgnore] public MillingData? MillingData => GetSubtype<MillingData>();
    [JsonIgnore] public EngineData? EngineData => GetSubtype<EngineData>();
    [JsonIgnore] public WheelData? WheelData => GetSubtype<WheelData>();
    [JsonIgnore] public PetArmorData? PetArmorData => GetSubtype<PetArmorData>();
    [JsonIgnore] public ToolModData? ToolModData => GetSubtype<ToolModData>();
    [JsonIgnore] public ArtifactData? ArtifactData => GetSubtype<ArtifactData>();

    /// <summary>Get a built-in or custom typed component, without parsing JSON.</summary>
    public TSlot? GetSubtype<TSlot>() where TSlot : class =>
        _subtypeData is not null &&
        _subtypeData.TryGetValue(typeof(TSlot), out var value) ? (TSlot)value : null;

    /// <summary>Attach an already-deserialised slot from an independent handler.</summary>
    public void SetSubtype<TSlot>(TSlot slot) where TSlot : class
    {
        ArgumentNullException.ThrowIfNull(slot);
        (_subtypeData ??= new())[typeof(TSlot)] = slot;
    }


}

